namespace AstEditor

open System
open System.Runtime.CompilerServices
open Avalonia
open Avalonia.Media
open Avalonia.Threading
open AvaloniaEdit
open AvaloniaEdit.Document
open AvaloniaEdit.Rendering

/// Inline error/warning squiggles for the DSL editor, fed by the FCS diagnostics. A
/// background renderer draws wavy underlines under each diagnostic range; the check runs
/// debounced off the UI thread on every edit.
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

    let private installed = ConditionalWeakTable<TextEditor, obj>()

    /// Wire debounced FCS diagnostics → squiggles onto an editor (idempotent per instance).
    let install(editor: TextEditor) =
        match installed.TryGetValue editor with
        | true, _ -> ()
        | _ ->
            installed.Add(editor, box())

            let textView = editor.TextArea.TextView
            let renderer = SquiggleRenderer()
            textView.BackgroundRenderers.Add(renderer)

            let errorPen = Pen(SolidColorBrush(Colors.Red), 1.0) :> IPen
            let warningPen = Pen(SolidColorBrush(Color.Parse "#D7BA7D"), 1.0) :> IPen

            let setMarkers(diags: Intellisense.Diagnostic[]) =
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
                textView.InvalidateLayer(KnownLayer.Selection)
                // Publish for hover, so pointing at a squiggle can show its message.
                DiagnosticsStore.set editor diags

            // Bumped on every edit; a check result is only published if the text it was
            // computed from is still current, so stale ranges never reach the store.
            let mutable changeStamp = 0

            let runCheck() =
                let stamp = changeStamp
                let source = editor.Text

                async {
                    try
                        let! diags = Intellisense.diagnostics source

                        Dispatcher.UIThread.Post(fun () ->
                            if stamp = changeStamp then
                                setMarkers diags)
                    with _ ->
                        ()
                }
                |> Async.Start

            // Re-check shortly after the user stops typing.
            let timer = DispatcherTimer(Interval = TimeSpan.FromMilliseconds 600.)

            timer.Tick.Add(fun _ ->
                timer.Stop()
                runCheck())

            editor.Document.TextChanged.Add(fun _ ->
                changeStamp <- changeStamp + 1
                // The stored diagnostics describe the *previous* text; acting on them (quick
                // fixes, hover) could rewrite the wrong span, so drop them until the re-check.
                DiagnosticsStore.clear editor
                timer.Stop()
                timer.Start())

            runCheck() // initial pass so the default sample is checked
