namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Quoted =
    let Value = Attributes.defineScalar<StringOrWidget<Expr>> "Value"

    let WidgetKey =
        Widgets.register "Quoted" (fun widget ->
            let expr = Widgets.getScalarValue widget Value

            let hasQuotes =
                Widgets.tryGetScalarValue widget Expr.HasQuotes |> ValueOption.defaultValue true

            let expr =
                match expr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(
                            SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value, hasQuotes))
                        )
                    )
                | StringOrWidget.WidgetExpr expr -> expr

            Expr.Quote(ExprQuoteNode(SingleTextNode.leftQuotation, expr, SingleTextNode.rightQuotation, Range.Zero)))

[<AutoOpen>]
module QuotedBuilders =
    type Ast with

        static member QuotedExpr(value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                Quoted.WidgetKey,
                AttributesBundle(
                    StackList.one(Quoted.Value.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak value))),
                    Array.empty,
                    Array.empty
                )
            )

        static member QuotedExpr(value: string) =
            WidgetBuilder<Expr>(
                Quoted.WidgetKey,
                AttributesBundle(
                    StackList.one(Quoted.Value.WithValue(StringOrWidget.StringExpr(value))),
                    Array.empty,
                    Array.empty
                )
            )
