namespace AstEditor

open System.Runtime.CompilerServices
open Avalonia
open Avalonia.Media
open AvaloniaEdit
open AvaloniaEdit.Document
open AvaloniaEdit.Rendering

/// Inline error/warning squiggles for the DSL editor. A background renderer draws wavy
/// underlines under each diagnostic range; the diagnostics themselves come from the model
/// (App runs the debounced FCS check), so squiggles and the Problems panel always agree.
module Squiggles =

    /// A wavy underline along the bottom of a rect (the classic squiggle).
    let private squiggle(r: Rect) : Geometry =
        let geometry = StreamGeometry()
        use ctx = geometry.Open()
        let y = r.Bottom - 1.0
        let amplitude = 2.5
        let step = 3.0
        ctx.BeginFigure(Point(r.Left, y), false)
        let mutable x = r.Left
        let mutable up = true

        while x < r.Right do
            let nextX = min (x + step) r.Right
            ctx.LineTo(Point(nextX, (if up then y - amplitude else y)))
            x <- nextX
            up <- not up

        ctx.EndFigure(false)
        geometry :> Geometry

    type private SquiggleRenderer() =
        let mutable markers: struct (int * int * IPen)[] = [||]

        member _.SetMarkers(m) = markers <- m

        interface IBackgroundRenderer with
            member _.Layer = KnownLayer.Selection

            member _.Draw(textView: TextView, dc: DrawingContext) =
                if markers.Length > 0 && textView.VisualLinesValid then
                    for struct (startOffset, endOffset, pen) in markers do
                        let segment =
                            TextSegment(StartOffset = startOffset, Length = endOffset - startOffset)

                        for rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, segment) do
                            dc.DrawGeometry(null, pen, squiggle rect)

    let private renderers = ConditionalWeakTable<TextEditor, SquiggleRenderer>()

    let private errorPen = Pen(SolidColorBrush(Color.Parse "#F14C4C"), 1.0) :> IPen
    let private warningPen = Pen(SolidColorBrush(Color.Parse "#CCA700"), 1.0) :> IPen

    /// The editor's squiggle renderer, installed on first use. Any edit clears the squiggles
    /// (their ranges describe the previous text) until the model supplies fresh diagnostics.
    let private rendererFor(editor: TextEditor) =
        match renderers.TryGetValue editor with
        | true, r -> r
        | _ ->
            let renderer = SquiggleRenderer()
            renderers.Add(editor, renderer)
            let textView = editor.TextArea.TextView
            textView.BackgroundRenderers.Add(renderer)

            editor.Document.TextChanged.Add(fun _ ->
                renderer.SetMarkers [||]
                textView.InvalidateLayer(KnownLayer.Selection)
                DiagnosticsStore.clear editor)

            renderer

    /// Show `diags` as squiggles on `editor` (and publish them for hover).
    let set (editor: TextEditor) (diags: Intellisense.Diagnostic[]) =
        let renderer = rendererFor editor
        let doc = editor.Document
        let length = doc.TextLength
        let clamp o = max 0 (min o length)

        let markers =
            diags
            |> Array.choose(fun d ->
                try
                    let startOffset = clamp(doc.GetOffset(d.StartLine, d.StartColumn + 1))
                    let rawEnd = clamp(doc.GetOffset(d.EndLine, d.EndColumn + 1))

                    let endOffset =
                        if rawEnd > startOffset then
                            rawEnd
                        else
                            clamp(startOffset + 1)

                    if endOffset > startOffset then
                        Some(struct (startOffset, endOffset, (if d.IsError then errorPen else warningPen)))
                    else
                        None
                with _ ->
                    None)

        renderer.SetMarkers markers
        editor.TextArea.TextView.InvalidateLayer(KnownLayer.Selection)
        DiagnosticsStore.set editor diags
