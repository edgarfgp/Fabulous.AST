namespace AstEditor

open System.Runtime.CompilerServices
open Avalonia
open Avalonia.Input
open Avalonia.Media
open AvaloniaEdit
open AvaloniaEdit.Editing
open AvaloniaEdit.Rendering

/// A code-action lightbulb in the editor's left gutter: lines that have a quick fix (or the
/// Rewrite action) get a 💡; clicking it opens the same picker as Ctrl+. on that line.
module Lightbulb =

    let private bulbBrush = SolidColorBrush(Color.Parse "#E5C100") :> IBrush // gold
    let private baseBrush = SolidColorBrush(Color.Parse "#9A9A9A") :> IBrush
    let private marginWidth = 18.0

    type private LightbulbMargin(editor: TextEditor) =
        inherit AbstractMargin()

        override _.MeasureOverride(_: Size) = Size(marginWidth, 0.0)

        override this.OnTextViewVisualLinesChanged() = this.InvalidateVisual()

        override this.Render(dc: DrawingContext) =
            let textView = this.TextView

            if not(isNull textView) && textView.VisualLinesValid then
                let lines = QuickFix.actionLines editor

                if not(Set.isEmpty lines) then
                    for visualLine in textView.VisualLines do
                        if lines.Contains visualLine.FirstDocumentLine.LineNumber then
                            let top =
                                visualLine.GetTextLineVisualYPosition(visualLine.TextLines[0], VisualYPosition.TextTop)
                                - textView.VerticalOffset

                            let cy = top + visualLine.Height / 2.0
                            // A small bulb: gold disc with a tiny base.
                            dc.DrawEllipse(bulbBrush, null, Point(marginWidth / 2.0, cy - 1.0), 5.0, 5.0)
                            dc.FillRectangle(baseBrush, Rect(marginWidth / 2.0 - 2.0, cy + 4.0, 4.0, 2.0))

        override this.OnPointerPressed(e: PointerPressedEventArgs) =
            base.OnPointerPressed(e)
            let textView = this.TextView

            if not(isNull textView) then
                let y = e.GetPosition(this).Y + textView.VerticalOffset
                let visualLine = textView.GetVisualLineFromVisualTop(y)

                if not(isNull visualLine) then
                    let line = visualLine.FirstDocumentLine.LineNumber

                    if (QuickFix.actionLines editor).Contains line then
                        QuickFix.popForLine editor line
                        e.Handled <- true

    let private installed = ConditionalWeakTable<TextEditor, obj>()

    /// Add the lightbulb gutter to an editor (idempotent per instance).
    let install(editor: TextEditor) =
        match installed.TryGetValue editor with
        | true, _ -> ()
        | _ ->
            installed.Add(editor, box())
            let margin = LightbulbMargin(editor)
            // Leftmost gutter column, before the line numbers. Set TextView explicitly so the
            // margin is connected even if adding to LeftMargins doesn't wire it.
            margin.TextView <- editor.TextArea.TextView
            editor.TextArea.LeftMargins.Insert(0, margin)

            DiagnosticsStore.onChanged(fun ed ->
                if obj.ReferenceEquals(ed, editor) then
                    margin.InvalidateVisual())
