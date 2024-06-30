namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Quoted =
    let Value = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "Quoted" (fun widget ->
            let expr = Widgets.getNodeFromWidget widget Value
            Expr.Quote(ExprQuoteNode(SingleTextNode.leftQuotation, expr, SingleTextNode.rightQuotation, Range.Zero)))

[<AutoOpen>]
module QuotedBuilders =
    type Ast with

        static member QuotedExpr(value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                Quoted.WidgetKey,
                AttributesBundle(StackList.empty(), [| Quoted.Value.WithValue(value.Compile()) |], Array.empty)
            )

        static member QuotedExpr(value: WidgetBuilder<Constant>) = Ast.QuotedExpr(Ast.ConstantExpr(value))

        static member QuotedExpr(value: string) = Ast.QuotedExpr(Ast.Constant(value))
