namespace Fabulous.AST

open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module IfThenElse =
    let IfExpr = Attributes.defineWidget "IfExpr"
    let ThenExpr = Attributes.defineWidget "ThenExpr"
    let ElseExpr = Attributes.defineWidget "ElseExpr"

    let WidgetKey =
        Widgets.register "IfThenElse" (fun widget ->
            let ifExpr = Widgets.getNodeFromWidget widget IfExpr
            let thenExpr = Widgets.getNodeFromWidget widget ThenExpr
            let elseExpr = Widgets.getNodeFromWidget widget ElseExpr

            Expr.IfThenElse(
                ExprIfThenElseNode(
                    IfKeywordNode.SingleWord(SingleTextNode.``if``),
                    ifExpr,
                    SingleTextNode.``then``,
                    thenExpr,
                    SingleTextNode.``else``,
                    elseExpr,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module IfThenElseBuilders =
    type Ast with

        static member inline IfThenElseExpr
            (ifExpr: WidgetBuilder<Expr>, thenExpr: WidgetBuilder<Expr>, elseExpr: WidgetBuilder<Expr>)
            =
            WidgetBuilder<Expr>(
                IfThenElse.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| IfThenElse.IfExpr.WithValue(ifExpr.Compile())
                       IfThenElse.ThenExpr.WithValue(thenExpr.Compile())
                       IfThenElse.ElseExpr.WithValue(elseExpr.Compile()) |],
                    Array.empty
                )
            )
