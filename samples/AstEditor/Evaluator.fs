namespace AstEditor

open System
open System.IO
open System.Text
open FSharp.Compiler.Interactive.Shell

/// Hosts an in-process F# Interactive session that has Fabulous.AST loaded, so the
/// editor can evaluate the DSL the user types and hand back the generated F# source.
///
/// The session is created once and reused: spinning up FSI is expensive, but once it
/// is warm each evaluation is just a compile-and-run of the user's snippet.
module Evaluator =

    /// Assemblies we want the FSI session to `#r`. We point at the copies that this
    /// app already loaded (so versions match exactly) and let FSI resolve the rest
    /// from the same output directory.
    let private referenceAssemblies =
        [ "Fabulous.AST"
          "Fantomas.Core"
          "Fantomas.FCS" ]

    let private buildSession() =
        // FSI writes banners/prompts to these; we don't surface them, but it needs real streams.
        let sbOut = StringBuilder()
        let sbErr = StringBuilder()
        let inStream = new StringReader("")
        let outStream = new StringWriter(sbOut)
        let errStream = new StringWriter(sbErr)

        let config = FsiEvaluationSession.GetDefaultConfiguration()
        let argv = [| "fsi"; "--noninteractive"; "--nologo"; "--gui-" |]

        let fsi =
            FsiEvaluationSession.Create(config, argv, inStream, outStream, errStream, collectible = false)

        // Reference the assemblies straight off disk, next to this executable.
        let baseDir = AppContext.BaseDirectory

        for name in referenceAssemblies do
            let dll = Path.Combine(baseDir, name + ".dll")

            if File.Exists dll then
                fsi.EvalInteractionNonThrowing($"#r @\"{dll}\"") |> ignore

        // Pre-open the DSL surface so the user can write `Oak() { ... }` directly.
        fsi.EvalInteractionNonThrowing("open Fabulous.AST") |> ignore
        fsi.EvalInteractionNonThrowing("open type Fabulous.AST.Ast") |> ignore

        fsi

    let private session = lazy buildSession()

    // A single FsiEvaluationSession is not thread-safe, and both generate/run redirect the
    // process-global Console.Out. App dispatches them on thread-pool threads that can overlap,
    // so every session interaction is serialized through this gate.
    let private gate = obj()

    let private formatDiagnostics (ex: exn) (diagnostics: FSharp.Compiler.Diagnostics.FSharpDiagnostic[]) =
        let diags =
            diagnostics
            |> Array.map(fun d -> $"({d.StartLine},{d.StartColumn}) {d.Severity} {d.Message}")
            |> String.concat Environment.NewLine

        if String.IsNullOrWhiteSpace diags then ex.Message else diags

    /// We already `#r` the core assemblies, so drop any `#r`/`#load` the user pasted —
    /// otherwise stale relative paths (like the docs' `../../src/...`) break evaluation.
    let private stripDirectives (script: string) =
        script.Replace("\r\n", "\n").Split('\n')
        |> Array.filter(fun l ->
            let t = l.TrimStart()
            not(t.StartsWith "#r " || t.StartsWith "#load "))
        |> String.concat "\n"

    /// Evaluate a Fabulous.AST DSL *script* and return the generated F# source.
    ///
    /// The script is a full F# fragment — opens, helper functions, active patterns, a
    /// `Rewrite` pass, whatever — that ends in the generated source. End it with a string
    /// expression (typically `... |> Gen.run`); that trailing value is what we show. If
    /// the script instead prints the source (e.g. `|> Gen.run |> printfn "%s"`) we fall
    /// back to its console output.
    let generate (script: string) : Result<string, string> =
        lock gate (fun () ->
            let fsi = session.Value
            let script = stripDirectives script

            let captured = new StringWriter()
            let previous = Console.Out
            Console.SetOut captured

            try
                // Reset `it` first: it only rebinds when the script ends in an expression, so
                // without this a declaration-ending script would surface the *previous* run's
                // value. After the reset, a non-expression script leaves `it = ()` and falls
                // through to the stdout fallback below.
                fsi.EvalInteractionNonThrowing "()" |> ignore

                let result, diagnostics = fsi.EvalInteractionNonThrowing script

                match result with
                | Choice1Of2 _ ->
                    // The trailing expression binds to `it`; prefer it when it's the source string.
                    let value, _ = fsi.EvalExpressionNonThrowing "it"

                    match value with
                    | Choice1Of2(Some v) when (v.ReflectionValue :? string) -> Ok(v.ReflectionValue :?> string)
                    | _ ->
                        let text = captured.ToString()

                        if String.IsNullOrWhiteSpace text then
                            Error
                                "The script ran but produced no F# source. End it with a string expression, e.g. `... |> Gen.run`."
                        else
                            Ok(text.TrimEnd())
                | Choice2Of2 ex -> Error(formatDiagnostics ex diagnostics)
            finally
                Console.SetOut previous)

    /// FSI evaluates an *interaction*, which can't begin with a file-level `namespace` or
    /// header-style `module Foo` (no `=`). Generated code often does, so drop that opening
    /// line — the declarations underneath aren't indented, so they run fine on their own.
    let private stripFileHeader (source: string) =
        let lines = source.Replace("\r\n", "\n").Split('\n')
        let firstCode = lines |> Array.tryFindIndex(fun l -> l.Trim() <> "")

        match firstCode with
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

    /// Compile and execute generated F# source in the hosted session, capturing whatever
    /// it writes to the console. Returns the captured output (or a note if it printed
    /// nothing), or the compiler/runtime diagnostics if it failed.
    let run (fsharpSource: string) : Result<string, string> =
        lock gate (fun () ->
            let fsi = session.Value
            let fsharpSource = stripFileHeader fsharpSource

            // The executed code's `printfn`/`Console.Write` go to Console.Out; redirect it so
            // we can show the program's output rather than letting it escape to our own stdout.
            let captured = new StringWriter()
            let previous = Console.Out
            Console.SetOut captured

            try
                let result, diagnostics = fsi.EvalInteractionNonThrowing fsharpSource

                match result with
                | Choice1Of2 _ ->
                    let text = captured.ToString()

                    if String.IsNullOrWhiteSpace text then
                        Ok "// The code compiled and ran, but produced no console output."
                    else
                        Ok(text.TrimEnd())
                | Choice2Of2 ex -> Error(formatDiagnostics ex diagnostics)
            finally
                Console.SetOut previous)
