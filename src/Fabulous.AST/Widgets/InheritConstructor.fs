namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module InheritConstructor =
    let TypeValue = Attributes.defineScalar<StringOrWidget<Type>> "TypeValue"

    let ExprValue = Attributes.defineScalar<StringOrWidget<Expr>> "ExprValue"

    let WidgetTypedOnlyKey =
        Widgets.register "TypedOnly" (fun widget ->
            let typeValue = Widgets.getScalarValue widget TypeValue

            let typeValue =
                match typeValue with
                | StringOrWidget.StringExpr value ->
                    let value = StringParsing.normalizeIdentifierBackticks value
                    Type.LongIdent(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(value)) ], Range.Zero))
                | StringOrWidget.WidgetExpr widget -> widget

            InheritConstructor.TypeOnly(
                InheritConstructorTypeOnlyNode(SingleTextNode.``inherit``, typeValue, Range.Zero)
            ))

    let WidgetUnitKey =
        Widgets.register "Unit" (fun widget ->
            let typeValue = Widgets.getScalarValue widget TypeValue

            let typeValue =
                match typeValue with
                | StringOrWidget.StringExpr value ->
                    let value = StringParsing.normalizeIdentifierBackticks value
                    Type.LongIdent(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(value)) ], Range.Zero))
                | StringOrWidget.WidgetExpr widget -> widget

            InheritConstructor.Unit(
                InheritConstructorUnitNode(
                    SingleTextNode.``inherit``,
                    typeValue,
                    SingleTextNode.leftParenthesis,
                    SingleTextNode.rightParenthesis,
                    Range.Zero
                )
            ))

    let WidgetParentKey =
        Widgets.register "Paren" (fun widget ->
            let typeValue = Widgets.getScalarValue widget TypeValue
            let expr = Widgets.getScalarValue widget ExprValue

            let expr =
                match expr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value)))
                    )
                | StringOrWidget.WidgetExpr value -> value

            let typeValue =
                match typeValue with
                | StringOrWidget.StringExpr value ->
                    let value = StringParsing.normalizeIdentifierBackticks value
                    Type.LongIdent(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(value)) ], Range.Zero))
                | StringOrWidget.WidgetExpr widget -> widget

            InheritConstructor.Paren(
                InheritConstructorParenNode(SingleTextNode.``inherit``, typeValue, expr, Range.Zero)
            ))

    let WidgetOtherKey =
        Widgets.register "Other" (fun widget ->
            let typeValue = Widgets.getScalarValue widget TypeValue
            let expr = Widgets.getScalarValue widget ExprValue

            let expr =
                match expr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value)))
                    )
                | StringOrWidget.WidgetExpr value -> value

            let typeValue =
                match typeValue with
                | StringOrWidget.StringExpr value ->
                    let value = StringParsing.normalizeIdentifierBackticks value
                    Type.LongIdent(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(value)) ], Range.Zero))
                | StringOrWidget.WidgetExpr widget -> widget

            InheritConstructor.Other(
                InheritConstructorOtherNode(SingleTextNode.``inherit``, typeValue, expr, Range.Zero)
            ))

[<AutoOpen>]
module InheritConstructorBuilders =
    type Ast with
        static member InheritConstructorTypeOnly(value: StringVariant) =
            WidgetBuilder<InheritConstructor>(
                InheritConstructor.WidgetTypedOnlyKey,
                AttributesBundle(
                    StackList.one(InheritConstructor.TypeValue.WithValue(StringOrWidget.StringExpr value)),
                    Array.empty,
                    Array.empty
                )
            )

        static member InheritConstructorTypeOnly(value: WidgetBuilder<Type>) =
            WidgetBuilder<InheritConstructor>(
                InheritConstructor.WidgetTypedOnlyKey,
                AttributesBundle(
                    StackList.one(InheritConstructor.TypeValue.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak value))),
                    Array.empty,
                    Array.empty
                )
            )

        static member InheritConstructorUnit(value: StringVariant) =
            WidgetBuilder<InheritConstructor>(
                InheritConstructor.WidgetUnitKey,
                AttributesBundle(
                    StackList.one(InheritConstructor.TypeValue.WithValue(StringOrWidget.StringExpr value)),
                    Array.empty,
                    Array.empty
                )
            )

        static member InheritConstructorUnit(value: WidgetBuilder<Type>) =
            WidgetBuilder<InheritConstructor>(
                InheritConstructor.WidgetUnitKey,
                AttributesBundle(
                    StackList.one(InheritConstructor.TypeValue.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak value))),
                    Array.empty,
                    Array.empty
                )
            )

        static member InheritConstructorParen(ty: StringVariant, exp: StringVariant) =
            WidgetBuilder<InheritConstructor>(
                InheritConstructor.WidgetParentKey,
                AttributesBundle(
                    StackList.two(
                        InheritConstructor.TypeValue.WithValue(StringOrWidget.StringExpr ty),
                        InheritConstructor.ExprValue.WithValue(StringOrWidget.StringExpr exp)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member InheritConstructorParen(value: WidgetBuilder<Type>, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<InheritConstructor>(
                InheritConstructor.WidgetParentKey,
                AttributesBundle(
                    StackList.two(
                        InheritConstructor.TypeValue.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak value)),
                        InheritConstructor.ExprValue.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak expr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member InheritConstructorParen(value: WidgetBuilder<Type>, expr: StringVariant) =
            WidgetBuilder<InheritConstructor>(
                InheritConstructor.WidgetParentKey,
                AttributesBundle(
                    StackList.two(
                        InheritConstructor.TypeValue.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak value)),
                        InheritConstructor.ExprValue.WithValue(StringOrWidget.StringExpr expr)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member InheritConstructorParen(value: StringVariant, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<InheritConstructor>(
                InheritConstructor.WidgetParentKey,
                AttributesBundle(
                    StackList.two(
                        InheritConstructor.TypeValue.WithValue(StringOrWidget.StringExpr value),
                        InheritConstructor.ExprValue.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak expr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member InheritConstructorOther(value: StringVariant, expr: StringVariant) =
            WidgetBuilder<InheritConstructor>(
                InheritConstructor.WidgetOtherKey,
                AttributesBundle(
                    StackList.two(
                        InheritConstructor.TypeValue.WithValue(StringOrWidget.StringExpr value),
                        InheritConstructor.ExprValue.WithValue(StringOrWidget.StringExpr expr)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member InheritConstructorOther(value: WidgetBuilder<Type>, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<InheritConstructor>(
                InheritConstructor.WidgetOtherKey,
                AttributesBundle(
                    StackList.two(
                        InheritConstructor.TypeValue.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak value)),
                        InheritConstructor.ExprValue.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak expr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member InheritConstructorOther(value: WidgetBuilder<Type>, expr: StringVariant) =
            WidgetBuilder<InheritConstructor>(
                InheritConstructor.WidgetOtherKey,
                AttributesBundle(
                    StackList.two(
                        InheritConstructor.TypeValue.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak value)),
                        InheritConstructor.ExprValue.WithValue(StringOrWidget.StringExpr expr)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member InheritConstructorOther(value: StringVariant, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<InheritConstructor>(
                InheritConstructor.WidgetOtherKey,
                AttributesBundle(
                    StackList.two(
                        InheritConstructor.TypeValue.WithValue(StringOrWidget.StringExpr value),
                        InheritConstructor.ExprValue.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak expr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )
