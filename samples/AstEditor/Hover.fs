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

    let install (editor: TextEditor) =
        match installed.TryGetValue editor with
        | true, _ -> ()
        | _ ->
            installed.Add(editor, box())
            let textView = editor.TextArea.TextView

            textView.PointerHover.Add(fun e ->
                let position = editor.GetPositionFromPoint(e.GetPosition(editor))

                if position.HasValue then
                    let tvp = position.Value
                    let doc = editor.Document
                    let line = tvp.Line
                    let lineText = doc.GetText(doc.GetLineByNumber(line))
                    let col = tvp.Column - 1 // 0-based index of the hovered character
                    let source = editor.Text

                    async {
                        try
                            match! Intellisense.tooltip source line col lineText with
                            | Some text ->
                                Dispatcher.UIThread.Post(fun () ->
                                    ToolTip.SetTip(editor, text)
                                    ToolTip.SetIsOpen(editor, true))
                            | None -> ()
                        with _ ->
                            ()
                    }
                    |> Async.Start)

            textView.PointerHoverStopped.Add(fun _ -> ToolTip.SetIsOpen(editor, false))
