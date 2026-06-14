namespace AstEditor

open System
open System.IO
open FSharp.Compiler.CodeAnalysis
open FSharp.Compiler.EditorServices
open FSharp.Compiler.Diagnostics
open FSharp.Compiler.Text
open FSharp.Compiler.Tokenization

/// Type-aware language service for the DSL editor, backed by FSharp.Compiler.Service.
/// It type-checks the script (with the Fabulous.AST/Fantomas references the evaluator uses)
/// and answers completion and diagnostics requests. Checks are async and cached by FCS.
module Intellisense =

    /// A completion candidate. `Describe` is deferred so we only render the (expensive)
    /// tooltip text for the item the user actually highlights, not all of them. `Glyph` is a
    /// one-letter category hint (C=class, M=method, F=field, …).
    type CompletionItem =
        { Name: string
          Glyph: string
          Describe: unit -> string }

    type Diagnostic =
        {
            StartLine: int
            StartColumn: int
            EndLine: int
            EndColumn: int
            IsError: bool
            /// FCS error code (e.g. 39 = "The value or constructor … is not defined").
            ErrorNumber: int
            Message: string
        }

    // suggestNamesForErrors: include "Maybe you want one of the following: …" hints in
    // diagnostic messages — the quick-fix code action parses those for replacements.
    let private checker = lazy FSharpChecker.Create(suggestNamesForErrors = true)

    // A stable script path; the content is supplied per-request.
    let private scriptPath =
        Path.Combine(Path.GetTempPath(), "fabulous_ast_playground.fsx")

    /// `-r:` flags for the same assemblies the evaluator loads, so completion resolves the DSL.
    let private referenceArgs =
        let baseDir = AppContext.BaseDirectory

        [ "Fabulous.AST"; "Fantomas.Core"; "Fantomas.FCS"; "FSharp.Core" ]
        |> List.map(fun n -> Path.Combine(baseDir, n + ".dll"))
        |> List.filter File.Exists
        |> List.map(fun p -> "-r:" + p)
        |> Array.ofList

    // The references are fixed and the DSL scripts carry no #load/#r, so the project options
    // don't depend on the edited source — resolve them once and reuse (resolution is the slow
    // part). A duplicate compute under a race is harmless (idempotent).
    let mutable private cachedOptions: FSharpProjectOptions option = None

    let private getOptions(source: ISourceText) =
        async {
            match cachedOptions with
            | Some options -> return options
            | None ->
                let! options, _ =
                    checker.Value.GetProjectOptionsFromScript(scriptPath, source, assumeDotNetFramework = false)

                let options =
                    { options with OtherOptions = Array.append options.OtherOptions referenceArgs }

                cachedOptions <- Some options
                return options
        }

    let private check(source: string) =
        async {
            let text = SourceText.ofString source
            let! options = getOptions text
            let! parseResults, answer = checker.Value.ParseAndCheckFileInProject(scriptPath, 0, text, options)

            match answer with
            | FSharpCheckFileAnswer.Succeeded checkResults -> return Some(parseResults, checkResults)
            | FSharpCheckFileAnswer.Aborted -> return None
        }

    /// Flatten a ToolTipText to a short single description (first group's main text).
    let private renderTip(tip: ToolTipText) =
        let (ToolTipText elements) = tip

        elements
        |> List.choose(fun element ->
            match element with
            | ToolTipElement.Group(data :: _) ->
                data.MainDescription |> Array.map(fun t -> t.Text) |> String.concat "" |> Some
            | ToolTipElement.CompositionError err -> Some err
            | _ -> None)
        |> String.concat "\n"

    /// A one-letter category hint for a completion glyph.
    let private glyphSymbol(glyph: FSharpGlyph) =
        match glyph with
        | FSharpGlyph.Class
        | FSharpGlyph.Struct -> "C"
        | FSharpGlyph.Method
        | FSharpGlyph.OverridenMethod
        | FSharpGlyph.ExtensionMethod -> "M"
        | FSharpGlyph.Field -> "F"
        | FSharpGlyph.Property -> "P"
        | FSharpGlyph.Module
        | FSharpGlyph.NameSpace -> "N"
        | FSharpGlyph.Type
        | FSharpGlyph.Typedef -> "T"
        | FSharpGlyph.Union
        | FSharpGlyph.Enum -> "U"
        | FSharpGlyph.EnumMember -> "e"
        | FSharpGlyph.Interface -> "I"
        | FSharpGlyph.Delegate -> "D"
        | FSharpGlyph.Event -> "E"
        | FSharpGlyph.Variable -> "x"
        | _ -> "v"

    /// Completions at a caret. `line`/`col` are 1-based (AvaloniaEdit's convention).
    let complete (source: string) (line: int) (col: int) (lineText: string) =
        async {
            match! check source with
            | None -> return [||]
            | Some(parseResults, checkResults) ->
                // GetPartialLongNameEx wants the 0-based index of the last character *before*
                // the caret (caret index - 1), so the 1-based caret column maps to col - 2.
                let partialName = QuickParse.GetPartialLongNameEx(lineText, col - 2)

                let info =
                    checkResults.GetDeclarationListInfo(Some parseResults, line, lineText, partialName)

                return
                    info.Items
                    |> Array.map(fun item ->
                        { Name = item.NameInList
                          Glyph = glyphSymbol item.Glyph
                          Describe = fun () -> renderTip item.Description })
        }

    /// Hover tooltip for the identifier at a caret. `line`/`col` are 1-based line / 0-based
    /// column (the index of the hovered character within `lineText`).
    let tooltip (source: string) (line: int) (col: int) (lineText: string) =
        async {
            match! check source with
            | None -> return None
            | Some(_, checkResults) ->
                match QuickParse.GetCompleteIdentifierIsland false lineText col with
                | Some(island, colAtEndOfNames, _) ->
                    let names = island.Split('.') |> List.ofArray

                    let tip =
                        checkResults.GetToolTip(line, colAtEndOfNames, lineText, names, FSharpTokenTag.Identifier)

                    let text = renderTip tip
                    return (if String.IsNullOrWhiteSpace text then None else Some text)
                | None -> return None
        }

    /// One overload's rendered signature (e.g. `Field(name, fieldType)`).
    type SignatureOverload =
        { Header: string
          Parameters: string[] }

    /// Overloads for the method whose `names` ends at `line`/`col` (e.g. when typing `(`).
    let signatures (source: string) (line: int) (col: int) (lineText: string) (names: string list) =
        async {
            match! check source with
            | None -> return [||]
            | Some(_, checkResults) ->
                let group = checkResults.GetMethods(line, col, lineText, Some names)

                return
                    group.Methods
                    |> Array.map(fun m ->
                        let ps =
                            m.Parameters
                            |> Array.map(fun p -> p.Display |> Array.map(fun t -> t.Text) |> String.concat "")

                        { Header = group.MethodName + "(" + String.concat ", " ps + ")"
                          Parameters = ps })
        }

    /// Type-check the whole script and return its diagnostics.
    let diagnostics(source: string) =
        async {
            match! check source with
            | None -> return [||]
            | Some(_, checkResults) ->
                return
                    checkResults.Diagnostics
                    |> Array.map(fun d ->
                        { StartLine = d.StartLine
                          StartColumn = d.StartColumn
                          EndLine = d.EndLine
                          EndColumn = d.EndColumn
                          IsError = (d.Severity = FSharpDiagnosticSeverity.Error)
                          ErrorNumber = d.ErrorNumber
                          Message = d.Message })
        }
