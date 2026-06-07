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
    /// tooltip text for the item the user actually highlights, not all of them.
    type CompletionItem =
        { Name: string
          Describe: unit -> string }

    type Diagnostic =
        { StartLine: int
          StartColumn: int
          EndLine: int
          EndColumn: int
          IsError: bool
          Message: string }

    let private checker = lazy FSharpChecker.Create()

    // A stable script path; the content is supplied per-request.
    let private scriptPath = Path.Combine(Path.GetTempPath(), "fabulous_ast_playground.fsx")

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

    let private getOptions (source: ISourceText) =
        async {
            match cachedOptions with
            | Some options -> return options
            | None ->
                let! options, _ =
                    checker.Value.GetProjectOptionsFromScript(scriptPath, source, assumeDotNetFramework = false)

                let options =
                    { options with
                        OtherOptions = Array.append options.OtherOptions referenceArgs }

                cachedOptions <- Some options
                return options
        }

    let private check (source: string) =
        async {
            let text = SourceText.ofString source
            let! options = getOptions text
            let! parseResults, answer = checker.Value.ParseAndCheckFileInProject(scriptPath, 0, text, options)

            match answer with
            | FSharpCheckFileAnswer.Succeeded checkResults -> return Some(parseResults, checkResults)
            | FSharpCheckFileAnswer.Aborted -> return None
        }

    /// Flatten a ToolTipText to a short single description (first group's main text).
    let private renderTip (tip: ToolTipText) =
        let (ToolTipText elements) = tip

        elements
        |> List.choose(fun element ->
            match element with
            | ToolTipElement.Group(data :: _) -> data.MainDescription |> Array.map(fun t -> t.Text) |> String.concat "" |> Some
            | ToolTipElement.CompositionError err -> Some err
            | _ -> None)
        |> String.concat "\n"

    /// Completions at a caret. `line`/`col` are 1-based (AvaloniaEdit's convention).
    let complete (source: string) (line: int) (col: int) (lineText: string) =
        async {
            match! check source with
            | None -> return [||]
            | Some(parseResults, checkResults) ->
                let partialName = QuickParse.GetPartialLongNameEx(lineText, col - 1)
                let info = checkResults.GetDeclarationListInfo(Some parseResults, line, lineText, partialName)

                return
                    info.Items
                    |> Array.map(fun item ->
                        { Name = item.NameInList
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
                    let tip = checkResults.GetToolTip(line, colAtEndOfNames, lineText, names, FSharpTokenTag.Identifier)
                    let text = renderTip tip
                    return (if String.IsNullOrWhiteSpace text then None else Some text)
                | None -> return None
        }

    /// Type-check the whole script and return its diagnostics.
    let diagnostics (source: string) =
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
                          Message = d.Message })
        }
