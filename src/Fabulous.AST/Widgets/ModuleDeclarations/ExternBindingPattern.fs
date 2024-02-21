namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module ExternBindingPattern =
    let Pattern = Attributes.defineWidget "DoExpression"
    let MultipleAttributes = Attributes.defineWidget "MultipleAttributes"
    let Type = Attributes.defineScalar<Type> "Type"

    let WidgetKey =
        Widgets.register "ExternBindingPattern" (fun widget ->
            let pat = Helpers.tryGetNodeFromWidget<Pattern> widget Pattern

            let attribute =
                Helpers.tryGetNodeFromWidget<AttributeListNode> widget MultipleAttributes

            let ``type`` = Helpers.tryGetScalarValue widget Type

            let ``type`` =
                match ``type`` with
                | ValueSome value -> Some(value)
                | ValueNone -> None

            let multipleAttributes =
                match attribute with
                | ValueSome values -> Some(MultipleAttributeListNode([ values ], Range.Zero))
                | ValueNone -> None

            let pat =
                match pat with
                | ValueSome value -> Some(value)
                | ValueNone -> None

            ExternBindingPatternNode(multipleAttributes, ``type``, pat, Range.Zero))

[<AutoOpen>]
module ExternBindingPatternNodeBuilders =
    type Ast with

        static member ExternBindingPattern(``type``: string, pat: WidgetBuilder<Pattern>) =
            WidgetBuilder<ExternBindingPatternNode>(
                ExternBindingPattern.WidgetKey,
                AttributesBundle(
                    StackList.one(ExternBindingPattern.Type.WithValue(CommonType.mkLongIdent(``type``))),
                    ValueSome [| ExternBindingPattern.Pattern.WithValue(pat.Compile()) |],
                    ValueNone
                )
            )

        static member ExternBindingPattern(``type``: Type, pat: WidgetBuilder<Pattern>) =
            WidgetBuilder<ExternBindingPatternNode>(
                ExternBindingPattern.WidgetKey,
                AttributesBundle(
                    StackList.one(ExternBindingPattern.Type.WithValue(``type``)),
                    ValueSome [| ExternBindingPattern.Pattern.WithValue(pat.Compile()) |],
                    ValueNone
                )
            )

[<Extension>]
type ExternBindingPatternNodeModifiers =
    [<Extension>]
    static member inline attributes
        (
            this: WidgetBuilder<ExternBindingPatternNode>,
            attributes: WidgetBuilder<AttributeListNode>
        ) =
        this.AddWidget(ExternBindingPattern.MultipleAttributes.WithValue(attributes.Compile()))

    [<Extension>]
    static member inline attributes
        (
            this: WidgetBuilder<ExternBindingPatternNode>,
            attribute: WidgetBuilder<AttributeNode>
        ) =
        this.AddWidget(
            ExternBindingPattern.MultipleAttributes.WithValue((Ast.AttributeNodes() { attribute }).Compile())
        )
