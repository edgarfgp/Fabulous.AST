namespace AstEditor

open System
open System.Runtime.CompilerServices
open System.Text.RegularExpressions
open Avalonia.Threading
open AvaloniaEdit
open AvaloniaEdit.CodeCompletion
open AvaloniaEdit.Document
open AvaloniaEdit.Editing

/// Autocomplete for the DSL editor. Completions come from FSharp.Compiler.Service (real,
/// type-aware, including member completion after `.`); a small curated list of the
/// Fabulous.AST surface is used only as a cold-start fallback while the checker warms up.
module Completion =

    /// One completion entry. `describe` is deferred — AvaloniaEdit only reads `Description`
    /// for the highlighted item, so we don't render every tooltip up front. `glyph` is a
    /// one-letter category hint shown before the name (the inserted `Text` stays bare).
    type private CompletionData(text: string, glyph: string, describe: unit -> string) =
        interface ICompletionData with
            member _.Image = null
            member _.Text = text
            member _.Content = box $"{glyph}  {text}"
            member _.Description = box(describe())
            member _.Priority = 0.0

            member _.Complete(textArea: TextArea, segment: ISegment, _: EventArgs) =
                textArea.Document.Replace(segment, text)

    /// Curated Fabulous.AST surface, shown only if the checker hasn't produced results yet.
    let private dsl =
        [ "Oak"; "AnonymousModule"; "Namespace"; "Module"; "Record"; "Field"; "Class"; "Struct"
          "Union"; "UnionCase"; "Enum"; "Interface"; "Member"; "Property"; "Method"; "Value"
          "Function"; "Literal"; "Attribute"; "ConstantExpr"; "AppExpr"; "InfixAppExpr"; "ParenExpr"
          "TupleExpr"; "ListExpr"; "IfThenElseExpr"; "MatchExpr"; "LambdaExpr"; "Constant"; "Int"
          "String"; "Bool"; "Gen"; "mkOak"; "run"; "parse"; "Rewrite"; "expr"; "typeDefn" ]

    let private dslSet = Set.ofList dsl
    let private wordRegex = Regex(@"[A-Za-z_][A-Za-z0-9_]{2,}", RegexOptions.Compiled)

    /// Curated names + distinct identifiers in the buffer, as (name, describe) pairs.
    let private fallback (source: string) =
        let inBuffer =
            wordRegex.Matches(source)
            |> Seq.cast<Match>
            |> Seq.map(fun m -> m.Value)
            |> Seq.distinct
            |> Seq.filter(fun w -> not(dslSet.Contains w))

        Seq.append dsl inBuffer
        |> Seq.distinct
        |> Seq.sort
        |> Seq.map(fun name -> name, "v", (fun () -> "Fabulous.AST"))
        |> Seq.toArray

    /// Offset where the identifier under the caret begins (so the window filters on it).
    let private wordStart (doc: TextDocument) (caret: int) =
        let mutable start = caret

        while start > 0 && (let c = doc.GetCharAt(start - 1) in Char.IsLetterOrDigit c || c = '_') do
            start <- start - 1

        start

    let private installed = ConditionalWeakTable<TextEditor, obj>()

    /// Wire FCS-backed completion onto an editor (idempotent per instance).
    let install (editor: TextEditor) =
        match installed.TryGetValue editor with
        | true, _ -> ()
        | _ ->
            installed.Add(editor, box())
            let mutable window: CompletionWindow = null
            let mutable pending = false

            // Build and show the window from the freshest caret position (UI thread).
            let showCompletions (items: (string * string * (unit -> string))[]) =
                if items.Length > 0 && isNull window then
                    let doc = editor.Document
                    let caret = editor.CaretOffset
                    let start = wordStart doc caret
                    let prefix = doc.GetText(start, caret - start)

                    let w = CompletionWindow(editor.TextArea)
                    w.StartOffset <- start

                    for (name, glyph, describe) in items do
                        w.CompletionList.CompletionData.Add(CompletionData(name, glyph, describe))

                    if prefix.Length > 0 then
                        w.CompletionList.SelectItem(prefix)

                    w.Closed.Add(fun _ -> window <- null)
                    w.Show()
                    window <- w

            editor.TextArea.TextEntered.Add(fun e ->
                let triggers =
                    not(isNull e.Text)
                    && e.Text.Length = 1
                    && (Char.IsLetter e.Text.[0] || e.Text.[0] = '.')

                if triggers && isNull window && not pending then
                    pending <- true
                    let doc = editor.Document
                    let caret = editor.CaretOffset
                    let loc = doc.GetLocation(caret) // 1-based line/column
                    let lineText = doc.GetText(doc.GetLineByNumber(loc.Line))
                    let source = editor.Text

                    async {
                        try
                            let! items = Intellisense.complete source loc.Line loc.Column lineText

                            let mapped =
                                if items.Length > 0 then
                                    items |> Array.map(fun it -> it.Name, it.Glyph, it.Describe)
                                else
                                    fallback source

                            Dispatcher.UIThread.Post(fun () ->
                                pending <- false
                                showCompletions mapped)
                        with _ ->
                            Dispatcher.UIThread.Post(fun () -> pending <- false)
                    }
                    |> Async.Start)
