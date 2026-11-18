namespace AstEditor

open System
open System.Runtime.CompilerServices
open Avalonia
open Avalonia.Markup.Xaml.Styling
open AvaloniaEdit.TextMate
open TextMateSharp.Grammars
open Fabulous
open Fabulous.ScalarAttributeDefinitions
open Fabulous.Avalonia

// AvaloniaEdit's editor control. Aliased so it doesn't clash with the `TextEditor` module
// (which holds the Fabulous attributes) or the `View.TextEditor` constructor below.
type private AvEdit = AvaloniaEdit.TextEditor

/// A Fabulous.Avalonia binding for AvaloniaEdit's TextEditor — a real code editor with line
/// numbers, undo, and (via TextMate) syntax highlighting. Hand-written because no official
/// binding exists; it follows the same shape Fabulous.Avalonia uses for its own controls.
///
/// TextEditor : TemplatedControl, so the marker interface inherits IFabTemplatedControl and
/// the usual modifiers (fontFamily, fontSize, margin, …) apply for free.
type IFabTextEditor =
    inherit IFabTemplatedControl

/// Switches the TextMate syntax theme on every editor at once, so they follow the app's
/// light/dark variant. (DarkPlus / LightPlus — the VS Code Dark+/Light+ palettes.)
///
/// The source of truth is the application's *resolved* variant (ActualThemeVariant): picking
/// "System" follows the real OS setting, and an OS theme change at runtime re-themes too.
module EditorTheme =
    let mutable private dark = true

    // Weakly keyed by editor, so a discarded editor (and its document) can be collected.
    let private installs =
        ConditionalWeakTable<AvaloniaEdit.TextEditor, TextMate.Installation * RegistryOptions>()

    let themeName() =
        if dark then ThemeName.DarkPlus else ThemeName.LightPlus

    let private apply(isDark: bool) =
        dark <- isDark

        for KeyValue(_, (installation, options)) in installs do
            try
                installation.SetTheme(options.LoadTheme(themeName()))
            with _ ->
                ()

    let mutable private hooked = false

    /// Follow the app's resolved variant from now on (idempotent; called once an editor
    /// materializes, so Application.Current is live).
    let private hookApp() =
        if not hooked then
            match Application.Current with
            | null -> () // app not up yet; a later editor will retry
            | app ->
                hooked <- true

                app.ActualThemeVariantChanged.Add(fun _ ->
                    apply(app.ActualThemeVariant = Avalonia.Styling.ThemeVariant.Dark))

                apply(app.ActualThemeVariant = Avalonia.Styling.ThemeVariant.Dark)

    let register (editor: AvaloniaEdit.TextEditor) (installation: TextMate.Installation) (options: RegistryOptions) =
        installs.AddOrUpdate(editor, (installation, options))
        hookApp()

[<AutoOpen>]
module private AvaloniaEditInterop =

    // AvaloniaEdit ships its own control theme; without it the editor renders unstyled. Add it
    // to the running app once, lazily, the first time an editor materializes (Application.Current
    // is live by then). Tolerant of a bad path so a theme miss never crashes the sample.
    let mutable private stylesAdded = false

    let ensureStyles() =
        if not stylesAdded then
            match Application.Current with
            | null -> () // app not up yet; a later editor will retry
            | app ->
                stylesAdded <- true

                try
                    let style = StyleInclude(null: Uri)
                    style.Source <- Uri "avares://AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml"
                    app.Styles.Add style
                with ex ->
                    // Surface it rather than hide it: a wrong path here leaves editors blank.
                    eprintfn "AstEditor: failed to load AvaloniaEdit theme — %s" ex.Message

    // TextMate is installed imperatively per control instance; track installs so we don't
    // double-install on re-render.
    let private installs = ConditionalWeakTable<AvEdit, obj>()

    // Set while the *model* changes an editor (replacing its text, selecting a range), so the
    // caret handler doesn't report those moves back as if the user made them. Without
    // this, refreshing the syntax-tree text moves its caret, which reads as a click on a node.
    let private suppressEvents = ConditionalWeakTable<AvEdit, bool ref>()

    let suppressRef(editor: AvEdit) =
        suppressEvents.GetValue(editor, fun _ -> ref false)

    /// Run a programmatic change with the editor's caret/selection events muted. The flag is
    /// cleared on a later dispatcher turn, since AvaloniaEdit raises some caret moves late.
    let programmatic (editor: AvEdit) (change: unit -> unit) =
        let flag = suppressRef editor
        flag.Value <- true

        try
            change()
        finally
            Avalonia.Threading.Dispatcher.UIThread.Post(fun () -> flag.Value <- false)

    let installTextMate(editor: AvEdit) =
        match installs.TryGetValue editor with
        | true, _ -> ()
        | _ ->
            try
                let registryOptions = RegistryOptions(EditorTheme.themeName())
                let installation = editor.InstallTextMate(registryOptions)
                let language = registryOptions.GetLanguageByExtension(".fs")

                if not(isNull(box language)) then
                    installation.SetGrammar(registryOptions.GetScopeByLanguageId(language.Id))

                EditorTheme.register editor installation registryOptions
                installs.Add(editor, box installation)
            with _ ->
                ()

module TextEditor =
    let WidgetKey = Widgets.register<AvEdit>()

    let ShowLineNumbers =
        Attributes.defineAvaloniaPropertyWithEquality AvEdit.ShowLineNumbersProperty

    let WordWrap = Attributes.defineAvaloniaPropertyWithEquality AvEdit.WordWrapProperty

    let IsReadOnly =
        Attributes.defineAvaloniaPropertyWithEquality AvEdit.IsReadOnlyProperty

    /// Set-only text, for read-only/display editors that never raise edits back to the model.
    let Text =
        Attributes.defineSimpleScalarWithEquality<string> "TextEditor_Text" (fun _ newValueOpt node ->
            let editor = node.Target :?> AvEdit
            ensureStyles()

            match newValueOpt with
            | ValueSome v when editor.Text <> v -> programmatic editor (fun () -> editor.Text <- v)
            | _ -> ())

    /// Enables FCS-backed DSL IntelliSense — autocomplete (incl. member completion) and
    /// hover tooltips — for this editor.
    let IntelliSense =
        Attributes.defineSimpleScalarWithEquality<bool> "TextEditor_IntelliSense" (fun _ newValueOpt node ->
            match newValueOpt with
            | ValueSome true ->
                let editor = node.Target :?> AvEdit
                Completion.install editor
                Hover.install editor
                SignatureHelp.install editor
            | _ -> ())

    /// Draws these diagnostics as squiggles. Carries a token so the same diagnostics are
    /// re-applied after an edit cleared them (equality alone would skip the update).
    let Diagnostics =
        Attributes.defineSimpleScalarWithEquality<int * Intellisense.Diagnostic[]>
            "TextEditor_Diagnostics"
            (fun _ newValueOpt node ->
                let diags =
                    match newValueOpt with
                    | ValueSome(_, d) -> d
                    | ValueNone -> [||]

                Squiggles.set (node.Target :?> AvEdit) diags)

    /// Raises (line, column) — both 1-based — whenever the caret moves.
    let CaretMoved: SimpleScalarAttributeDefinition<int * int -> MsgValue> =
        let name = "TextEditor_CaretMoved"

        let key =
            SimpleScalarAttributeDefinition.CreateAttributeData(
                ScalarAttributeComparers.noCompare,
                (fun _ (newValueOpt: (int * int -> MsgValue) voption) (node: IViewNode) ->
                    let editor = node.Target :?> AvEdit

                    match node.TryGetHandler(name) with
                    | ValueNone -> ()
                    | ValueSome handler -> handler.Dispose()

                    match newValueOpt with
                    | ValueNone -> node.RemoveHandler(name)
                    | ValueSome fn ->
                        let handler =
                            editor.TextArea.Caret.PositionChanged.Subscribe(fun _ ->
                                if not (suppressRef editor).Value then
                                    let caret = editor.TextArea.Caret
                                    let (MsgValue r) = fn(caret.Line, caret.Column)
                                    Dispatcher.dispatch node r)

                        node.SetHandler(name, handler))
            )
            |> AttributeDefinitionStore.registerScalar

        { Key = key
          Name = name }

    /// Programmatically selects a range (start offset, length) in the editor and scrolls it
    /// into view — used to highlight the DSL that produced a clicked syntax-tree node. Carries
    /// a token so each distinct request applies once (equality skips unchanged renders); a
    /// ValueNone range leaves the current selection alone.
    let SelectRange =
        Attributes.defineSimpleScalarWithEquality<int * (int * int) option>
            "TextEditor_SelectRange"
            (fun _ newValueOpt node ->
                match newValueOpt with
                | ValueSome(_, Some(start, length)) when length > 0 ->
                    let editor = node.Target :?> AvEdit
                    let len = editor.Document.TextLength
                    let s = max 0 (min start len)
                    let l = max 0 (min length (len - s))

                    if l > 0 then
                        programmatic editor (fun () ->
                            editor.Select(s, l)
                            editor.TextArea.Caret.BringCaretToView()
                            editor.TextArea.Focus() |> ignore)
                | _ -> ())

    /// Turns on TextMate F# highlighting for this editor instance. Uses no-compare so a first
    /// install that fails (e.g. control not ready) is retried on the next render; once the
    /// ConditionalWeakTable records success, subsequent applies are no-ops.
    let HighlightFSharp: SimpleScalarAttributeDefinition<bool> =
        let name = "TextEditor_HighlightFSharp"

        let key =
            SimpleScalarAttributeDefinition.CreateAttributeData(
                ScalarAttributeComparers.noCompare,
                (fun _ (newValueOpt: bool voption) (node: IViewNode) ->
                    match newValueOpt with
                    | ValueSome true -> installTextMate(node.Target :?> AvEdit)
                    | _ -> ())
            )
            |> AttributeDefinitionStore.registerScalar

        { Key = key
          Name = name }

    /// Two-way text. AvaloniaEdit's `Text` is a plain CLR property (no AvaloniaProperty) and
    /// `TextChanged` is a parameterless EventHandler, so neither `defineAvaloniaProperty*` nor
    /// `defineEvent<'args>` fits — we wire it by hand. The teardown-before-set ordering means a
    /// programmatic text update can't re-enter dispatch, which is what keeps the caret stable.
    let TextWithChangedEvent: SimpleScalarAttributeDefinition<ValueEventData<string, string>> =
        let name = "TextEditor_TextWithChangedEvent"

        let key =
            SimpleScalarAttributeDefinition.CreateAttributeData(
                ScalarAttributeComparers.noCompare,
                (fun _ (newValueOpt: ValueEventData<string, string> voption) (node: IViewNode) ->
                    let editor = node.Target :?> AvEdit
                    ensureStyles()

                    // Tear down the previous subscription first so the set below is silent.
                    match node.TryGetHandler(name) with
                    | ValueNone -> ()
                    | ValueSome handler -> handler.Dispose()

                    match newValueOpt with
                    | ValueNone -> node.RemoveHandler(name)
                    | ValueSome data ->
                        match data.Value with
                        | ValueSome v when editor.Text <> v -> programmatic editor (fun () -> editor.Text <- v)
                        | _ -> ()

                        let handler =
                            editor.TextChanged.Subscribe(fun _ ->
                                // Ignore the echo from our own programmatic set: if the editor's
                                // text already equals the model value we applied, this fired for a
                                // set, not a user edit. Guards against AvaloniaEdit raising
                                // TextChanged asynchronously (after we've resubscribed).
                                let isEcho =
                                    match data.Value with
                                    | ValueSome v -> editor.Text = v
                                    | ValueNone -> false

                                if not isEcho then
                                    let (MsgValue r) = data.Event editor.Text
                                    Dispatcher.dispatch node r)

                        node.SetHandler(name, handler))
            )
            |> AttributeDefinitionStore.registerScalar

        { Key = key
          Name = name }

[<AutoOpen>]
module TextEditorBuilders =
    type Fabulous.Avalonia.View with

        /// Creates an AvaloniaEdit code editor bound to <paramref name="text"/>, raising
        /// <paramref name="onTextChanged"/> with the new text on every edit.
        static member inline TextEditor(text: string, onTextChanged: string -> 'msg) =
            WidgetBuilder<'msg, IFabTextEditor>(
                TextEditor.WidgetKey,
                TextEditor.TextWithChangedEvent.WithValue(ValueEventData.create text onTextChanged)
            )

        /// Creates a read-only/display AvaloniaEdit editor showing <paramref name="text"/>.
        /// It sets text but subscribes to no edit event — for panes the user can't type into.
        static member inline TextEditor(text: string) =
            WidgetBuilder<'msg, IFabTextEditor>(TextEditor.WidgetKey, TextEditor.Text.WithValue(text))

type TextEditorModifiers =
    [<Extension>]
    static member inline showLineNumbers(this: WidgetBuilder<'msg, #IFabTextEditor>, value: bool) =
        this.AddScalar(TextEditor.ShowLineNumbers.WithValue(value))

    [<Extension>]
    static member inline wordWrap(this: WidgetBuilder<'msg, #IFabTextEditor>, value: bool) =
        this.AddScalar(TextEditor.WordWrap.WithValue(value))

    [<Extension>]
    static member inline isReadOnly(this: WidgetBuilder<'msg, #IFabTextEditor>, value: bool) =
        this.AddScalar(TextEditor.IsReadOnly.WithValue(value))

    /// Enables TextMate F# syntax highlighting.
    [<Extension>]
    static member inline highlightFSharp(this: WidgetBuilder<'msg, #IFabTextEditor>) =
        this.AddScalar(TextEditor.HighlightFSharp.WithValue(true))

    /// Enables FCS-backed DSL autocomplete.
    [<Extension>]
    static member inline intelliSense(this: WidgetBuilder<'msg, #IFabTextEditor>) =
        this.AddScalar(TextEditor.IntelliSense.WithValue(true))

    /// Shows diagnostics as squiggles. Bump `token` whenever a fresh check result arrives.
    [<Extension>]
    static member inline diagnostics
        (this: WidgetBuilder<'msg, #IFabTextEditor>, token: int, diags: Intellisense.Diagnostic[])
        =
        this.AddScalar(TextEditor.Diagnostics.WithValue((token, diags)))

    /// Raises (line, column) — 1-based — whenever the caret moves.
    [<Extension>]
    static member inline onCaretMoved(this: WidgetBuilder<'msg, #IFabTextEditor>, fn: int * int -> 'msg) =
        this.AddScalar(TextEditor.CaretMoved.WithValue(fn >> box >> MsgValue))

    /// Selects a (start offset, length) range, scrolling it into view. `token` makes each
    /// distinct request apply once; pass None to leave the selection untouched.
    [<Extension>]
    static member inline selectRange
        (this: WidgetBuilder<'msg, #IFabTextEditor>, token: int, range: (int * int) option)
        =
        this.AddScalar(TextEditor.SelectRange.WithValue((token, range)))
