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

    type Model =
        { Source: string
          Output: string
          /// The last F# source that generated cleanly — what the Run button executes.
          LastValid: string option
          /// Captured console output from the most recent Run.
          RunOutput: string
          /// Bumped on every edit. A debounced eval only runs if its tag still matches,
          /// so we recompile once the user pauses — not on every keystroke.
          Version: int
          IsRunning: bool
          IsExecuting: bool }

    type Msg =
        | SetSource of string
        /// Fires after the debounce delay, carrying the Version it was scheduled for.
        | Settle of int
        | RunDone of Result<string, string>
        | RunCode
        | RunCodeDone of Result<string, string>

    /// How long to wait after the last keystroke before regenerating.
    let private debounceMs = 400

    let init() =
        { Source = sample
          Output = "// Generating…"
          LastValid = None
          RunOutput = "// Click Run ▶ to execute the generated F#."
          Version = 0
          IsRunning = false
          IsExecuting = false },
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
        | SetSource src ->
            let version = model.Version + 1
            { model with
                Source = src
                Version = version },
            debounce version
        | Settle version ->
            // Stale tag? The user kept typing; let the latest debounce win.
            if version = model.Version then
                { model with IsRunning = true }, Cmd.OfAsync.perform runEval model.Source RunDone
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

    // The dockable code panes. Dock supplies the tab title, so the panes are bare editors.
    let private sourcePane(model: Model) =
        (TextEditor(model.Source, SetSource) |> code)
            .showLineNumbers(true)
            .highlightFSharp()
            .intelliSense()
            .diagnostics()

    let private generatedPane(model: Model) =
        (TextEditor(model.Output) |> code)
            .isReadOnly(true)
            .showLineNumbers(true)
            .highlightFSharp()

    let private outputPane(model: Model) =
        (TextEditor(model.RunOutput) |> code).isReadOnly(true).wordWrap(true)

    let private docked model =
        DockControl(sourcePane model, generatedPane model, outputPane model)

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

    /// Bottom status bar (the signature IDE blue strip).
    let private statusBar(model: Model) =
        (Border(
            (Grid(coldefs = [ Auto; Star; Auto ], rowdefs = [ Auto ]) {
                TextBlock(
                    if model.IsRunning then "Generating…"
                    elif model.IsExecuting then "Running generated code…"
                    else "Ready"
                )
                    .foreground(white)
                    .centerVertical()
                    .gridColumn(0)

                TextBlock("F#  •  Fabulous.AST 2.0  •  TextMate")
                    .foreground(white)
                    .centerVertical()
                    .gridColumn(2)
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
