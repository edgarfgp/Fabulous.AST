namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Paren =
    let Value = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "Paren" (fun widget ->
            let expr = Helpers.getNodeFromWidget<Expr> widget Value

            Expr.Paren(
                ExprParenNode(SingleTextNode.leftParenthesis, expr, SingleTextNode.rightParenthesis, Range.Zero)
            ))

[<AutoOpen>]
module ParenBuilders =
    type Ast with

        static member ParenExpr(value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                Paren.WidgetKey,
                AttributesBundle(StackList.empty(), ValueSome [| Paren.Value.WithValue(value.Compile()) |], ValueNone)
            )
