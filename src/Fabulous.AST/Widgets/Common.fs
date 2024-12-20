namespace Fabulous.AST

open Fabulous.AST
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Microsoft.FSharp.Core.CompilerServices

[<AbstractClass; Sealed>]
type Ast = class end

type MethodParamsType =
    | UnNamed of parameters: WidgetBuilder<Type> list * isTupled: bool
    | Named of types: (string * WidgetBuilder<Type>) list * isTupled: bool

type AccessControl =
    | Public
    | Private
    | Internal
    | Unknown

[<AutoOpen>]
module CommonExtensions =
    type MultipleTextsNode with
        static member Create(texts: SingleTextNode list) = MultipleTextsNode(texts, Range.Zero)

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
