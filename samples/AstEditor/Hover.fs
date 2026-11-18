namespace AstEditor

open System.Runtime.CompilerServices
open Avalonia.Controls
open Avalonia.Threading
open AvaloniaEdit

/// FCS-powered hover tooltips for the DSL editor. AvaloniaEdit raises PointerHover (with its
/// own hover delay) over the text view; we resolve the identifier under the pointer, ask FCS
/// for its tooltip, and show it through the editor's attached ToolTip.
module Hover =

    let private installed = ConditionalWeakTable<TextEditor, obj>()

    let install(editor: TextEditor) =
        match installed.TryGetValue editor with
        | true, _ -> ()
        | _ ->
            installed.Add(editor, box())
            let textView = editor.TextArea.TextView

            // Bumped on every hover event and on hover-stop, so an async FCS lookup that
            // resolves late (the pointer already moved on) can't reopen a stale tooltip.
            let mutable hoverGen = 0

            textView.PointerHover.Add(fun e ->
                hoverGen <- hoverGen + 1
                let gen = hoverGen
                let position = editor.GetPositionFromPoint(e.GetPosition(editor))

                if position.HasValue then
                    let tvp = position.Value
                    let doc = editor.Document
                    let line = tvp.Line
                    let lineText = doc.GetText(doc.GetLineByNumber(line))
                    let col = tvp.Column - 1 // 0-based index of the hovered character

                    // A diagnostic under the pointer wins — show its message immediately, no
                    // type-check needed.
                    match DiagnosticsStore.get editor |> Array.tryFind(DiagnosticsStore.covers line col) with
                    | Some d ->
                        let prefix = if d.IsError then "● error  " else "● warning  "
                        ToolTip.SetTip(editor, prefix + d.Message)
                        ToolTip.SetIsOpen(editor, true)
                    | None ->
                        let source = editor.Text

                        async {
                            try
                                match! Intellisense.tooltip source line col lineText with
                                | Some text ->
                                    Dispatcher.UIThread.Post(fun () ->
                                        if gen = hoverGen then
                                            ToolTip.SetTip(editor, text)
                                            ToolTip.SetIsOpen(editor, true))
                                | None -> ()
                            with _ ->
                                ()
                        }
                        |> Async.Start)

            textView.PointerHoverStopped.Add(fun _ ->
                hoverGen <- hoverGen + 1
                ToolTip.SetIsOpen(editor, false))
