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

    // Ships with a deliberate typo so the quick-fix flow is one squiggle away.
    let private quickFixSample =
        """open Fabulous.AST
open type Fabulous.AST.Ast

// ⚡ Quick-fix demo: `Vlaue` below is a typo, so this script doesn't generate.
// Wait for the red squiggle, then click the gutter lightbulb (or press Ctrl+.)
// and choose "Replace with 'Value'". Fixes only appear while the script is broken.
Oak() {
    AnonymousModule() {
        Vlaue("greeting", String("Hello from quick fixes"))
        AppExpr("printfn", [ String("%s"); Constant("greeting") ])
    }
}
|> Gen.mkOak
|> Gen.run"""

    let private convertSample =
        """open Fabulous.AST
open type Fabulous.AST.Ast

// ✦ Convert demo: code actions can restructure the DSL, not just repair typos.
// Click the 💡 lightbulb on the `Record` line below (or press Ctrl+.) and pick
// "Convert Record to Union (DU)" — each Field becomes a UnionCase and the
// generated F# on the right switches from a record to a discriminated union.
Oak() {
    AnonymousModule() {
        Record("Shape") {
            Field("Width", "float")
            Field("Height", "float")
        }
    }
}
|> Gen.mkOak
|> Gen.run"""

    /// Example DSL scripts offered in the toolbar.
    let private examples =
        [ "Record", recordSample
          "Rewrite", sample
          "Hello", helloSample
          "Quick Fix", quickFixSample
          "Convert", convertSample ]

    type Model =
        {
            /// One DSL editor tab per example; the active one drives generation.
            TabNames: string[]
            TabSources: string[]
            ActiveTab: int
            /// Tabs the user closed (by index). Their sources are kept — and still saved in
            /// the session — so reopening (or restarting) brings the edits back.
            ClosedTabs: Set<int>
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
            CaretColumn: int
            /// The application theme variant (also drives the editors' syntax theme).
            Theme: Avalonia.Styling.ThemeVariant
        }

    type Msg =
        /// (tab index, new source)
        | SetSource of int * string
        /// Fires after the debounce delay, carrying the Version it was scheduled for.
        | Settle of int
        /// Eval finished. Tagged with the Version the eval was started for, so a stale
        /// result (the user edited or switched tabs meanwhile) is dropped instead of
        /// overwriting the current tab's panes.
        | RunDone of int * Result<string, string>
        | RunCode
        | RunCodeDone of Result<string, string>
        /// (tab index, line, column) — also makes that tab active.
        | SetCaret of int * int * int
        /// Dock activated a different DSL tab (e.g. a tab-header click).
        | ActivateTab of int
        /// The user closed a DSL tab (the X on its header).
        | TabClosed of int
        /// Reopen a closed DSL tab (toolbar button) and make it active.
        | ReopenTab of int
        /// Persist the dock arrangement + session on window close.
        | SaveSession
        /// Switch the app theme variant (and the editors' syntax theme).
        | SetTheme of Avalonia.Styling.ThemeVariant

    /// How long to wait after the last keystroke before regenerating.
    let private debounceMs = 400

    let init() =
        let names = examples |> List.map fst |> Array.ofList
        let defaults = examples |> List.map snd |> Array.ofList

        // Restore the last session's edits + active tab if they match the current tab count.
        let sources, activeTab =
            match Session.tryLoad() with
            | Some(saved, active) when saved.Length = defaults.Length ->
                saved, (if active >= 0 && active < saved.Length then active else 0)
            | _ -> defaults, 0

        { TabNames = names
          TabSources = sources
          ActiveTab = activeTab
          ClosedTabs = Set.empty
          Output = "// Generating…"
          LastValid = None
          RunOutput = "// Click Run ▶ to execute the generated F#."
          Version = 0
          IsRunning = false
          IsExecuting = false
          CaretLine = 1
          CaretColumn = 1
          Theme = Avalonia.Styling.ThemeVariant.Dark },
        // Kick off an initial render so the right pane isn't empty on launch.
        Cmd.OfAsync.perform (fun () -> async { return 0 }) () Settle

    let private runEval(source: string) =
        async {
            // FSI evaluation is synchronous and can be slow on the first call; hop off
            // the UI thread so the window stays responsive while it compiles.
            do! Async.SwitchToThreadPool()
            return Evaluator.generate source
        }

    let private execCode(code: string) =
        async {
            do! Async.SwitchToThreadPool()
            return Evaluator.run code
        }

    /// Schedule a Settle for `version` after the debounce window elapses.
    let private debounce(version: int) =
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
                { model with IsRunning = true },
                Cmd.OfAsync.perform runEval model.TabSources.[model.ActiveTab] (fun r -> RunDone(version, r))
            else
                model, Cmd.none
        | RunDone(version, result) ->
            if version <> model.Version then
                // Stale eval (edit or tab switch since it started); a newer one is in flight.
                model, Cmd.none
            else
                match result with
                | Ok source ->
                    { model with
                        Output = source
                        LastValid = Some source
                        IsRunning = false },
                    Cmd.none
                | Error diagnostics ->
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

                // LastValid belonged to the previous tab; clear it so Run can't execute the
                // wrong tab's code. The debounced regenerate repopulates it.
                { model with
                    ActiveTab = tab
                    CaretLine = line
                    CaretColumn = column
                    LastValid = None
                    Version = version },
                debounce version
            else
                { model with
                    CaretLine = line
                    CaretColumn = column },
                Cmd.none
        | ActivateTab tab ->
            if tab <> model.ActiveTab && tab >= 0 && tab < model.TabSources.Length then
                let version = model.Version + 1

                { model with
                    ActiveTab = tab
                    LastValid = None
                    Version = version },
                debounce version
            else
                model, Cmd.none
        | TabClosed tab ->
            // The dock keeps at least one tab open (closing the last is cancelled), and
            // closing the *active* tab activates a neighbor, reported via ActivateTab —
            // which re-points generation. Nothing more to do here.
            { model with ClosedTabs = model.ClosedTabs.Add tab }, Cmd.none
        | ReopenTab tab ->
            if model.ClosedTabs.Contains tab then
                // Same transition as activating the tab: the dock re-inserts the document
                // (via closedTabs reconciliation) and the debounced eval regenerates it.
                let version = model.Version + 1

                { model with
                    ClosedTabs = model.ClosedTabs.Remove tab
                    ActiveTab = tab
                    LastValid = None
                    Version = version },
                debounce version
            else
                model, Cmd.none
        | SaveSession ->
            Session.save model.TabSources model.ActiveTab
            model, Cmd.none
        | SetTheme variant ->
            // The view's requestedThemeVariant drives Avalonia; the editors' syntax theme
            // follows the *resolved* variant via EditorTheme's ActualThemeVariantChanged
            // subscription (so "System" tracks the real OS setting).
            { model with Theme = variant }, Cmd.none

    let private monoFont =
        Avalonia.Media.FontFamily("Cascadia Code, Consolas, Menlo, monospace")

    let private code(w: WidgetBuilder<'msg, IFabTextEditor>) = w.fontFamily(monoFont).fontSize(14.)

    // IDE palette (VS Code-ish): dark title bar, signature blue status bar.
    let private brush(hex: string) : Avalonia.Media.IBrush =
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
        (TextEditor(model.Output) |> code).isReadOnly(true).showLineNumbers(true).highlightFSharp()

    let private outputPane(model: Model) =
        (TextEditor(model.RunOutput) |> code).isReadOnly(true).wordWrap(true)

    let private docked model =
        DockControl(
            model.TabNames |> Array.mapi(fun i name -> name, tabPane model i),
            generatedPane model,
            outputPane model
        )
            .onActiveTabChanged(ActivateTab)
            // Restore the session's active tab when the layout is first built.
            .initialActiveTab(model.ActiveTab)
            .onTabClosed(TabClosed)
            .closedTabs(model.ClosedTabs)

    /// A small grouped theme switcher (System / Light / Dark).
    let private themePicker(model: Model) =
        let pick (label: string) (variant: Avalonia.Styling.ThemeVariant) =
            Button(label, SetTheme variant).isEnabled(model.Theme <> variant)

        (HStack(4.) {
            pick "🖥 System" Avalonia.Styling.ThemeVariant.Default
            pick "☀ Light" Avalonia.Styling.ThemeVariant.Light
            pick "🌙 Dark" Avalonia.Styling.ThemeVariant.Dark
        })
            .centerVertical()

    /// One reopen button per closed tab, so a closed tab is never more than a click away.
    let private reopenStrip(model: Model) =
        (HStack(4.) {
            for i in Set.toList model.ClosedTabs do
                Button($"⊕ {model.TabNames.[i]}", ReopenTab i)
        })
            .centerVertical()
            .margin(16., 0., 0., 0.)

    /// Top application bar: title, reopen buttons for closed tabs, theme + status + Run.
    let private toolbar(model: Model) =
        (Border(
            (Grid(coldefs = [ Auto; Star; Auto; Auto; Auto ], rowdefs = [ Auto ]) {
                TextBlock("⚡  Fabulous.AST Studio").fontSize(14.).foreground(white).centerVertical().gridColumn(0)

                (reopenStrip model).gridColumn(1)

                (themePicker model).margin(0., 0., 16., 0.).gridColumn(2)

                TextBlock(statusLabel model).foreground(dimText).centerVertical().margin(0., 0., 12., 0.).gridColumn(3)

                Button((if model.IsExecuting then "Running…" else "▶  Run"), RunCode)
                    .isEnabled(model.LastValid.IsSome && not model.IsExecuting)
                    .gridColumn(4)
            })
                .margin(12., 8.)
        ))
            .background(toolbarBrush)

    let private caretBubbleBrush = brush "#1E5C99"

    /// The IDE "Ln x, Col y" position pill.
    let private caretBubble(model: Model) =
        (Border(TextBlock($"Ln {model.CaretLine},  Col {model.CaretColumn}").foreground(white).fontSize(12.)))
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

                TextBlock("F#  •  Fabulous.AST 2.0  •  TextMate").foreground(white).centerVertical().gridColumn(3)
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
            Window(shell model)
                .title("Fabulous.AST Studio")
                .width(1400.)
                .height(820.)
                .onWindowClosing(fun _ -> SaveSession)
        })
            .requestedThemeVariant(model.Theme)

    let create() =
        let program = Program.statefulWithCmd init update |> Program.withView view
        FabulousAppBuilder.Configure(FluentTheme, program)
