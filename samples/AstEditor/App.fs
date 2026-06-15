namespace AstEditor

open Fabulous
open Fabulous.Avalonia
open Avalonia.Layout
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
            /// The AST inspector's rendering: the generated F#'s syntax tree by default, or the
            /// selected DSL fragment's tree when there's a selection.
            Tree: string
            /// One locate-hint per tree line (the DSL name to find when that line is clicked),
            /// parallel to `Tree`'s lines.
            TreeHints: string option[]
            /// The active editor's current selection ("" when nothing is selected). When
            /// non-empty the tree shows this fragment instead of the generated F#.
            SelectedText: string
            /// Bumped whenever the tree's input (selection or generated F#) changes; a
            /// debounced refresh only reparses if its tag still matches, like Version/Settle.
            TreeVersion: int
            /// A highlight request for the active DSL editor (set when a tree line is clicked):
            /// a token so each request applies once, and the (offset, length) range to select.
            HighlightToken: int
            HighlightRange: (int * int) option
            /// Rewrite step-through: the generated F# at each pass (label, output), the current
            /// step index, and whether a recompute is in flight.
            RewriteSteps: (string * string)[]
            RewriteStep: int
            RewriteComputing: bool
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
            /// Whether the left file-explorer panel is shown (toggled from the activity bar).
            ShowExplorer: bool
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
        /// The active editor's selection changed (tab index, selected text).
        | SelectionChanged of int * string
        /// Fires after the tree debounce, carrying the TreeVersion it was scheduled for.
        | RefreshTree of int
        /// AST inspector: the summarized syntax tree, tagged with the TreeVersion it was
        /// parsed for, so a stale parse (the source/selection changed meanwhile) is dropped.
        | TreeDone of int * SyntaxTree.Tree
        /// A line of the syntax-tree pane was clicked (1-based); highlight the matching DSL.
        | TreeCaret of int
        /// Rewrite step-through: evaluate the active sample with 0..n rewrite passes applied.
        | RunRewriteSteps
        | RewriteStepsDone of (string * string)[]
        /// Navigate to a Rewrite step (clamped to range).
        | SetRewriteStep of int
        | RunCode
        | RunCodeDone of Result<string, string>
        /// (tab index, line, column) — also makes that tab active.
        | SetCaret of int * int * int
        /// Dock activated a different DSL tab (e.g. a tab-header click).
        | ActivateTab of int
        /// The user closed a DSL tab (the X on its header).
        | TabClosed of int
        /// The file explorer selected a sample: open it if closed, then make it active
        /// (this is also how a closed sample is reopened — click its dimmed row).
        | SelectSample of int
        /// Show/hide the left file-explorer panel.
        | ToggleExplorer
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
          Tree = "// Parsing…"
          TreeHints = [||]
          SelectedText = ""
          TreeVersion = 0
          HighlightToken = 0
          HighlightRange = None
          RewriteSteps = [||]
          RewriteStep = 0
          RewriteComputing = false
          LastValid = None
          RunOutput = "// Click Run ▶ to execute the generated F#."
          Version = 0
          IsRunning = false
          IsExecuting = false
          CaretLine = 1
          CaretColumn = 1
          Theme = Avalonia.Styling.ThemeVariant.Dark
          ShowExplorer = true },
        // Kick off the initial generation (right pane) and the initial syntax-tree parse.
        Cmd.batch
            [ Cmd.OfAsync.perform (fun () -> async { return 0 }) () Settle
              Cmd.OfAsync.perform (fun () -> async { return 0 }) () RefreshTree ]

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

    /// Split a script's `… |> Rewrite.expr f |> … |> Gen.mkOak |> Gen.run` pipeline into a
    /// step-through: step 0 is the input with no rewrites, step k applies the first k rewrite
    /// passes. Each entry is (label, a runnable script ending in Gen.run). Empty if the script
    /// has no Rewrite passes.
    let private rewriteStepScripts(script: string) : (string * string) list =
        let lines = script.Replace("\r\n", "\n").Split('\n') |> Array.toList

        let isStage(l: string) =
            l.TrimStart().StartsWith("|>", System.StringComparison.Ordinal)

        match lines |> List.tryFindIndex isStage with
        | None -> []
        | Some firstStage ->
            let prefix = lines |> List.take firstStage |> String.concat "\n"
            let stages = lines |> List.skip firstStage |> List.filter(fun l -> l.Trim() <> "")
            let rewrites = stages |> List.filter(fun l -> l.Contains "Rewrite.")
            let tail = stages |> List.filter(fun l -> not(l.Contains "Rewrite."))

            if List.isEmpty rewrites then
                []
            else
                [ for k in 0 .. List.length rewrites ->
                      let used = rewrites |> List.truncate k
                      let body = prefix :: (used @ tail) |> String.concat "\n"

                      let label =
                          if k = 0 then
                              "input (no rewrites)"
                          else
                              rewrites.[k - 1].Trim()

                      label, body ]

    /// How long after a tree-input change before reparsing (snappier than the eval debounce).
    let private treeDebounceMs = 250

    /// What the AST inspector parses: the selected DSL fragment if there's a selection, else
    /// the last cleanly-generated F# (so the default view is the generated output's tree).
    let private treeSource(model: Model) : string option =
        if model.SelectedText <> "" then
            Some model.SelectedText
        else
            model.LastValid

    /// Schedule a tree refresh for `treeVersion` after the debounce window.
    let private debounceTree(treeVersion: int) =
        Cmd.OfAsync.perform
            (fun () ->
                async {
                    do! Async.Sleep treeDebounceMs
                    return treeVersion
                })
            ()
            RefreshTree

    let private placeholderTree(text: string) : SyntaxTree.Tree =
        { Text = text
          Hints = [||] }

    /// Parse the tree's input and render its syntax tree (text + per-line locate hints).
    let private parseTree(source: string option) =
        async {
            match source with
            | None -> return placeholderTree "// (no generated F# yet — fix errors to see the tree)"
            | Some s ->
                match! Intellisense.parse s with
                | Some tree -> return SyntaxTree.summarize tree
                | None -> return placeholderTree "// (not a parseable expression)"
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

    /// The canonical "make tab `tab` active" transition shared by every input path (dock
    /// header click, file explorer, reopen button): reopen it if it was closed, point the
    /// model at it, drop the now-stale generated output, and schedule a regenerate. A no-op
    /// when it's already the open active tab. Bounds-checked against the one tab count.
    let private activateTab (tab: int) (model: Model) =
        if tab < 0 || tab >= model.TabNames.Length then
            model, Cmd.none
        elif tab = model.ActiveTab && not(model.ClosedTabs.Contains tab) then
            model, Cmd.none
        else
            let version = model.Version + 1
            let treeVersion = model.TreeVersion + 1

            // Switching tabs clears any selection / stale highlight / rewrite steps and
            // refreshes the tree.
            { model with
                ActiveTab = tab
                ClosedTabs = model.ClosedTabs.Remove tab
                LastValid = None
                SelectedText = ""
                Version = version
                TreeVersion = treeVersion
                HighlightRange = None
                RewriteSteps = [||]
                RewriteStep = 0 },
            Cmd.batch [ debounce version; debounceTree treeVersion ]

    let update msg model =
        match msg with
        | SetSource(tab, src) ->
            let version = model.Version + 1
            let treeVersion = model.TreeVersion + 1
            let sources = Array.copy model.TabSources
            sources[tab] <- src

            { model with
                TabSources = sources
                ActiveTab = tab
                Version = version
                TreeVersion = treeVersion },
            Cmd.batch [ debounce version; debounceTree treeVersion ]
        | Settle version ->
            // Stale tag? The user kept typing; let the latest debounce win.
            if version = model.Version then
                { model with IsRunning = true },
                Cmd.OfAsync.perform runEval model.TabSources[model.ActiveTab] (fun r -> RunDone(version, r))
            else
                model, Cmd.none
        | RunDone(version, result) ->
            if version <> model.Version then
                // Stale eval (edit or tab switch since it started); a newer one is in flight.
                model, Cmd.none
            else
                match result with
                | Ok source ->
                    // The generated F# changed; refresh the (default) tree unless a selection
                    // is currently driving it instead.
                    let treeVersion = model.TreeVersion + 1

                    { model with
                        Output = source
                        LastValid = Some source
                        IsRunning = false
                        TreeVersion = treeVersion },
                    (if model.SelectedText = "" then
                         debounceTree treeVersion
                     else
                         Cmd.none)
                | Error diagnostics ->
                    { model with
                        Output = "// " + diagnostics.Replace("\n", "\n// ")
                        IsRunning = false },
                    Cmd.none
        | SelectionChanged(tab, text) ->
            // Only the active editor's selection drives the inspector. A new selection switches
            // the tree to that fragment; clearing it returns to the generated-F# tree.
            if tab = model.ActiveTab && text <> model.SelectedText then
                let treeVersion = model.TreeVersion + 1

                { model with
                    SelectedText = text
                    TreeVersion = treeVersion },
                debounceTree treeVersion
            else
                model, Cmd.none
        | RefreshTree treeVersion ->
            // Stale tag? The input changed again; let the latest debounce win.
            if treeVersion = model.TreeVersion then
                model, Cmd.OfAsync.perform parseTree (treeSource model) (fun tree -> TreeDone(treeVersion, tree))
            else
                model, Cmd.none
        | TreeDone(treeVersion, tree) ->
            if treeVersion = model.TreeVersion then
                { model with
                    Tree = tree.Text
                    TreeHints = tree.Hints },
                Cmd.none
            else
                model, Cmd.none
        | TreeCaret line ->
            // A tree line was clicked: find that node's locate-hint in the active DSL and
            // request a highlight of the whole line containing it (a name-search heuristic —
            // fine for the samples where names are distinct).
            let hint =
                if line >= 1 && line <= model.TreeHints.Length then
                    model.TreeHints[line - 1]
                else
                    None

            match hint with
            | Some h when model.ActiveTab >= 0 && model.ActiveTab < model.TabSources.Length ->
                let src = model.TabSources[model.ActiveTab]
                let idx = src.IndexOf(h, System.StringComparison.Ordinal)

                if idx < 0 then
                    model, Cmd.none
                else
                    let lineStart = src.LastIndexOf('\n', idx) + 1
                    let nl = src.IndexOf('\n', idx)
                    let lineEnd = if nl < 0 then src.Length else nl

                    { model with
                        HighlightToken = model.HighlightToken + 1
                        HighlightRange = Some(lineStart, lineEnd - lineStart) },
                    Cmd.none
            | _ -> model, Cmd.none
        | RunRewriteSteps ->
            let scripts =
                if model.ActiveTab >= 0 && model.ActiveTab < model.TabSources.Length then
                    rewriteStepScripts model.TabSources[model.ActiveTab]
                else
                    []

            if List.isEmpty scripts then
                model, Cmd.none
            else
                { model with RewriteComputing = true },
                Cmd.OfAsync.perform
                    (fun () ->
                        async {
                            do! Async.SwitchToThreadPool()

                            return
                                scripts
                                |> List.map(fun (label, s) ->
                                    match Evaluator.generate s with
                                    | Ok out -> label, out
                                    | Error e ->
                                        label, "// could not evaluate this step:\n// " + e.Replace("\n", "\n// "))
                                |> List.toArray
                        })
                    ()
                    RewriteStepsDone
        | RewriteStepsDone steps ->
            { model with
                RewriteSteps = steps
                RewriteStep = 0
                RewriteComputing = false },
            Cmd.none
        | SetRewriteStep i ->
            let clamped = max 0 (min i (model.RewriteSteps.Length - 1))
            { model with RewriteStep = clamped }, Cmd.none
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
            // Interacting with a tab makes it active (regenerating it, via activateTab) and
            // always updates the caret pill — so the generated/output panes and the status
            // bar follow the tab you're working in.
            let m, cmd = activateTab tab model

            { m with
                CaretLine = line
                CaretColumn = column },
            cmd
        // Dock header click and explorer selection both funnel through the one activation
        // transition, so they can't drift apart.
        | ActivateTab tab -> activateTab tab model
        | SelectSample tab -> activateTab tab model
        | TabClosed tab ->
            // The dock keeps at least one tab open (closing the last is cancelled), and
            // closing the *active* tab activates a neighbor, reported via ActivateTab —
            // which re-points generation. Nothing more to do here.
            { model with ClosedTabs = model.ClosedTabs.Add tab }, Cmd.none
        | ToggleExplorer -> { model with ShowExplorer = not model.ShowExplorer }, Cmd.none
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

    // Theme-independent accents.
    let private statusBrush = brush "#007ACC" // the signature IDE blue status strip
    let private white = brush "#FFFFFF" // text on the blue status strip
    let private fsGlyph = brush "#B180D7" // F# violet (reads on light and dark)
    let private transparentBrush = brush "#00000000"

    /// The IDE chrome palette (toolbar / activity bar / explorer / status). Hand-rolled
    /// controls don't inherit the Fluent theme's brushes, so we carry both variants and pick
    /// the one matching the resolved light/dark theme — otherwise the chrome stays dark under
    /// a light editor (wrong contrast).
    type private Palette =
        { Toolbar: Avalonia.Media.IBrush
          Activity: Avalonia.Media.IBrush
          Explorer: Avalonia.Media.IBrush
          ExplorerSel: Avalonia.Media.IBrush
          Text: Avalonia.Media.IBrush
          DimText: Avalonia.Media.IBrush
          TreeText: Avalonia.Media.IBrush
          HeaderText: Avalonia.Media.IBrush }

    let private darkPalette =
        { Toolbar = brush "#252526"
          Activity = brush "#2D2D30"
          Explorer = brush "#252526"
          ExplorerSel = brush "#37373D"
          Text = brush "#FFFFFF"
          DimText = brush "#CCCCCC"
          TreeText = brush "#CCCCCC"
          HeaderText = brush "#9A9A9A" }

    let private lightPalette =
        { Toolbar = brush "#DDE1E6"
          Activity = brush "#D0D4DA"
          Explorer = brush "#F3F3F3"
          ExplorerSel = brush "#CFE0F7"
          Text = brush "#1E1E1E"
          DimText = brush "#3C3C3C"
          TreeText = brush "#2B2B2B"
          HeaderText = brush "#6A6A6A" }

    /// True when the app currently resolves to a dark theme. Light/Dark are explicit; System
    /// (Default) follows the OS preference, read from PlatformSettings so it's stable in-view.
    let private resolvedDark(model: Model) =
        if model.Theme = Avalonia.Styling.ThemeVariant.Light then
            false
        elif model.Theme = Avalonia.Styling.ThemeVariant.Dark then
            true
        else
            match Avalonia.Application.Current with
            | null -> true
            | app ->
                match app.PlatformSettings with
                | null -> true
                | ps -> ps.GetColorValues().ThemeVariant = Avalonia.Platform.PlatformThemeVariant.Dark

    let private palette(model: Model) =
        if resolvedDark model then darkPalette else lightPalette

    // Fixed pane widths.
    let private activityBarWidth = 48.
    let private explorerWidth = 240.

    let private statusLabel(model: Model) =
        if model.IsRunning then "● Generating F#…"
        elif model.IsExecuting then "● Running…"
        else "● Ready"

    // One editable DSL tab per example. Dock supplies the tab title, so the pane is a bare
    // editor; editing a tab makes it the active one that drives generation.
    let private tabPane (model: Model) (i: int) =
        (TextEditor(model.TabSources[i], (fun s -> SetSource(i, s))) |> code)
            .showLineNumbers(true)
            .highlightFSharp()
            .intelliSense()
            .diagnostics()
            .onCaretMoved(fun (line, column) -> SetCaret(i, line, column))
            .onSelectionChanged(fun text -> SelectionChanged(i, text))
            // Only the active editor honours a tree-click highlight request.
            .selectRange(
                (if i = model.ActiveTab then model.HighlightToken else 0),
                (if i = model.ActiveTab then model.HighlightRange else None)
            )

    let private generatedPane(model: Model) =
        (TextEditor(model.Output) |> code).isReadOnly(true).showLineNumbers(true).highlightFSharp()

    /// AST inspector: the generated F#'s syntax tree (or the selected DSL fragment's tree when
    /// there's a selection). Clicking a line highlights the DSL that produced it. Plain (no F#
    /// highlighting) since it's a node tree, not source.
    let private syntaxTreePane(model: Model) =
        (TextEditor(model.Tree) |> code).isReadOnly(true).wordWrap(false).onCaretMoved(fun (line, _) -> TreeCaret line)

    let private outputPane(model: Model) =
        (TextEditor(model.RunOutput) |> code).isReadOnly(true).wordWrap(true)

    /// Rewrite step-through: a stepper over the generated F# with 0..n Rewrite passes applied,
    /// so you can watch each `Rewrite.expr` transform the output. On-demand (the evals are
    /// real FSI runs) via the Run-steps button; Prev/Next walk the passes.
    let private rewritePane(model: Model) =
        let p = palette model

        let usesRewrite =
            model.ActiveTab >= 0
            && model.ActiveTab < model.TabSources.Length
            && model.TabSources[model.ActiveTab].Contains "Rewrite."

        let count = model.RewriteSteps.Length

        if not usesRewrite then
            (Grid(coldefs = [ Star ], rowdefs = [ Star ]) {
                TextBlock(
                    "This sample has no Rewrite passes.\n\nOpen the “Rewrite” sample and press ↻ Run steps to watch\nconstant-folding transform the generated F# pass by pass."
                )
                    .foreground(p.DimText)
                    .centerHorizontal()
                    .centerVertical()
            })
        else
            let label =
                if count = 0 then
                    "Press ↻ Run steps"
                else
                    let lbl, _ = model.RewriteSteps[model.RewriteStep]
                    $"Step {model.RewriteStep} / {count - 1}  ·  {lbl}"

            let body =
                if count = 0 then
                    "// Press “↻ Run steps” to evaluate the active sample with each Rewrite\n// pass applied in turn (step 0 = the input, before any rewrites)."
                else
                    snd model.RewriteSteps[model.RewriteStep]

            (Grid(coldefs = [ Star ], rowdefs = [ Auto; Star ]) {
                (HStack(8.) {
                    Button((if model.RewriteComputing then "Running…" else "↻ Run steps"), RunRewriteSteps)
                        .isEnabled(not model.RewriteComputing)

                    Button("◀ Prev", SetRewriteStep(model.RewriteStep - 1))
                        .isEnabled(count > 0 && model.RewriteStep > 0)

                    TextBlock(label).centerVertical().foreground(p.Text)

                    Button("Next ▶", SetRewriteStep(model.RewriteStep + 1))
                        .isEnabled(count > 0 && model.RewriteStep < count - 1)
                })
                    .margin(8., 6.)
                    .gridRow(0)

                ((TextEditor(body) |> code).isReadOnly(true).showLineNumbers(true).highlightFSharp()).gridRow(1)
            })

    let private docked model =
        DockControl(
            model.TabNames |> Array.mapi(fun i name -> name, tabPane model i),
            generatedPane model,
            syntaxTreePane model,
            rewritePane model,
            outputPane model
        )
            .onActiveTabChanged(ActivateTab)
            .onTabClosed(TabClosed)
            // The model owns the tab arrangement; the dock reconciles to it in one pass
            // (session restore, explorer selection, and the reopen buttons all flow here).
            .tabsState(model.ClosedTabs, model.ActiveTab)

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

    /// Top application bar: title on the left, then a flexible spacer (column 1) pushing the
    /// theme switcher + live status + Run to the right. (Closed samples are reopened from the
    /// file explorer, not here.)
    let private toolbar(model: Model) =
        let p = palette model

        Border(
            (Grid(coldefs = [ Auto; Star; Auto; Auto; Auto ], rowdefs = [ Auto ]) {
                TextBlock("⚡  Fabulous.AST Studio").fontSize(14.).foreground(p.Text).centerVertical().gridColumn(0)

                (themePicker model).margin(0., 0., 16., 0.).gridColumn(2)

                TextBlock(statusLabel model)
                    .foreground(p.DimText)
                    .centerVertical()
                    .margin(0., 0., 12., 0.)
                    .gridColumn(3)

                Button((if model.IsExecuting then "Running…" else "▶  Run"), RunCode)
                    .isEnabled(model.LastValid.IsSome && not model.IsExecuting)
                    .gridColumn(4)
            })
                .margin(12., 8.)
        )
            .background(p.Toolbar)

    let private caretBubbleBrush = brush "#1E5C99"

    /// The IDE "Ln x, Col y" position pill.
    let private caretBubble(model: Model) =
        Border(TextBlock($"Ln {model.CaretLine},  Col {model.CaretColumn}").foreground(white).fontSize(12.))
            .background(caretBubbleBrush)
            .cornerRadius(9.)
            .padding(10., 1.)
            .centerVertical()

    /// The active sample's name, or "—" if the active index is somehow out of range.
    let private activeTabName(model: Model) =
        if model.ActiveTab >= 0 && model.ActiveTab < model.TabNames.Length then
            model.TabNames[model.ActiveTab]
        else
            "—"

    /// Live work state, shown in the status bar (also mirrored, tersely, in the toolbar).
    let private workStatus(model: Model) =
        if model.IsRunning then "Generating…"
        elif model.IsExecuting then "Running generated code…"
        else "Ready"

    /// Bottom status bar (the signature IDE blue strip): a breadcrumb to the active sample on
    /// the left, then live work state, caret position, and environment on the right.
    let private statusBar(model: Model) =
        Border(
            (Grid(coldefs = [ Auto; Star; Auto; Auto; Auto ], rowdefs = [ Auto ]) {
                TextBlock($"Fabulous.AST  ›  samples  ›  AstEditor  ›  {activeTabName model}")
                    .foreground(white)
                    .centerVertical()
                    .gridColumn(0)

                TextBlock(workStatus model).foreground(white).centerVertical().margin(0., 0., 16., 0.).gridColumn(2)

                (caretBubble model).margin(0., 0., 12., 0.).gridColumn(3)

                TextBlock("F#  •  Fabulous.AST 2.0  •  TextMate").foreground(white).centerVertical().gridColumn(4)
            })
                .margin(12., 4.)
        )
            .background(statusBrush)

    /// Far-left activity bar (VS Code / Rider style): a tight column of icon buttons at the
    /// top. Every icon is wired — the folder toggles the explorer, the triangle runs the
    /// generated F#. Fixed-height square cells keep the two glyphs evenly spaced regardless of
    /// emoji line-height differences.
    let private activityBar(model: Model) =
        let p = palette model

        let icon (glyph: string) (msg: Msg) (active: bool) =
            Button(
                msg,
                TextBlock(glyph)
                    .fontSize(18.)
                    .centerHorizontal()
                    .centerVertical()
                    .foreground(if active then p.Text else p.DimText)
            )
                .background(transparentBrush)
                .borderThickness(Avalonia.Thickness 0.)
                .cornerRadius(0.)
                .padding(0.)
                .height(activityBarWidth)
                .horizontalAlignment(HorizontalAlignment.Stretch)
                .horizontalContentAlignment(HorizontalAlignment.Center)

        (VStack(0.) {
            icon "🗂" ToggleExplorer model.ShowExplorer
            icon "▶" RunCode model.LastValid.IsSome
        })
            .background(p.Activity)
            .width(activityBarWidth)

    /// One file-explorer row per sample: an F# glyph + the sample name. The active sample is
    /// highlighted, closed ones are dimmed; clicking opens/activates it.
    let private sampleRow (model: Model) (i: int) =
        let p = palette model
        let isClosed = model.ClosedTabs.Contains i
        let isActive = i = model.ActiveTab && not isClosed

        Button(
            SelectSample i,
            (HStack(6.) {
                TextBlock("F#").fontSize(11.).centerVertical().foreground(fsGlyph)

                TextBlock(model.TabNames[i])
                    .fontSize(13.)
                    .centerVertical()
                    .foreground(
                        if isClosed then p.DimText
                        elif isActive then p.Text
                        else p.TreeText
                    )
            })
                .margin(22., 0., 0., 0.)
        )
            .background(if isActive then p.ExplorerSel else transparentBrush)
            .borderThickness(Avalonia.Thickness 0.)
            .cornerRadius(0.)
            .padding(8., 5.)
            .horizontalAlignment(HorizontalAlignment.Stretch)
            .horizontalContentAlignment(HorizontalAlignment.Left)

    /// The left file-explorer panel: a "Solution"-style tree of the DSL samples. Indent rhythm:
    /// the "Solution" header and the "AstEditor" root sit at 12px; sample leaves are nested
    /// under the root (8px button padding + 22px content inset) so their glyphs line up just
    /// past the disclosure triangle.
    let private explorer(model: Model) =
        let p = palette model

        Border(
            VStack(2.) {
                Border(TextBlock("Solution").fontSize(12.).foreground(p.HeaderText).centerVertical()).padding(12., 8.)

                TextBlock("▾  AstEditor").fontSize(13.).foreground(p.TreeText).margin(12., 2., 0., 4.)

                ScrollViewer(
                    VStack(1.) {
                        for i in 0 .. model.TabNames.Length - 1 do
                            sampleRow model i
                    }
                )
            }
        )
            .background(p.Explorer)
            .width(explorerWidth)

    /// One stable grid: activity bar, the explorer (collapsed via isVisible when hidden, so
    /// its Auto column shrinks to zero), and the dock. Keeping the dock at a fixed child
    /// position means toggling the explorer never rebuilds it (which would lose live editor
    /// state); Fabulous diffs the subtree in place.
    let private mainRow(model: Model) =
        (Grid(coldefs = [ Auto; Auto; Star ], rowdefs = [ Star ]) {
            (activityBar model).gridColumn(0)
            (explorer model).isVisible(model.ShowExplorer).gridColumn(1)
            (docked model).gridColumn(2)
        })

    let private shell(model: Model) =
        (Grid(coldefs = [ Star ], rowdefs = [ Auto; Star; Auto ]) {
            (toolbar model).gridRow(0)
            (mainRow model).gridRow(1)
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
