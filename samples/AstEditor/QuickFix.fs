namespace AstEditor

open System
open System.Runtime.CompilerServices
open Avalonia.Input
open AvaloniaEdit
open AvaloniaEdit.CodeCompletion
open AvaloniaEdit.Document
open AvaloniaEdit.Editing

/// Quick fixes (code actions) for the DSL editor. Triggered by Ctrl+. or by clicking the
/// lightbulb in the gutter. Two kinds: *repairs* — identifier corrections from FCS's "Maybe
/// you want one of the following: …" suggestions, only present while the script has errors —
/// and *refactorings*, which restructure working DSL (apply a constant-folding Rewrite pass,
/// convert a Record to a discriminated union) and are offered regardless of errors.
module QuickFix =

    type private FixData(description: string, apply: unit -> unit) =
        interface ICompletionData with
            member _.Image = null
            member _.Text = description
            member _.Content = box description
            member _.Description = box "Quick fix"
            member _.Priority = 0.0
            member _.Complete(_: TextArea, _: ISegment, _: EventArgs) = apply()

    /// FS0039: "The value or constructor … is not defined" — the diagnostic whose message
    /// carries FCS's name suggestions.
    let private undefinedName = 39

    /// The identifiers FCS suggests in a "Maybe you want one of the following:" message.
    /// FCS only exposes the suggestions inside the (English-resource) message text, so they
    /// have to be parsed out — but *detection* gates on the error number, not the wording.
    let private suggestions(d: Intellisense.Diagnostic) =
        if d.ErrorNumber <> undefinedName then
            [||]
        else
            let marker = "Maybe you want one of the following:"
            let idx = d.Message.IndexOf(marker, StringComparison.Ordinal)

            if idx < 0 then
                [||]
            else
                d.Message.Substring(idx + marker.Length).Split('\n')
                |> Array.map(fun s -> s.Trim())
                |> Array.filter(fun s -> s.Length > 0)
                |> Array.truncate 6

    let private hasFix(d: Intellisense.Diagnostic) = suggestions d |> Array.isEmpty |> not

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
                    suggestions d
                    |> Array.map(fun s ->
                        FixData(
                            $"Replace with '{s}'",
                            (fun () -> doc.Replace(startOffset, endOffset - startOffset, s))
                        )
                        :> ICompletionData)
                else
                    [||]
            with _ ->
                [||])

    /// Document-wide refactorings (offered regardless of the caret position or of errors —
    /// restructuring valid DSL is their whole point).
    let private globalActions(editor: TextEditor) : ICompletionData[] =
        let replaceAll(text: string) =
            fun () -> editor.Document.Replace(0, editor.Document.TextLength, text)

        [| match RewriteAction.addConstantFolding editor.Text with
           | Some rewritten -> FixData("✦ Apply constant-folding Rewrite", replaceAll rewritten) :> ICompletionData
           | None -> ()

           match ConvertAction.convertRecordToUnion editor.Text with
           | Some converted -> FixData("✦ Convert Record to Union (DU)", replaceAll converted) :> ICompletionData
           | None -> () |]

    let private computeFixes (editor: TextEditor) (line: int) (col: int) =
        Array.append (fixesAt editor line col) (globalActions editor)

    let private showWindow (editor: TextEditor) (fixes: ICompletionData[]) =
        if fixes.Length > 0 then
            let w = CompletionWindow(editor.TextArea)
            w.CompletionList.CompletionData.Clear()

            for f in fixes do
                w.CompletionList.CompletionData.Add(f)

            w.Show()

    /// The 1-based lines that currently carry a lightbulb: lines with a diagnostic repair
    /// (present only while the script has errors), plus the refactoring anchors (the
    /// `|> Gen.mkOak` pipeline, the first `Record(…) {` block) — those show on valid code too.
    let actionLines(editor: TextEditor) : Set<int> =
        let diagLines =
            DiagnosticsStore.get editor
            |> Array.filter hasFix
            |> Array.map(fun d -> d.StartLine)

        let text = editor.Text

        let rewriteLines =
            if RewriteAction.canApply text then
                RewriteAction.anchorLine text |> Option.toList
            else
                []

        let convertLines = ConvertAction.anchorLine text |> Option.toList

        Set.ofArray diagLines
        |> Set.union(Set.ofList rewriteLines)
        |> Set.union(Set.ofList convertLines)

    /// Show the fix picker for a clicked gutter line (moves the caret there first).
    let popForLine (editor: TextEditor) (line: int) =
        let doc = editor.Document

        let col =
            match
                DiagnosticsStore.get editor
                |> Array.tryFind(fun d -> d.StartLine = line && hasFix d)
            with
            | Some d -> d.StartColumn
            | None -> 0

        try
            editor.CaretOffset <- doc.GetOffset(line, col + 1)
        with _ ->
            ()

        showWindow editor (computeFixes editor line col)

    let private installed = ConditionalWeakTable<TextEditor, obj>()

    /// Wire Ctrl+. quick fixes onto an editor (idempotent per instance).
    let install(editor: TextEditor) =
        match installed.TryGetValue editor with
        | true, _ -> ()
        | _ ->
            installed.Add(editor, box())

            editor.TextArea.KeyDown.Add(fun e ->
                if e.Key = Key.OemPeriod && e.KeyModifiers = KeyModifiers.Control then
                    let loc = editor.Document.GetLocation(editor.CaretOffset)
                    showWindow editor (computeFixes editor loc.Line (loc.Column - 1))
                    e.Handled <- true)
