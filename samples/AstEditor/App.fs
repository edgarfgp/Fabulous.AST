namespace AstEditor

open System
open System.Diagnostics
open Avalonia
open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Input
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Styling
open Avalonia.Themes.Fluent
open Fabulous
open Fabulous.Avalonia

open type Fabulous.Avalonia.View

/// A live playground for Fabulous.AST: write the DSL on the left, see the generated F# on the
/// right, with problems / run output / the generated code's syntax tree in the bottom panel.
module App =

    // ---- Model -----------------------------------------------------------------------------

    type Doc =
        { Name: string
          Source: string
          IsOpen: bool }

    type BottomTab =
        | Problems
        | Output
        | SyntaxTree

    /// What the pipeline (check → generate) is doing for the active document.
    type Phase =
        | Checking
        | Generating
        | Done

    type Model =
        {
            /// Every document ever opened this session. Closed ones stay (hidden) so their
            /// editors — and undo history — survive a reopen, and indices stay stable.
            Docs: Doc[]
            Active: int
            /// Bumped on every edit/activation; async results tagged with an older version
            /// are stale and dropped.
            Version: int
            Phase: Phase
            /// FCS diagnostics for the active document, and a token bumped per fresh result.
            Diagnostics: Intellisense.Diagnostic[]
            DiagnosticsToken: int
            /// Set when type-checking passed but evaluation still failed (e.g. a runtime error).
            GenerateError: string option
            /// The last F# generated for the active document ("" before the first success).
            Generated: string
            /// True when `Generated` no longer matches the source (the script has errors).
            IsStale: bool
            GenerateMs: int64
            Tree: SyntaxTree.Tree
            RunOutput: string
            IsRunning: bool
            BottomTab: BottomTab
            CaretLine: int
            CaretColumn: int
            /// Selection requests (token, (offset, length)) for the DSL editor (go to problem)
            /// and the generated pane (syntax-tree click).
            EditorSelect: int * (int * int) option
            GeneratedSelect: int * (int * int) option
            Theme: ThemeVariant
        }

    type Msg =
        | Edit of doc: int * source: string
        | Settle of version: int
        | Checked of version: int * Intellisense.Diagnostic[]
        | GenerateDone of version: int * Result<string, string> * elapsedMs: int64
        | TreeParsed of source: string * SyntaxTree.Tree
        | Activate of int
        | Close of int
        | NewDoc
        | OpenExample of string
        | Run
        | RunDone of Result<string, string>
        | ShowBottom of BottomTab
        | GoToProblem of Intellisense.Diagnostic
        | TreeLineClicked of int
        | CaretMoved of doc: int * line: int * column: int
        | CopyGenerated
        | SetTheme of ThemeVariant
        | KeyPressed of KeyEventArgs
        | SaveSession
        | NoOp

    /// How long after the last keystroke before re-checking and regenerating.
    let private debounceMs = 400

    let private hasErrors(diags: Intellisense.Diagnostic[]) = diags |> Array.exists _.IsError

    let private nextUntitled(docs: Doc[]) =
        let names = docs |> Array.map _.Name |> Set.ofArray

        Seq.initInfinite(fun i -> $"Untitled-{i + 1}")
        |> Seq.find(names.Contains >> not)

    /// Settle immediately, but dispatched asynchronously on purpose: a synchronous Cmd.ofMsg
    /// issued while the first view mounts is queued until some other event arrives.
    let private settleNow(version: int) =
        Cmd.OfAsync.perform (fun () -> async { return version }) () Settle

    let init() =
        let docs, active =
            match Session.tryLoad() with
            | Some(saved, active) ->
                let docs =
                    saved
                    |> Array.map(fun d ->
                        { Name = d.Name
                          Source = d.Source
                          IsOpen = d.IsOpen })

                let active = if active >= 0 && active < docs.Length then active else 0
                // Always have the active document open, even if the file was hand-edited.
                docs[active] <- { docs[active] with IsOpen = true }
                docs, active
            | None ->
                let name, source = List.head Examples.all

                [| { Name = name
                     Source = source
                     IsOpen = true } |],
                0

        { Docs = docs
          Active = active
          Version = 0
          Phase = Checking
          Diagnostics = [||]
          DiagnosticsToken = 0
          GenerateError = None
          Generated = ""
          IsStale = false
          GenerateMs = 0L
          Tree = SyntaxTree.message "// Waiting for generated F#…"
          RunOutput = ""
          IsRunning = false
          BottomTab = Problems
          CaretLine = 1
          CaretColumn = 1
          EditorSelect = 0, None
          GeneratedSelect = 0, None
          Theme = ThemeVariant.Default },
        settleNow 0

    // ---- Commands --------------------------------------------------------------------------

    let private afterDebounce(version: int) =
        Cmd.OfAsync.perform
            (fun () ->
                async {
                    do! Async.Sleep debounceMs
                    return version
                })
            ()
            Settle

    let private checkCmd (version: int) (source: string) =
        Cmd.OfAsync.perform
            (fun () ->
                async {
                    try
                        return! Intellisense.diagnostics source
                    with ex ->
                        eprintfn "AstEditor: type-check failed — %O" ex
                        return [||]
                })
            ()
            (fun d -> Checked(version, d))

    let private generateCmd (version: int) (source: string) =
        Cmd.OfAsync.perform
            (fun () ->
                async {
                    // FSI evaluation is synchronous (and slow the first time): keep it off the UI thread.
                    do! Async.SwitchToThreadPool()
                    let sw = Stopwatch.StartNew()
                    let result = Evaluator.generate source
                    return result, sw.ElapsedMilliseconds
                })
            ()
            (fun (result, ms) -> GenerateDone(version, result, ms))

    let private parseTreeCmd(source: string) =
        Cmd.OfAsync.perform
            (fun () ->
                async {
                    match! Intellisense.parse source with
                    | Some tree -> return SyntaxTree.summarize tree
                    | None -> return SyntaxTree.message "// The generated F# could not be parsed."
                })
            ()
            (fun tree -> TreeParsed(source, tree))

    let private runCmd(code: string) =
        Cmd.OfAsync.perform
            (fun () ->
                async {
                    do! Async.SwitchToThreadPool()
                    return Evaluator.run code
                })
            ()
            RunDone

    let private mainWindow() =
        match Application.Current with
        | null -> None
        | app ->
            match app.ApplicationLifetime with
            | :? IClassicDesktopStyleApplicationLifetime as desktop when not(isNull desktop.MainWindow) ->
                Some desktop.MainWindow
            | _ -> None

    let private copyCmd(text: string) : Cmd<Msg> =
        [ fun _ ->
              match mainWindow() with
              | Some w when not(isNull w.Clipboard) -> w.Clipboard.SetTextAsync(text) |> ignore
              | _ -> () ]

    // ---- Update ----------------------------------------------------------------------------

    /// Offset of a 1-based line / 0-based column in `text` (clamped).
    let private offsetOf (text: string) (line: int) (column: int) =
        let mutable offset = 0
        let mutable l = 1

        while l < line && offset < text.Length do
            let nl = text.IndexOf('\n', offset)

            if nl < 0 then offset <- text.Length else offset <- nl + 1

            l <- l + 1

        min text.Length (offset + max 0 column)

    let private rangeOf (text: string) (startLine, startCol, endLine, endCol) =
        let s = offsetOf text startLine startCol
        let e = offsetOf text endLine endCol
        s, max 1 (e - s)

    /// Make `doc` the active, open document and regenerate it from scratch.
    let private activate (doc: int) (model: Model) =
        if doc < 0 || doc >= model.Docs.Length then
            model, Cmd.none
        elif doc = model.Active && model.Docs[doc].IsOpen then
            model, Cmd.none
        else
            let docs = Array.copy model.Docs
            docs[doc] <- { docs[doc] with IsOpen = true }
            let version = model.Version + 1

            { model with
                Docs = docs
                Active = doc
                Version = version
                Phase = Checking
                Diagnostics = [||]
                DiagnosticsToken = model.DiagnosticsToken + 1
                GenerateError = None
                Generated = ""
                IsStale = false
                Tree = SyntaxTree.message "// Waiting for generated F#…" },
            settleNow version

    let private addDoc (name: string) (source: string) (model: Model) =
        let docs =
            Array.append
                model.Docs
                [| { Name = name
                     Source = source
                     IsOpen = true } |]

        activate (docs.Length - 1) { model with Docs = docs }

    let private openDocs(model: Model) =
        [ for i in 0 .. model.Docs.Length - 1 do
              if model.Docs[i].IsOpen then
                  i ]

    let private cmdModifier =
        if OperatingSystem.IsMacOS() then
            KeyModifiers.Meta
        else
            KeyModifiers.Control

    let private trace =
        not(String.IsNullOrEmpty(Environment.GetEnvironmentVariable "ASTEDITOR_TRACE"))

    let rec update msg model =
        if trace then
            eprintfn "msg: %s" ((sprintf "%A" msg).Split('\n')[0])

        try
            updateCore msg model
        with ex ->
            eprintfn "AstEditor: update failed on %A — %O" msg ex
            model, Cmd.none

    and private updateCore msg model =
        match msg with
        | Edit(doc, source) ->
            let docs = Array.copy model.Docs
            docs[doc] <- { docs[doc] with Source = source }
            let version = model.Version + 1

            { model with
                Docs = docs
                Active = doc
                Version = version },
            afterDebounce version

        | Settle version when version = model.Version ->
            { model with Phase = Checking }, checkCmd version model.Docs[model.Active].Source

        | Checked(version, diags) when version = model.Version ->
            let model =
                { model with
                    Diagnostics = diags
                    DiagnosticsToken = model.DiagnosticsToken + 1
                    GenerateError = None }

            if hasErrors diags then
                // Don't evaluate a script that doesn't type-check: FSI would only repeat the
                // same errors. Keep the last good output, marked stale.
                { model with
                    Phase = Done
                    IsStale = model.Generated <> "" },
                Cmd.none
            else
                { model with Phase = Generating }, generateCmd version model.Docs[model.Active].Source

        | GenerateDone(version, result, ms) when version = model.Version ->
            match result with
            | Ok source ->
                { model with
                    Phase = Done
                    Generated = source
                    IsStale = false
                    GenerateMs = ms },
                parseTreeCmd source
            | Error message ->
                { model with
                    Phase = Done
                    GenerateError = Some message
                    IsStale = model.Generated <> "" },
                Cmd.none

        | Settle _
        | Checked _
        | GenerateDone _ -> model, Cmd.none // stale: a newer edit is in flight

        | TreeParsed(source, tree) when source = model.Generated -> { model with Tree = tree }, Cmd.none
        | TreeParsed _ -> model, Cmd.none

        | Activate doc -> activate doc model

        | Close doc ->
            let open' = openDocs model

            if not model.Docs[doc].IsOpen || open'.Length <= 1 then
                model, Cmd.none // the last open document stays open
            else
                let docs = Array.copy model.Docs
                docs[doc] <- { docs[doc] with IsOpen = false }
                let model = { model with Docs = docs }

                if doc <> model.Active then
                    model, Cmd.none
                else
                    // Activate the neighbour to the left (or right, when closing the first tab).
                    let pos = open' |> List.findIndex((=) doc)
                    let next = if pos > 0 then open'[pos - 1] else open'[pos + 1]
                    activate next model

        | NewDoc -> addDoc (nextUntitled model.Docs) Examples.blank model

        | OpenExample name ->
            match model.Docs |> Array.tryFindIndex(fun d -> d.Name = name) with
            | Some i -> activate i model
            | None ->
                let source = Examples.all |> List.find(fst >> (=) name) |> snd
                addDoc name source model

        | Run when model.Generated <> "" && not model.IsStale && not model.IsRunning ->
            { model with
                IsRunning = true
                BottomTab = Output
                RunOutput = "Running…" },
            runCmd model.Generated
        | Run -> model, Cmd.none

        | RunDone result ->
            let text =
                match result with
                | Ok output -> output
                | Error message -> "Run failed:\n" + message

            { model with
                IsRunning = false
                RunOutput = text },
            Cmd.none

        | ShowBottom tab -> { model with BottomTab = tab }, Cmd.none

        | GoToProblem d ->
            let range =
                rangeOf model.Docs[model.Active].Source (d.StartLine, d.StartColumn, d.EndLine, d.EndColumn)

            { model with EditorSelect = fst model.EditorSelect + 1, Some range }, Cmd.none

        | TreeLineClicked line ->
            match model.Tree.Spans |> Array.tryItem(line - 1) |> Option.flatten with
            | Some s ->
                let range =
                    rangeOf model.Generated (s.StartLine, s.StartColumn, s.EndLine, s.EndColumn)

                { model with GeneratedSelect = fst model.GeneratedSelect + 1, Some range }, Cmd.none
            | None -> model, Cmd.none

        | CaretMoved(doc, line, column) when doc = model.Active ->
            { model with
                CaretLine = line
                CaretColumn = column },
            Cmd.none
        | CaretMoved _ -> model, Cmd.none

        | CopyGenerated -> model, copyCmd model.Generated

        | SetTheme variant -> { model with Theme = variant }, Cmd.none

        | KeyPressed e when e.KeyModifiers = cmdModifier ->
            // Window-level shortcuts. These bubble up from the editor, which doesn't handle
            // Cmd/Ctrl+Enter, +N or +W itself.
            match e.Key with
            | Key.Enter ->
                e.Handled <- true
                updateCore Run model
            | Key.N ->
                e.Handled <- true
                updateCore NewDoc model
            | Key.W ->
                e.Handled <- true
                updateCore (Close model.Active) model
            | _ -> model, Cmd.none
        | KeyPressed _ -> model, Cmd.none

        | SaveSession ->
            Session.save
                (model.Docs
                 |> Array.map(fun d ->
                     { Session.SavedDoc.Name = d.Name
                       Source = d.Source
                       IsOpen = d.IsOpen }))
                model.Active

            model, Cmd.none

        | NoOp -> model, Cmd.none

    // ---- View ------------------------------------------------------------------------------
    // Colours, hover states and light/dark variants live in Styles.axaml; the view only
    // assigns classes, so the theme switch (and "System" following the OS) is live.

    let private shortcut(key: string) =
        if OperatingSystem.IsMacOS() then
            "⌘" + key
        else
            "Ctrl+" + key

    let private newKey = shortcut "N"
    let private runKey = shortcut "Enter"
    let private closeKey = shortcut "W"

    let private libraryVersion =
        let asm = typeof<Fabulous.AST.Ast>.Assembly

        match
            asm.GetCustomAttributes(typeof<Reflection.AssemblyInformationalVersionAttribute>, false)
            |> Array.tryHead
        with
        | Some(:? Reflection.AssemblyInformationalVersionAttribute as a) -> a.InformationalVersion.Split('+')[0]
        | _ -> string(asm.GetName().Version)

    /// Height of the tab strips and panel headers, so their edges line up across panes.
    let private stripHeight = 35.

    let private text (classes: string list) (value: string) =
        TextBlock(value).classes(classes).centerVertical()

    let private toolbar(model: Model) =
        let themeItem (label: string) (variant: ThemeVariant) =
            MenuItem((if model.Theme = variant then "✓  " else "     ") + label).onClick(fun _ -> SetTheme variant)

        let canRun = model.Generated <> "" && not model.IsStale && not model.IsRunning

        Border(
            (Grid(coldefs = [ Auto; Auto; Star; Auto; Auto ], rowdefs = [ Auto ]) {
                Button("New", NewDoc).classes([ "flat" ]).tip(ToolTip($"New file ({newKey})"))

                DropDownButton("Examples", (fun _ -> NoOp))
                    .classes([ "flat" ])
                    .flyout(
                        MenuFlyout() {
                            for name, _ in Examples.all do
                                MenuItem(name).onClick(fun _ -> OpenExample name)
                        }
                    )
                    .gridColumn(1)

                DropDownButton("Theme", (fun _ -> NoOp))
                    .classes([ "flat" ])
                    .flyout(
                        MenuFlyout() {
                            themeItem "System" ThemeVariant.Default
                            themeItem "Light" ThemeVariant.Light
                            themeItem "Dark" ThemeVariant.Dark
                        }
                    )
                    .margin(0., 0., 8., 0.)
                    .gridColumn(3)

                Button((if model.IsRunning then "Running…" else "▶  Run"), Run)
                    .classes([ "accent"; "run" ])
                    .isEnabled(canRun)
                    .tip(ToolTip($"Run the generated F# ({runKey})"))
                    .gridColumn(4)
            })
                .margin(6., 5.)
        )
            .classes([ "chrome"; "toolbar" ])

    /// A document tab: F# glyph + name (click to activate) and a close button that shows on
    /// hover / when active. The title and close are separate buttons so closing never also
    /// activates the tab.
    let private tab (name: string) (isActive: bool) (onSelect: Msg) (onClose: Msg option) =
        Border(
            Grid() {
                Border().classes([ "tabAccent" ])

                HStack(0.) {
                    Button(
                        onSelect,
                        HStack(6.) {
                            text [ "fsharp" ] "F#"
                            text [ "tabTitle" ] name
                        }
                    )
                        .classes([ "tabButton" ])

                    match onClose with
                    | Some close ->
                        Button("×", close)
                            .classes([ "flat"; "tabClose" ])
                            .margin(0., 0., 8., 0.)
                            .tip(ToolTip($"Close ({closeKey})"))
                    | None -> ()
                }
            }
        )
            .classes(if isActive then [ "tab"; "active" ] else [ "tab" ])

    /// Tabs on the left, then a filler (carrying the strip's bottom edge) holding `extra`.
    let private tabStrip
        (tabs: WidgetBuilder<Msg, IFabReversibleStackPanel>)
        (extra: WidgetBuilder<Msg, #IFabControl>)
        =
        Grid(coldefs = [ Auto; Star ], rowdefs = [ Star ]) {
            tabs
            Border(extra).classes([ "chrome"; "tabFiller" ]).gridColumn(1)
        }

    let private documentTabs(model: Model) =
        let open' = openDocs model

        tabStrip
            (HStack(0.) {
                for i in open' do
                    tab
                        model.Docs[i].Name
                        (i = model.Active)
                        (Activate i)
                        (if open'.Length > 1 then Some(Close i) else None)
            })
            (Button("+", NewDoc)
                .classes([ "flat" ])
                .fontSize(16.)
                .padding(8., 0.)
                .margin(4., 0.)
                .horizontalAlignment(HorizontalAlignment.Left)
                .tip(ToolTip($"New file ({newKey})")))

    /// One editor per document, stacked in one cell; only the active one is visible. Hidden
    /// editors keep their own undo history and caret.
    let private dslEditors(model: Model) =
        Grid() {
            for i in 0 .. model.Docs.Length - 1 do
                let isActive = i = model.Active

                TextEditor(model.Docs[i].Source, (fun s -> Edit(i, s)))
                    .showLineNumbers(true)
                    .highlightFSharp()
                    .intelliSense()
                    .diagnostics(
                        (if isActive then model.DiagnosticsToken else 0),
                        (if isActive then model.Diagnostics else [||])
                    )
                    .onCaretMoved(fun (line, column) -> CaretMoved(i, line, column))
                    .selectRange(
                        (if isActive then fst model.EditorSelect else 0),
                        (if isActive then snd model.EditorSelect else None)
                    )
                    .isVisible(isActive && model.Docs[i].IsOpen)
        }

    let private generatedPane(model: Model) =
        let placeholder =
            match model.Phase with
            | _ when model.Generated <> "" -> None
            | Checking
            | Generating -> Some "Generating…"
            | Done -> Some "Fix the problems in your script to see the generated F#."

        (Grid(coldefs = [ Star ], rowdefs = [ Auto; Auto; Star ]) {
            tabStrip
                (HStack(0.) { tab "Generated.fs" true NoOp None })
                ((HStack(4.) {
                    text [ "dim" ] "read-only"
                    Button("Copy", CopyGenerated).classes([ "flat" ]).isEnabled(model.Generated <> "")
                })
                    .horizontalAlignment(HorizontalAlignment.Right)
                    .margin(0., 0., 6., 0.))

            Border(text [ "text" ] "⚠  Out of date — the script has problems. Showing the last successful output.")
                .classes([ "banner" ])
                .padding(12., 6.)
                .isVisible(model.IsStale)
                .gridRow(1)

            TextEditor(model.Generated)
                .isReadOnly(true)
                .showLineNumbers(model.Generated <> "")
                .highlightFSharp()
                .selectRange(fst model.GeneratedSelect, snd model.GeneratedSelect)
                .opacity(if model.IsStale then 0.55 else 1.0)
                .gridRow(2)

            match placeholder with
            | Some message -> (text [ "dim" ] message).centerHorizontal().gridRow(2)
            | None -> ()
        })

    let private problemCounts(model: Model) =
        let errors =
            (model.Diagnostics |> Array.filter _.IsError).Length
            + (if model.GenerateError.IsSome then 1 else 0)

        let warnings = (model.Diagnostics |> Array.filter(_.IsError >> not)).Length
        errors, warnings

    /// The problems list: FCS diagnostics, plus an evaluation failure when the script
    /// type-checked but still failed to generate. Clicking a diagnostic selects its range.
    let private problemsPane(model: Model) =
        let diags =
            model.Diagnostics
            |> Array.sortBy(fun d -> (not d.IsError), d.StartLine, d.StartColumn)

        let row (isError: bool) (message: string) (location: string) (msg: Msg) =
            Button(
                msg,
                (Grid(coldefs = [ Auto; Star; Auto ], rowdefs = [ Auto ]) {
                    (text [ (if isError then "error" else "warning") ] (if isError then "✕" else "⚠"))
                        .margin(0., 0., 10., 0.)

                    (text [ "text" ] message).textWrapping(TextWrapping.Wrap).gridColumn(1)
                    (text [ "dim" ] location).margin(16., 0., 0., 0.).gridColumn(2)
                })
            )
                .classes([ "flat"; "row" ])

        if diags.Length = 0 && model.GenerateError.IsNone then
            AnyView(
                (text
                    [ "dim" ]
                    (if model.Phase = Done then
                         "No problems have been detected."
                     else
                         "Checking…"))
                    .margin(14., 8.)
                    .verticalAlignment(VerticalAlignment.Top)
            )
        else
            AnyView(
                ScrollViewer(
                    VStack(0.) {
                        match model.GenerateError with
                        | Some message -> row true ("Generation failed: " + message) "" NoOp
                        | None -> ()

                        for d in diags do
                            row d.IsError d.Message $"Ln {d.StartLine}, Col {d.StartColumn + 1}" (GoToProblem d)
                    }
                )
            )

    let private outputPane(model: Model) =
        if model.RunOutput = "" then
            AnyView(
                (text [ "dim" ] $"Press ▶ Run ({runKey}) to execute the generated F#.")
                    .margin(14., 8.)
                    .verticalAlignment(VerticalAlignment.Top)
            )
        else
            AnyView(
                TextEditor(model.RunOutput).isReadOnly(true).showLineNumbers(false).wordWrap(true).classes([ "panel" ])
            )

    let private treePane(model: Model) =
        TextEditor(model.Tree.Text)
            .isReadOnly(true)
            .showLineNumbers(false)
            .classes([ "panel" ])
            .onCaretMoved(fun (line, _) -> TreeLineClicked line)

    let private bottomPanel(model: Model) =
        let errors, warnings = problemCounts model

        let panelTab (label: string) (badge: int) (tab: BottomTab) =
            Border(
                Button(
                    ShowBottom tab,
                    HStack(0.) {
                        text [] label

                        if badge > 0 then
                            Border(TextBlock(string badge)).classes([ "badge" ])
                    }
                )
                    .classes([ "flat" ])
            )
                .classes(
                    if model.BottomTab = tab then
                        [ "panelTab"; "active" ]
                    else
                        [ "panelTab" ]
                )

        (Grid(coldefs = [ Star ], rowdefs = [ Auto; Star ]) {
            (HStack(0.) {
                panelTab "PROBLEMS" (errors + warnings) Problems
                panelTab "OUTPUT" 0 Output
                panelTab "SYNTAX TREE" 0 SyntaxTree
            })
                .margin(12., 0.)
                .height(stripHeight)

            (problemsPane model).isVisible(model.BottomTab = Problems).gridRow(1)
            (outputPane model).isVisible(model.BottomTab = Output).gridRow(1)
            (treePane model).isVisible(model.BottomTab = SyntaxTree).gridRow(1)
        })

    let private statusBar(model: Model) =
        let errors, warnings = problemCounts model

        let state, stateClass =
            match model.Phase with
            | Checking -> "◌  Checking…", "dim"
            | Generating -> "◌  Generating…", "dim"
            | Done when errors > 0 -> $"✕  {errors} error" + (if errors = 1 then "" else "s"), "error"
            | Done when warnings > 0 -> $"⚠  {warnings} warning" + (if warnings = 1 then "" else "s"), "warning"
            | Done -> $"✓  Generated in {model.GenerateMs} ms", "ok"

        Border(
            (Grid(coldefs = [ Auto; Star; Auto; Auto ], rowdefs = [ Auto ]) {
                Button(ShowBottom Problems, text [ stateClass ] state).classes([ "flat" ])
                (text [ "dim" ] $"Ln {model.CaretLine}, Col {model.CaretColumn}").margin(0., 0., 18., 0.).gridColumn(2)
                (text [ "dim" ] $"Fabulous.AST {libraryVersion}").margin(0., 0., 8., 0.).gridColumn(3)
            })
                .margin(4., 1.)
        )
            .classes([ "chrome"; "statusBar" ])

    let private splitter(direction: GridResizeDirection) : WidgetBuilder<Msg, IFabGridSplitter> =
        GridSplitter(direction)

    let private shell(model: Model) =
        let editorColumn: WidgetBuilder<Msg, IFabGrid> =
            Grid(coldefs = [ Star ], rowdefs = [ Auto; Star ]) {
                documentTabs model
                (dslEditors model).gridRow(1)
            }

        let mainArea: WidgetBuilder<Msg, IFabGrid> =
            Grid(coldefs = [ Star; Pixel 1.; Star ], rowdefs = [ Star ]) {
                editorColumn
                (splitter GridResizeDirection.Columns).gridColumn(1)
                (generatedPane model).gridColumn(2)
            }

        Grid(coldefs = [ Star ], rowdefs = [ Auto; Star; Pixel 1.; Pixel 230.; Auto ]) {
            toolbar model
            mainArea.gridRow(1)
            (splitter GridResizeDirection.Rows).gridRow(2)
            (Border(bottomPanel model)).classes([ "chrome" ]).gridRow(3)
            (statusBar model).gridRow(4)
        }

    let view model =
        (DesktopApplication() {
            Window(shell model)
                .title("Fabulous.AST Playground")
                .width(1400.)
                .height(880.)
                .styleInclude("avares://AstEditor/Styles.axaml")
                .onKeyDown(KeyPressed)
                .onWindowClosing(fun _ -> SaveSession)
        })
            .requestedThemeVariant(model.Theme)

    let create() =
        let program = Program.statefulWithCmd init update |> Program.withView view

        // Surface MVU-loop errors (view/update/commands) on stderr instead of dropping them,
        // which otherwise leaves the UI frozen with no clue why.
        let program =
            { program with
                State =
                    { program.State with
                        ExceptionHandler =
                            fun ex ->
                                eprintfn "AstEditor: unhandled error — %O" ex
                                true } }

        FabulousAppBuilder.Configure(FluentTheme, program)
