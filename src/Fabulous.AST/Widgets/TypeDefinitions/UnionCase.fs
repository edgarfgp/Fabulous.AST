namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList


module UnionCase =

    let Name = Attributes.defineScalar<string> "Name"

    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"

    let Fields = Attributes.defineWidgetCollection "Fields"

    let WidgetKey =
        Widgets.register "UnionCase" (fun widget ->
            let name = Helpers.getScalarValue widget Name
            let attributes = Helpers.tryGetScalarValue widget MultipleAttributes
            let fields = Helpers.tryGetNodesFromWidgetCollection<FieldNode> widget Fields

            let fields =
                match fields with
                | Some fields -> fields
                | None -> []

            let multipleAttributes =
                match attributes with
                | ValueSome values -> MultipleAttributeListNode.Create values |> Some
                | ValueNone -> None

            UnionCaseNode(None, multipleAttributes, None, SingleTextNode.Create(name), fields, Range.Zero))

[<AutoOpen>]
module UnionCaseBuilders =
    type Ast with
        static member UnionCase(name: string) =
            WidgetBuilder<UnionCaseNode>(
                UnionCase.WidgetKey,
                AttributesBundle(StackList.one(UnionCase.Name.WithValue(name)), ValueNone, ValueNone)
            )

        static member UnionParamsCase(name: string) =
            CollectionBuilder<UnionCaseNode, FieldNode>(
                UnionCase.WidgetKey,
                UnionCase.Fields,
                AttributesBundle(StackList.one(UnionCase.Name.WithValue(name)), ValueNone, ValueNone)
            )


[<Extension>]
type UnionCaseModifiers =
    [<Extension>]
    static member inline attributes(this: WidgetBuilder<UnionCaseNode>, attributes: string list) =
        this.AddScalar(UnionCase.MultipleAttributes.WithValue(attributes))
