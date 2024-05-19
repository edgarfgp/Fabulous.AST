namespace Fabulous.AST

open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Microsoft.FSharp.Core.CompilerServices

type AccessControl =
    | Public
    | Private
    | Internal
    | Unknown

[<AutoOpen>]
module CommonExtensions =
    type MultipleTextsNode with

        static member Create(texts: string list) =
            MultipleTextsNode(
                [ for v in texts do
                      SingleTextNode.Create(v) ],
                Range.Zero
            )

    type XmlDocNode with

        static member Create(content: string list) =
            content
            |> List.map(fun v -> $"/// {v}")
            |> Array.ofList
            |> fun v -> XmlDocNode(v, Range.Zero)

    type MultipleAttributeListNode with
        static member Create(values: AttributeNode list) =
            MultipleAttributeListNode(
                [ AttributeListNode(SingleTextNode.leftAttribute, values, SingleTextNode.rightAttribute, Range.Zero) ],
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
