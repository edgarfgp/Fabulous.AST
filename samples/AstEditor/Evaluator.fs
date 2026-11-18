namespace AstEditor

open System
open System.IO
open System.Text
open System.Threading
open FSharp.Compiler.Interactive.Shell

/// Hosts in-process F# Interactive sessions with Fabulous.AST loaded: one evaluates the DSL
/// you type (`generate`), a second executes the generated F# (`run`). Keeping them apart
/// means user code that hangs or redefines things can't block or pollute generation.
module Evaluator =

    /// Assemblies the sessions `#r`. We point at the copies this app already loaded (so
    /// versions match exactly).
    let private referenceAssemblies =
        [ "Fabulous.AST"; "Fantomas.Core"; "Fantomas.FCS" ]

    /// Blank out `#r` / `#load` lines (we already reference the core assemblies, and pasted
    /// docs snippets carry stale relative paths). Lines are blanked, not removed, so
    /// diagnostic line numbers still match the editor.
    let blankDirectives(script: string) =
        script.Replace("\r\n", "\n").Split('\n')
        |> Array.map(fun l ->
            let t = l.TrimStart()

            if t.StartsWith "#r " || t.StartsWith "#load " then
                ""
            else
                l)
        |> String.concat "\n"

    // ---- Console capture -------------------------------------------------------------------
    // Console.Out is process-global, but generate and run can execute concurrently on
    // different threads. Install one router as Console.Out that forwards to a per-thread
    // capture buffer (FSI evaluates on the calling thread), so each call sees only its output.

    let private captureTarget = new ThreadLocal<TextWriter>()

    type private ConsoleRouter(fallback: TextWriter) =
        inherit TextWriter()

        member private _.Target =
            match captureTarget.Value with
            | null -> fallback
            | w -> w

        override _.Encoding = Encoding.UTF8
        override this.Write(c: char) = this.Target.Write c
        override this.Write(s: string) = this.Target.Write s
        override this.WriteLine(s: string) = this.Target.WriteLine s
        override this.Flush() = this.Target.Flush()

    let private installRouter =
        lazy
            (Console.SetOut(new ConsoleRouter(Console.Out))
             // The app never reads stdin; make generated `Console.ReadLine()` return null
             // instead of blocking on the terminal.
             Console.SetIn TextReader.Null)

    /// Run `f`, returning its result and everything it wrote to the console.
    let private capture(f: unit -> 'a) : 'a * string =
        installRouter.Force()
        let buffer = new StringWriter()
        captureTarget.Value <- buffer

        try
            let result = f()
            result, buffer.ToString()
        finally
            captureTarget.Value <- null

    // ---- Sessions --------------------------------------------------------------------------

    let private buildSession() =
        // FSI writes banners/prompts to these; we don't surface them, but it needs real streams.
        let inStream = new StringReader("")
        let outStream = new StringWriter()
        let errStream = new StringWriter()

        let config = FsiEvaluationSession.GetDefaultConfiguration()
        let argv = [| "fsi"; "--noninteractive"; "--nologo"; "--gui-" |]

        let fsi =
            FsiEvaluationSession.Create(config, argv, inStream, outStream, errStream, collectible = false)

        for name in referenceAssemblies do
            let dll = Path.Combine(AppContext.BaseDirectory, name + ".dll")

            if File.Exists dll then
                fsi.EvalInteractionNonThrowing($"#r @\"{dll}\"") |> ignore

        // Pre-open the DSL surface so a script can start straight at `Oak() { ... }`.
        fsi.EvalInteractionNonThrowing("open Fabulous.AST") |> ignore
        fsi.EvalInteractionNonThrowing("open type Fabulous.AST.Ast") |> ignore
        fsi

    // A session is not thread-safe, so each one is serialized through its own gate.
    let private generator = lazy buildSession()
    let private generatorGate = obj()
    let private runner = lazy buildSession()
    let private runnerGate = obj()

    let private formatDiagnostics (ex: exn) (diagnostics: FSharp.Compiler.Diagnostics.FSharpDiagnostic[]) =
        let diags =
            diagnostics
            |> Array.filter(fun d -> d.Severity = FSharp.Compiler.Diagnostics.FSharpDiagnosticSeverity.Error)
            |> Array.map(fun d -> $"({d.StartLine},{d.StartColumn}) {d.Message}")
            |> String.concat Environment.NewLine

        if String.IsNullOrWhiteSpace diags then
            ex.Message
        else
            diags

    /// Evaluate a Fabulous.AST DSL *script* and return the generated F# source.
    ///
    /// The script is a full F# fragment — opens, helpers, a `Rewrite` pass, whatever — ending
    /// in a string expression (typically `... |> Gen.run`); that value is the result. If the
    /// script prints the source instead (`|> printfn "%s"`), its console output is used.
    let generate(script: string) : Result<string, string> =
        lock generatorGate (fun () ->
            let fsi = generator.Value

            let (result, diagnostics), printed =
                capture(fun () ->
                    // Reset `it`: it only rebinds when the script ends in an expression, so
                    // otherwise a declaration-only script would surface the previous value.
                    fsi.EvalInteractionNonThrowing "()" |> ignore
                    fsi.EvalInteractionNonThrowing(blankDirectives script))

            match result with
            | Choice1Of2 _ ->
                match fsi.EvalExpressionNonThrowing "it" with
                | Choice1Of2(Some v), _ when (v.ReflectionValue :? string) -> Ok(v.ReflectionValue :?> string)
                | _ when not(String.IsNullOrWhiteSpace printed) -> Ok(printed.TrimEnd())
                | _ ->
                    Error
                        "The script ran but produced no F# source. End it with a string expression, e.g. `... |> Gen.run`."
            | Choice2Of2 ex -> Error(formatDiagnostics ex diagnostics))

    /// FSI evaluates an *interaction*, which can't begin with a file-level `namespace` or
    /// header-style `module Foo` (no `=`). Generated code often does, so blank that line —
    /// the declarations underneath aren't indented, so they run fine on their own.
    let private stripFileHeader(source: string) =
        let lines = source.Replace("\r\n", "\n").Split('\n')

        match lines |> Array.tryFindIndex(fun l -> l.Trim() <> "") with
        | Some i ->
            let line = lines[i].Trim()

            let isHeader =
                (line.StartsWith "namespace " || line = "namespace")
                || (line.StartsWith "module " && not(line.Contains "="))

            if isHeader then
                lines |> Array.mapi(fun j l -> if j = i then "" else l) |> String.concat "\n"
            else
                source
        | None -> source

    /// Compile and execute generated F# in the run session, capturing its console output.
    let run(fsharpSource: string) : Result<string, string> =
        lock runnerGate (fun () ->
            let fsi = runner.Value

            let (result, diagnostics), printed =
                capture(fun () -> fsi.EvalInteractionNonThrowing(stripFileHeader fsharpSource))

            match result with
            | Choice1Of2 _ ->
                if String.IsNullOrWhiteSpace printed then
                    Ok "The code compiled and ran, but printed nothing."
                else
                    Ok(printed.TrimEnd())
            | Choice2Of2 ex -> Error(formatDiagnostics ex diagnostics))
