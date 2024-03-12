namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module IfThenElse =
    let IfExpr = Attributes.defineWidget "IfExpr"
    let ThenExpr = Attributes.defineWidget "ThenExpr"
    let ElseExpr = Attributes.defineWidget "ElseExpr"

    let WidgetKey =
        Widgets.register "IfThenElse" (fun widget ->
            let ifExpr = Widgets.getNodeFromWidget<Expr> widget IfExpr
            let thenExpr = Widgets.getNodeFromWidget<Expr> widget ThenExpr
            let elseExpr = Widgets.getNodeFromWidget<Expr> widget ElseExpr

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
                    ValueSome
                        [| IfThenElse.IfExpr.WithValue(ifExpr.Compile())
                           IfThenElse.ThenExpr.WithValue(thenExpr.Compile())
                           IfThenElse.ElseExpr.WithValue(elseExpr.Compile()) |],
                    ValueNone
                )
            )

        static member inline IfThenElseExpr(ifExpr: string, thenExpr: string, elseExpr: string) =
            WidgetBuilder<Expr>(
                IfThenElse.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome
                        [| IfThenElse.IfExpr.WithValue(Ast.ConstantExpr(ifExpr, false).Compile())
                           IfThenElse.ThenExpr.WithValue(Ast.ConstantExpr(thenExpr, false).Compile())
                           IfThenElse.ElseExpr.WithValue(Ast.ConstantExpr(elseExpr, false).Compile()) |],
                    ValueNone
                )
            )
