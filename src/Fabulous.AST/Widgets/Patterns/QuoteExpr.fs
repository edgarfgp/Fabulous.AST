namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module QuoteExpr =
    let Value = Attributes.defineScalar<StringOrWidget<Expr>> "Value"

    let WidgetKey =
        Widgets.register "IsInst" (fun widget ->
            let value = Widgets.getScalarValue widget Value

            let value =
                match value with
                | StringOrWidget.StringExpr value ->
                    let value = StringParsing.normalizeIdentifierBackticks value
                    Expr.Constant(Constant.FromText(SingleTextNode.Create(value)))
                | StringOrWidget.WidgetExpr value -> value

            Pattern.QuoteExpr(
                ExprQuoteNode(SingleTextNode.leftQuotation, value, SingleTextNode.rightQuotation, Range.Zero)
            ))

[<AutoOpen>]
module QuoteExprBuilders =
    type Ast with

        static member QuoteExprPat(value: WidgetBuilder<Expr>) =
            WidgetBuilder<Pattern>(
                QuoteExpr.WidgetKey,
                AttributesBundle(
                    StackList.one(QuoteExpr.Value.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak value))),
                    Array.empty,
                    Array.empty
                )
            )

        static member QuoteExprPat(value: StringVariant) =
            WidgetBuilder<Pattern>(
                QuoteExpr.WidgetKey,
                AttributesBundle(
                    StackList.one(QuoteExpr.Value.WithValue(StringOrWidget.StringExpr(value))),
                    Array.empty,
                    Array.empty
                )
            )
