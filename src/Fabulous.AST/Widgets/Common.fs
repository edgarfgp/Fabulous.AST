namespace Fabulous.AST

open Fabulous.Builders
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Microsoft.FSharp.Core.CompilerServices

[<AbstractClass; Sealed>]
type Ast = class end

type MethodParamsType =
    | UnNamed of parameters: WidgetBuilder<Type> list * isTupled: bool
    | Named of types: (string option * WidgetBuilder<Type>) list * isTupled: bool

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

        static member Create(texts: SingleTextNode list) = MultipleTextsNode(texts, Range.Zero)

    type XmlDocNode with

        static member Create(content: string list) =
            content
            |> List.map(fun v -> $"/// {v}")
            |> Array.ofList
            |> fun v -> XmlDocNode(v, Range.Zero)

    type Type with
        static member Create(name: string) =
            Type.LongIdent(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero))

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

/// It takes the root of the widget tree and create the corresponding Fantomas node, and recursively creating all children nodes
module Gen =
    let mkOak(root: WidgetBuilder<'node>) : 'node =
        let widget = root.Compile()
        let definition = WidgetDefinitionStore.get widget.Key
        definition.CreateView widget |> unbox
