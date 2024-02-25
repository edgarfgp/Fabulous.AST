namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections

module SimplePat =
    let Name = Attributes.defineScalar<string> "Name"

    let MultipleAttributes = Attributes.defineWidgetCollection "MultipleAttributes"

    let IsOptional = Attributes.defineScalar<bool> "Parameters"

    let Typed = Attributes.defineScalar<Type> "Typed"

    let WidgetKey =
        Widgets.register "SimplePatNode" (fun widget ->
            let name = Helpers.getScalarValue widget Name
            let isOptional = Helpers.getScalarValue widget IsOptional
            let typed = Helpers.tryGetScalarValue widget Typed

            let typed =
                match typed with
                | ValueSome t -> Some t
                | ValueNone -> None

            let attributes =
                Helpers.tryGetNodesFromWidgetCollection<AttributeNode> widget MultipleAttributes


            let multipleAttributes =
                match attributes with
                | Some values ->
                    Some(
                        MultipleAttributeListNode(
                            [ AttributeListNode(
                                  SingleTextNode.leftAttribute,
                                  values,
                                  SingleTextNode.rightAttribute,
                                  Range.Zero
                              ) ],
                            Range.Zero
                        )
                    )
                | None -> None

            SimplePatNode(multipleAttributes, isOptional, SingleTextNode.Create(name), typed, Range.Zero))

[<AutoOpen>]
module SimplePatNodeBuilders =
    type Ast with

        static member SimplePat(name: string, isOptional: bool) =
            WidgetBuilder<SimplePatNode>(
                SimplePat.WidgetKey,
                AttributesBundle(
                    StackList.two(SimplePat.Name.WithValue(name), SimplePat.IsOptional.WithValue(isOptional)),
                    ValueNone,
                    ValueNone
                )
            )

        static member SimplePat(name: string, tp: Type, isOptional: bool) =
            WidgetBuilder<SimplePatNode>(
                SimplePat.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        SimplePat.Name.WithValue(name),
                        SimplePat.Typed.WithValue(tp),
                        SimplePat.IsOptional.WithValue(isOptional)
                    ),
                    ValueNone,
                    ValueNone
                )
            )

        static member SimplePat(name: string, tp: string, isOptional: bool) =
            WidgetBuilder<SimplePatNode>(
                SimplePat.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        SimplePat.Name.WithValue(name),
                        SimplePat.Typed.WithValue(CommonType.mkLongIdent tp),
                        SimplePat.IsOptional.WithValue(isOptional)
                    ),
                    ValueNone,
                    ValueNone
                )
            )

[<Extension>]
type SimplePatNodeModifiers =
    [<Extension>]
    static member inline attributes(this: WidgetBuilder<SimplePatNode>) =
        AttributeCollectionBuilder<SimplePatNode, AttributeNode>(this, SimplePat.MultipleAttributes)

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<SimplePatNode>, attributes: string list) =
        AttributeCollectionBuilder<SimplePatNode, AttributeNode>(this, SimplePat.MultipleAttributes) {
            for attribute in attributes do
                Ast.Attribute(attribute)
        }

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<SimplePatNode>, attribute: WidgetBuilder<AttributeNode>) =
        AttributeCollectionBuilder<SimplePatNode, AttributeNode>(this, SimplePat.MultipleAttributes) { attribute }

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<SimplePatNode>, attribute: string) =
        AttributeCollectionBuilder<SimplePatNode, AttributeNode>(this, SimplePat.MultipleAttributes) {
            Ast.Attribute(attribute)
        }
