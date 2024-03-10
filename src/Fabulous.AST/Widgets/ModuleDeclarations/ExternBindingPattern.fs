namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module ExternBindingPattern =
    let Pattern = Attributes.defineWidget "DoExpression"
    let MultipleAttributes = Attributes.defineWidgetCollection "MultipleAttributes"
    let Type = Attributes.defineWidget "Type"

    let WidgetKey =
        Widgets.register "ExternBindingPattern" (fun widget ->
            let pat = Helpers.tryGetNodeFromWidget<Pattern> widget Pattern

            let attributes =
                Helpers.tryGetNodesFromWidgetCollection<AttributeNode> widget MultipleAttributes

            let ``type`` = Helpers.tryGetNodeFromWidget widget Type

            let ``type`` =
                match ``type`` with
                | ValueSome value -> Some(value)
                | ValueNone -> None

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

            let pat =
                match pat with
                | ValueSome value -> Some(value)
                | ValueNone -> None

            ExternBindingPatternNode(multipleAttributes, ``type``, pat, Range.Zero))

[<AutoOpen>]
module ExternBindingPatternNodeBuilders =
    type Ast with

        static member ExternBindingPat(``type``: string, pat: WidgetBuilder<Pattern>) =
            WidgetBuilder<ExternBindingPatternNode>(
                ExternBindingPattern.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome
                        [| ExternBindingPattern.Pattern.WithValue(pat.Compile())
                           ExternBindingPattern.Type.WithValue(Ast.LongIdent(``type``).Compile()) |],
                    ValueNone
                )
            )

        static member ExternBindingPat(``type``: WidgetBuilder<Type>, pat: WidgetBuilder<Pattern>) =
            WidgetBuilder<ExternBindingPatternNode>(
                ExternBindingPattern.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome
                        [| ExternBindingPattern.Pattern.WithValue(pat.Compile())
                           ExternBindingPattern.Type.WithValue(``type``.Compile()) |],
                    ValueNone
                )
            )

[<Extension>]
type ExternBindingPatternNodeModifiers =
    [<Extension>]
    static member inline attributes(this: WidgetBuilder<ExternBindingPatternNode>) =
        AttributeCollectionBuilder<ExternBindingPatternNode, AttributeNode>(
            this,
            ExternBindingPattern.MultipleAttributes
        )

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<ExternBindingPatternNode>, attributes: string list) =
        AttributeCollectionBuilder<ExternBindingPatternNode, AttributeNode>(
            this,
            ExternBindingPattern.MultipleAttributes
        ) {
            for attribute in attributes do
                Ast.Attribute(attribute)
        }

    [<Extension>]
    static member inline attribute
        (this: WidgetBuilder<ExternBindingPatternNode>, attribute: WidgetBuilder<AttributeNode>)
        =
        AttributeCollectionBuilder<ExternBindingPatternNode, AttributeNode>(
            this,
            ExternBindingPattern.MultipleAttributes
        ) {
            attribute
        }

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<ExternBindingPatternNode>, attribute: string) =
        AttributeCollectionBuilder<ExternBindingPatternNode, AttributeNode>(
            this,
            ExternBindingPattern.MultipleAttributes
        ) {
            Ast.Attribute(attribute)
        }
