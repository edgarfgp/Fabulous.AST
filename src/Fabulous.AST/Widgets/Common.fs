namespace Fabulous.AST

open System.Text
open Fabulous.AST
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Microsoft.FSharp.Core.CompilerServices

[<AbstractClass; Sealed>]
type Ast = class end

type MethodParamsType =
    | UnNamed of parameters: WidgetBuilder<Type> seq * isTupled: bool
    | Named of types: (string * WidgetBuilder<Type>) seq * isTupled: bool

type AccessControl =
    | Public
    | Private
    | Internal
    | Unknown

[<AutoOpen>]
module CommonExtensions =
    type MultipleTextsNode with
        static member Create(texts: SingleTextNode seq) =
            MultipleTextsNode(List.ofSeq texts, Range.Zero)

        /// Builds the optional `with [acc?] get [, [acc?] set]` clause for
        /// property-like members. Returns `None` when neither accessor is present.
        static member CreateGetSet(getter: bool * AccessControl, setter: bool * AccessControl) =
            let accTexts =
                function
                | Public -> [ SingleTextNode.``public`` ]
                | Private -> [ SingleTextNode.``private`` ]
                | Internal -> [ SingleTextNode.``internal`` ]
                | Unknown -> []

            match getter, setter with
            | (true, gAcc), (true, sAcc) ->
                Some(
                    MultipleTextsNode.Create(
                        [ SingleTextNode.``with``
                          yield! accTexts gAcc
                          SingleTextNode.Create "get,"
                          yield! accTexts sAcc
                          SingleTextNode.set ]
                    )
                )
            | (true, gAcc), (false, _) ->
                Some(MultipleTextsNode.Create([ SingleTextNode.``with``; yield! accTexts gAcc; SingleTextNode.get ]))
            | (false, _), (true, sAcc) ->
                Some(MultipleTextsNode.Create([ SingleTextNode.``with``; yield! accTexts sAcc; SingleTextNode.set ]))
            | (false, _), (false, _) -> None

    type Type with
        static member Create(name: string) =
            Type.LongIdent(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero))

    type MultipleAttributeListNode with
        static member Create(values: AttributeNode seq) =
            MultipleAttributeListNode(
                [ AttributeListNode(
                      SingleTextNode.leftAttribute,
                      List.ofSeq values,
                      SingleTextNode.rightAttribute,
                      Range.Zero
                  ) ],
                Range.Zero
            )

[<RequireQualifiedAccess>]
module List =
    let intersperse separator (source: List<'T>) =
        let mutable coll = new ListCollector<'T>()

        let mutable notFirst = false

        source
        |> List.iter(fun element ->
            if notFirst then
                coll.Add separator

            coll.Add element
            notFirst <- true)

        coll.Close()

[<RequireQualifiedAccess>]
module Seq =
    let intersperse separator (source: 'T seq) =
        let mutable coll = new ListCollector<'T>()

        let mutable notFirst = false

        source
        |> Seq.iter(fun element ->
            if notFirst then
                coll.Add separator

            coll.Add element
            notFirst <- true)

        coll.Close()

open Fantomas.Core
open Fantomas.FCS.Diagnostics
open Fantomas.FCS.Parse

/// <summary>
/// Renders a widget tree to F# source and verifies the result. <c>mkOak</c> turns
/// the root widget into a Fantomas node (recursively building its children),
/// <c>run</c> formats that node to source (optionally with a config), and <c>parse</c> round-trips
/// the source back through the parser to confirm it is syntactically valid.
/// Designed for pipeline use, e.g. <c>widget |> Gen.parse</c> or
/// <c>widget |> Gen.mkOak |> Gen.run |> Gen.parse</c>.
/// </summary>
[<AbstractClass; Sealed>]
type Gen =
    /// Builds the Fantomas node for <paramref name="root"/>, recursively creating all children nodes.
    static member mkOak(root: WidgetBuilder<'node>) : 'node =
        let widget = root.Compile()
        let definition = WidgetDefinitionStore.get widget.Key
        definition.CreateView widget |> unbox

    /// Formats <paramref name="oak"/> to F# source using the default config.
    static member run(oak: Oak) : string =
        CodeFormatter.FormatOakAsync(oak, FormatConfig.Default)
        |> Async.RunSynchronously

    /// Formats <paramref name="oak"/> to F# source using <paramref name="config"/>.
    static member run(oak: Oak, config: FormatConfig) : string =
        CodeFormatter.FormatOakAsync(oak, config) |> Async.RunSynchronously

    /// Renders a single parser diagnostic as
    /// "(startLine,startCol)-(endLine,endCol) Severity FSxxxx: message".
    static member private formatDiagnostic(diagnostic: FSharpParserDiagnostic) : string =
        let position =
            match diagnostic.Range with
            | Some range -> $"(%d{range.StartLine},%d{range.StartColumn})-(%d{range.EndLine},%d{range.EndColumn}) "
            | None -> ""

        let number =
            match diagnostic.ErrorNumber with
            | Some n -> $" FS%04d{n}"
            | None -> ""

        $"%s{position}%A{diagnostic.Severity}%s{number}: %s{diagnostic.Message}"

    /// <summary>
    /// Parses <paramref name="source"/> with Fantomas's F# parser. Returns the
    /// source unchanged when syntactically valid, otherwise the formatted error
    /// diagnostics separated by the OS newline. Does not type-check — for that,
    /// run the real compiler against the written files.
    /// </summary>
    static member parse(source: string) : string =
        // parseFile gives the parser diagnostics directly (CodeFormatter's
        // ParseOakAsync throws ParseException on errors instead). We parse with
        // no conditional-compilation defines, so syntax inside inactive #if
        // branches is not checked — that matches the empty-defines parse that
        // ParseOakAsync surfaces first.
        try
            let _, diagnostics = parseFile false (SourceText.ofString source) []

            let errors =
                diagnostics
                |> List.filter(fun d -> d.Severity = FSharpDiagnosticSeverity.Error)
                |> List.map Gen.formatDiagnostic

            if List.isEmpty errors then
                source
            else
                String.concat System.Environment.NewLine errors
        with ex ->
            ex.Message

    /// <summary>
    /// Renders <paramref name="widget"/> to source and parses it back to confirm
    /// Fabulous.AST produced syntactically valid F#. Returns the rendered source
    /// when valid, otherwise the formatted error diagnostics (or the render
    /// failure message) separated by the OS newline.
    /// </summary>
    static member parse(widget: WidgetBuilder<Oak>) : string =
        try
            Gen.mkOak widget |> Gen.run |> Gen.parse
        with ex ->
            ex.Message

module String =
    // Taken from https://github.com/dotnet/fsharp/blob/8e773e70700eea38f472950fd042ac0065dabae0/src/FSharp.Build/WriteCodeFragment.fs#L26-L44
    let escape(str: string) =
        let sb =
            str.ToCharArray()
            |> Seq.fold
                (fun (sb: StringBuilder) (c: char) ->
                    match c with
                    | '\n'
                    | '\u2028'
                    | '\u2028' -> sb.Append("\\n")
                    | '\r' -> sb.Append("\\r")
                    | '\t' -> sb.Append("\\t")
                    | '\'' -> sb.Append("\\'")
                    | '\\' -> sb.Append("\\\\")
                    | '"' -> sb.Append("\\\"")
                    | '\u0000' -> sb.Append("\\0")
                    | _ -> sb.Append(c))
                (StringBuilder().Append("\""))

        sb.Append("\"").ToString()
