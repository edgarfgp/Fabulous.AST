namespace AstEditor

open System
open System.Runtime.CompilerServices
open Avalonia.Input
open AvaloniaEdit
open AvaloniaEdit.CodeCompletion
open AvaloniaEdit.Document
open AvaloniaEdit.Editing

/// Quick fixes (code actions) for the DSL editor. Press Ctrl+. on a diagnostic and pick a
/// replacement — driven by FCS's "Maybe you want one of the following: …" suggestions, so a
/// typo like `Fielddd` can be rewritten to `Field` in one keystroke. The picker reuses
/// AvaloniaEdit's CompletionWindow; each entry rewrites the document instead of inserting text.
module QuickFix =

    type private FixData(description: string, apply: unit -> unit) =
        interface ICompletionData with
            member _.Image = null
            member _.Text = description
            member _.Content = box description
            member _.Description = box "Quick fix"
            member _.Priority = 0.0
            member _.Complete(_: TextArea, _: ISegment, _: EventArgs) = apply()

    /// The identifiers FCS suggests in a "Maybe you want one of the following:" message.
    let private suggestions (message: string) =
        let marker = "Maybe you want one of the following:"
        let idx = message.IndexOf(marker, StringComparison.Ordinal)

        if idx < 0 then
            [||]
        else
            message.Substring(idx + marker.Length).Split('\n')
            |> Array.map(fun s -> s.Trim())
            |> Array.filter(fun s -> s.Length > 0)
            |> Array.truncate 6

    /// Replacement fixes for every diagnostic covering the caret.
    let private fixesAt (editor: TextEditor) (line: int) (col: int) : ICompletionData[] =
        let doc = editor.Document

        DiagnosticsStore.get editor
        |> Array.filter(DiagnosticsStore.covers line col)
        |> Array.collect(fun d ->
            try
                let startOffset = doc.GetOffset(d.StartLine, d.StartColumn + 1)
                let endOffset = doc.GetOffset(d.EndLine, d.EndColumn + 1)

                if endOffset > startOffset then
                    suggestions d.Message
                    |> Array.map(fun s ->
                        FixData($"Replace with '{s}'", (fun () -> doc.Replace(startOffset, endOffset - startOffset, s)))
                        :> ICompletionData)
                else
                    [||]
            with _ ->
                [||])

    let private installed = ConditionalWeakTable<TextEditor, obj>()

    /// Wire Ctrl+. quick fixes onto an editor (idempotent per instance).
    let install (editor: TextEditor) =
        match installed.TryGetValue editor with
        | true, _ -> ()
        | _ ->
            installed.Add(editor, box())

            editor.TextArea.KeyDown.Add(fun e ->
                if e.Key = Key.OemPeriod && e.KeyModifiers = KeyModifiers.Control then
                    let loc = editor.Document.GetLocation(editor.CaretOffset)
                    let fixes = fixesAt editor loc.Line (loc.Column - 1)

                    if fixes.Length > 0 then
                        let w = CompletionWindow(editor.TextArea)
                        w.CompletionList.CompletionData.Clear()

                        for f in fixes do
                            w.CompletionList.CompletionData.Add(f)

                        w.Show()
                        e.Handled <- true)
