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

/// It takes the root of the widget tree and create the corresponding Fantomas node, and recursively creating all children nodes
module Gen =
    let mkOak(root: WidgetBuilder<'node>) : 'node =
        let widget = root.Compile()
        let definition = WidgetDefinitionStore.get widget.Key
        definition.CreateView widget |> unbox

    let run oak =
        CodeFormatter.FormatOakAsync(oak, FormatConfig.Default)
        |> Async.RunSynchronously

    let runWith config oak =
        CodeFormatter.FormatOakAsync(oak, config) |> Async.RunSynchronously

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
