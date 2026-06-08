namespace AstEditor

open Fabulous
open Fabulous.Avalonia
open Avalonia.Themes.Fluent

open type Fabulous.Avalonia.View

/// A live REPL for Fabulous.AST: type the DSL on the left, see the generated F# on the
/// right. Evaluation runs through a hosted FSI session (see `Evaluator`), so anything
/// the library can express is fair game — not just records.
module App =

    // A full F# script: opens, helpers, an active pattern, a Rewrite pass, ending in the
    // generated source (`... |> Gen.run`). The trailing string is what the right pane shows.
    let private sample =
        """open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open type Fabulous.AST.Ast

let intExpr (value: int) : Expr =
    Expr.Constant(Constant.FromText(SingleTextNode(string value, Range.Zero)))

let (|IntLit|_|) (e: Expr) : int option =
    match e with
    | Expr.Constant(Constant.FromText n) ->
        match System.Int32.TryParse n.Text with
        | true, v -> Some v
        | _ -> None
    | _ -> None

// Constant-fold trivial arithmetic: x*1, x+0, and literal folding.
let foldConstants (e: Expr) : Expr =
    match e with
    | Expr.InfixApp n ->
        match n.Operator.Text, n.LeftHandSide, n.RightHandSide with
        | "+", lhs, IntLit 0
        | "+", IntLit 0, lhs
        | "*", lhs, IntLit 1
        | "*", IntLit 1, lhs -> lhs
        | "+", IntLit a, IntLit b -> intExpr (a + b)
        | "*", IntLit a, IntLit b -> intExpr (a * b)
        | _ -> e
    | _ -> e

Oak() {
    AnonymousModule() {
        Value("width", Int(10))
        Value("area", InfixAppExpr(InfixAppExpr(ConstantExpr(Constant "width"), "*", Int(1)), "+", Int(0)))
        Value("total", InfixAppExpr(Int(2), "+", Int(3)))
        AppExpr("printfn", [ String("area = %d, total = %d"); Constant("area"); Constant("total") ])
    }
}
|> Rewrite.expr foldConstants
|> Gen.mkOak
|> Gen.run"""

    let private recordSample =
        """open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        Record("Person") {
            Field("Name", "string")
            Field("Age", "int")
        }
    }
}
|> Gen.mkOak
|> Gen.run"""

    let private helloSample =
        """open Fabulous.AST
open type Fabulous.AST.Ast

Oak() {
    AnonymousModule() {
        Value("greeting", String("Hello from Fabulous.AST"))
        AppExpr("printfn", [ String("%s"); Constant("greeting") ])
    }
}
|> Gen.mkOak
|> Gen.run"""

    /// Example DSL scripts offered in the toolbar.
    let private examples =
        [ "Record", recordSample
          "Rewrite", sample
          "Hello", helloSample ]

    type Model =
        { /// One DSL editor tab per example; the active one drives generation.
          TabNames: string[]
          TabSources: string[]
          ActiveTab: int
          Output: string
          /// The last F# source that generated cleanly — what the Run button executes.
          LastValid: string option
          /// Captured console output from the most recent Run.
          RunOutput: string
          /// Bumped on every edit. A debounced eval only runs if its tag still matches,
          /// so we recompile once the user pauses — not on every keystroke.
          Version: int
          IsRunning: bool
          IsExecuting: bool
          /// Caret position in the DSL editor (1-based), shown in the status bar.
          CaretLine: int
          CaretColumn: int }

    type Msg =
        /// (tab index, new source)
        | SetSource of int * string
        /// Fires after the debounce delay, carrying the Version it was scheduled for.
        | Settle of int
        | RunDone of Result<string, string>
        | RunCode
        | RunCodeDone of Result<string, string>
        /// (tab index, line, column) — also makes that tab active.
        | SetCaret of int * int * int

    /// How long to wait after the last keystroke before regenerating.
    let private debounceMs = 400

    let init() =
        { TabNames = examples |> List.map fst |> Array.ofList
          TabSources = examples |> List.map snd |> Array.ofList
          ActiveTab = 0
          Output = "// Generating…"
          LastValid = None
          RunOutput = "// Click Run ▶ to execute the generated F#."
          Version = 0
          IsRunning = false
          IsExecuting = false
          CaretLine = 1
          CaretColumn = 1 },
        // Kick off an initial render so the right pane isn't empty on launch.
        Cmd.OfAsync.perform (fun () -> async { return 0 }) () Settle

    let private runEval (source: string) =
        async {
            // FSI evaluation is synchronous and can be slow on the first call; hop off
            // the UI thread so the window stays responsive while it compiles.
            do! Async.SwitchToThreadPool()
            return Evaluator.generate source
        }

    let private execCode (code: string) =
        async {
            do! Async.SwitchToThreadPool()
            return Evaluator.run code
        }

    /// Schedule a Settle for `version` after the debounce window elapses.
    let private debounce (version: int) =
        Cmd.OfAsync.perform
            (fun () ->
                async {
                    do! Async.Sleep debounceMs
                    return version
                })
            ()
            Settle

    let update msg model =
        match msg with
        | SetSource(tab, src) ->
            let version = model.Version + 1
            let sources = Array.copy model.TabSources
            sources.[tab] <- src

            { model with
                TabSources = sources
                ActiveTab = tab
                Version = version },
            debounce version
        | Settle version ->
            // Stale tag? The user kept typing; let the latest debounce win.
            if version = model.Version then
                { model with IsRunning = true }, Cmd.OfAsync.perform runEval model.TabSources.[model.ActiveTab] RunDone
            else
                model, Cmd.none
        | RunDone(Ok source) ->
            { model with
                Output = source
                LastValid = Some source
                IsRunning = false },
            Cmd.none
        | RunDone(Error diagnostics) ->
            { model with
                Output = "// " + diagnostics.Replace("\n", "\n// ")
                IsRunning = false },
            Cmd.none
        | RunCode ->
            match model.LastValid with
            | Some code -> { model with IsExecuting = true }, Cmd.OfAsync.perform execCode code RunCodeDone
            | None -> model, Cmd.none
        | RunCodeDone(Ok output) ->
            { model with
                RunOutput = output
                IsExecuting = false },
            Cmd.none
        | RunCodeDone(Error diagnostics) ->
            { model with
                RunOutput = "// " + diagnostics.Replace("\n", "\n// ")
                IsExecuting = false },
            Cmd.none
        | SetCaret(tab, line, column) ->
            // Interacting with a different tab makes it active and regenerates it, so the
            // generated/output panes follow the tab you're working in.
            if tab <> model.ActiveTab then
                let version = model.Version + 1

                { model with
                    ActiveTab = tab
                    CaretLine = line
                    CaretColumn = column
                    Version = version },
                debounce version
            else
                { model with
                    CaretLine = line
                    CaretColumn = column },
                Cmd.none

    let private monoFont =
        Avalonia.Media.FontFamily("Cascadia Code, Consolas, Menlo, monospace")

    let private code (w: WidgetBuilder<'msg, IFabTextEditor>) =
        w.fontFamily(monoFont).fontSize(14.)

    // IDE palette (VS Code-ish): dark title bar, signature blue status bar.
    let private brush (hex: string) : Avalonia.Media.IBrush =
        Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse hex)

    let private toolbarBrush = brush "#252526"
    let private statusBrush = brush "#007ACC"
    let private dimText = brush "#CCCCCC"
    let private white = brush "#FFFFFF"

    let private statusLabel(model: Model) =
        if model.IsRunning then "● Generating F#…"
        elif model.IsExecuting then "● Running…"
        else "● Ready"

    // One editable DSL tab per example. Dock supplies the tab title, so the pane is a bare
    // editor; editing a tab makes it the active one that drives generation.
    let private tabPane (model: Model) (i: int) =
        (TextEditor(model.TabSources.[i], (fun s -> SetSource(i, s))) |> code)
            .showLineNumbers(true)
            .highlightFSharp()
            .intelliSense()
            .diagnostics()
            .onCaretMoved(fun (line, column) -> SetCaret(i, line, column))

    let private generatedPane(model: Model) =
        (TextEditor(model.Output) |> code)
            .isReadOnly(true)
            .showLineNumbers(true)
            .highlightFSharp()

    let private outputPane(model: Model) =
        (TextEditor(model.RunOutput) |> code).isReadOnly(true).wordWrap(true)

    let private docked model =
        DockControl(
            (model.TabNames.[0], tabPane model 0),
            (model.TabNames.[1], tabPane model 1),
            (model.TabNames.[2], tabPane model 2),
            generatedPane model,
            outputPane model
        )

    /// Top application bar: title on the left, live status + Run on the right.
    let private toolbar(model: Model) =
        (Border(
            (Grid(coldefs = [ Auto; Star; Auto; Auto ], rowdefs = [ Auto ]) {
                TextBlock("⚡  Fabulous.AST Studio")
                    .fontSize(14.)
                    .foreground(white)
                    .centerVertical()
                    .gridColumn(0)

                TextBlock(statusLabel model)
                    .foreground(dimText)
                    .centerVertical()
                    .margin(0., 0., 12., 0.)
                    .gridColumn(2)

                Button((if model.IsExecuting then "Running…" else "▶  Run"), RunCode)
                    .isEnabled(model.LastValid.IsSome && not model.IsExecuting)
                    .gridColumn(3)
            })
                .margin(12., 8.)
        ))
            .background(toolbarBrush)

    let private caretBubbleBrush = brush "#1E5C99"

    /// The IDE "Ln x, Col y" position pill.
    let private caretBubble(model: Model) =
        (Border(
            TextBlock($"Ln {model.CaretLine},  Col {model.CaretColumn}")
                .foreground(white)
                .fontSize(12.)
        ))
            .background(caretBubbleBrush)
            .cornerRadius(9.)
            .padding(10., 1.)
            .centerVertical()

    /// Bottom status bar (the signature IDE blue strip).
    let private statusBar(model: Model) =
        (Border(
            (Grid(coldefs = [ Auto; Star; Auto; Auto ], rowdefs = [ Auto ]) {
                TextBlock(
                    if model.IsRunning then "Generating…"
                    elif model.IsExecuting then "Running generated code…"
                    else "Ready"
                )
                    .foreground(white)
                    .centerVertical()
                    .gridColumn(0)

                (caretBubble model).margin(0., 0., 12., 0.).gridColumn(2)

                TextBlock("F#  •  Fabulous.AST 2.0  •  TextMate")
                    .foreground(white)
                    .centerVertical()
                    .gridColumn(3)
            })
                .margin(12., 4.)
        ))
            .background(statusBrush)

    let private shell(model: Model) =
        (Grid(coldefs = [ Star ], rowdefs = [ Auto; Star; Auto ]) {
            (toolbar model).gridRow(0)
            (docked model).gridRow(1)
            (statusBar model).gridRow(2)
        })

    let view model =
        (DesktopApplication() {
            Window(shell model).title("Fabulous.AST Studio").width(1400.).height(820.)
        })
            .requestedThemeVariant(Avalonia.Styling.ThemeVariant.Dark)

    let create() =
        let program = Program.statefulWithCmd init update |> Program.withView view
        FabulousAppBuilder.Configure(FluentTheme, program)
