namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module ExternBindingPattern =
    let PatternVal = Attributes.defineScalar<StringOrWidget<Pattern>> "DoExpression"

    let MultipleAttributes = Attributes.defineWidgetCollection "MultipleAttributes"
    let TypeValue = Attributes.defineScalar<StringOrWidget<Type>> "Type"

    let WidgetKey =
        Widgets.register "ExternBindingPattern" (fun widget ->
            let pat = Widgets.tryGetScalarValue widget PatternVal

            let attributes =
                Widgets.tryGetNodesFromWidgetCollection<AttributeNode> widget MultipleAttributes

            let tp = Widgets.tryGetScalarValue widget TypeValue

            let tp =
                match tp with
                | ValueSome tp ->
                    match tp with
                    | StringOrWidget.StringExpr value ->
                        let value = StringParsing.normalizeIdentifierBackticks value

                        Some(
                            Type.LongIdent(
                                IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(value)) ], Range.Zero)
                            )
                        )
                    | StringOrWidget.WidgetExpr widget -> Some widget
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
                | ValueSome value ->
                    match value with
                    | StringOrWidget.StringExpr value ->
                        Some(Pattern.Named(PatNamedNode(None, SingleTextNode.Create(value.Normalize()), Range.Zero)))
                    | StringOrWidget.WidgetExpr pattern -> Some pattern
                | ValueNone -> None

            ExternBindingPatternNode(multipleAttributes, tp, pat, Range.Zero))

[<AutoOpen>]
module ExternBindingPatternNodeBuilders =
    type Ast with

        static member ExternBindingPat(value: string, pat: WidgetBuilder<Pattern>) =
            WidgetBuilder<ExternBindingPatternNode>(
                ExternBindingPattern.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        ExternBindingPattern.PatternVal.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak pat)),
                        ExternBindingPattern.TypeValue.WithValue(StringOrWidget.StringExpr(Unquoted value))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member ExternBindingPat(value: WidgetBuilder<Type>, pat: WidgetBuilder<Pattern>) =
            WidgetBuilder<ExternBindingPatternNode>(
                ExternBindingPattern.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        ExternBindingPattern.PatternVal.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak pat)),
                        ExternBindingPattern.TypeValue.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak value))
                    ),
                    Array.empty,
                    Array.empty
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
