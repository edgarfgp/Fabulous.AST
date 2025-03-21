namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ParenExpr =
    let Value = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "Paren" (fun widget ->
            let expr = Widgets.getNodeFromWidget<Expr> widget Value

            Expr.Paren(
                ExprParenNode(SingleTextNode.leftParenthesis, expr, SingleTextNode.rightParenthesis, Range.Zero)
            ))

[<AutoOpen>]
module ParenBuilders =
    type Ast with

        static member ParenExpr(value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(ParenExpr.WidgetKey, ParenExpr.Value.WithValue(value.Compile()))

        static member ParenExpr(value: WidgetBuilder<Constant>) = Ast.ParenExpr(Ast.ConstantExpr(value))

        static member ParenExpr(value: string) = Ast.ParenExpr(Ast.Constant(value))
