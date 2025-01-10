namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module QuoteExpr =
    let Value = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "IsInst" (fun widget ->
            let value = Widgets.getNodeFromWidget widget Value

            Pattern.QuoteExpr(
                ExprQuoteNode(SingleTextNode.leftQuotation, value, SingleTextNode.rightQuotation, Range.Zero)
            ))

[<AutoOpen>]
module QuoteExprBuilders =
    type Ast with

        static member QuoteExprPat(value: WidgetBuilder<Expr>) =
            WidgetBuilder<Pattern>(QuoteExpr.WidgetKey, QuoteExpr.Value.WithValue(value.Compile()))
