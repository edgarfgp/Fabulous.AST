namespace AstEditor

open System
open System.Runtime.CompilerServices
open Avalonia.Input
open AvaloniaEdit
open AvaloniaEdit.CodeCompletion
open AvaloniaEdit.Document
open AvaloniaEdit.Editing

/// Quick fixes (code actions) for the DSL editor. Triggered by Ctrl+. or by clicking the
/// lightbulb in the gutter. Diagnostic fixes come from FCS's "Maybe you want one of the
/// following: …" suggestions; a document-wide Rewrite action is offered alongside them.
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

    let private hasFix (d: Intellisense.Diagnostic) = suggestions d.Message |> Array.isEmpty |> not

    /// Replacement fixes for every diagnostic covering the (1-based line, 0-based col).
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

    /// Document-wide actions (offered regardless of the caret position).
    let private globalActions (editor: TextEditor) : ICompletionData[] =
        match RewriteAction.addConstantFolding editor.Text with
        | Some rewritten ->
            [| FixData(
                   "✦ Apply constant-folding Rewrite",
                   (fun () -> editor.Document.Replace(0, editor.Document.TextLength, rewritten))
               )
               :> ICompletionData |]
        | None -> [||]

    let private computeFixes (editor: TextEditor) (line: int) (col: int) =
        Array.append (fixesAt editor line col) (globalActions editor)

    let private showWindow (editor: TextEditor) (fixes: ICompletionData[]) =
        if fixes.Length > 0 then
            let w = CompletionWindow(editor.TextArea)
            w.CompletionList.CompletionData.Clear()

            for f in fixes do
                w.CompletionList.CompletionData.Add(f)

            w.Show()

    /// The 1-based lines that currently carry a lightbulb: lines with a diagnostic fix, plus
    /// the `|> Gen.mkOak` line when the Rewrite action applies.
    let actionLines (editor: TextEditor) : Set<int> =
        let diagLines =
            DiagnosticsStore.get editor |> Array.filter hasFix |> Array.map(fun d -> d.StartLine)

        let rewriteLines =
            if RewriteAction.canApply editor.Text then
                let idx = editor.Text.IndexOf("|> Gen.mkOak", StringComparison.Ordinal)

                if idx >= 0 then
                    [ editor.Document.GetLineByOffset(idx).LineNumber ]
                else
                    []
            else
                []

        Set.union (Set.ofArray diagLines) (Set.ofList rewriteLines)

    /// Show the fix picker for a clicked gutter line (moves the caret there first).
    let popForLine (editor: TextEditor) (line: int) =
        let doc = editor.Document

        let col =
            match DiagnosticsStore.get editor |> Array.tryFind(fun d -> d.StartLine = line && hasFix d) with
            | Some d -> d.StartColumn
            | None -> 0

        try
            editor.CaretOffset <- doc.GetOffset(line, col + 1)
        with _ ->
            ()

        showWindow editor (computeFixes editor line col)

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
                    showWindow editor (computeFixes editor loc.Line (loc.Column - 1))
                    e.Handled <- true)
