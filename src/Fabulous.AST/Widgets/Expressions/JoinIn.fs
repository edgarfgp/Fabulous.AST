namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module JoinIn =
    let LHS = Attributes.defineWidget "LHS"
    let RHS = Attributes.defineWidget "RHS"

    let WidgetKey =
        Widgets.register "JoinIn" (fun widget ->
            let lhs = Widgets.getNodeFromWidget<Expr> widget LHS
            let rhs = Widgets.getNodeFromWidget<Expr> widget RHS
            Expr.JoinIn(ExprJoinInNode(lhs, SingleTextNode.``in``, rhs, Range.Zero)))

[<AutoOpen>]
module JoinInBuilders =
    type Ast with

        static member JoinInExpr(lhs: WidgetBuilder<Expr>, rhs: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                JoinIn.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| JoinIn.LHS.WithValue(lhs.Compile()); JoinIn.RHS.WithValue(rhs.Compile()) |],
                    Array.empty
                )
            )

        static member JoinInExpr(lhs: WidgetBuilder<Constant>, rhs: WidgetBuilder<Expr>) =
            Ast.JoinInExpr(Ast.ConstantExpr(lhs), rhs)

        static member JoinInExpr(lhs: WidgetBuilder<Expr>, rhs: WidgetBuilder<Constant>) =
            Ast.JoinInExpr(lhs, Ast.ConstantExpr(rhs))

        static member JoinInExpr(lhs: WidgetBuilder<Constant>, rhs: WidgetBuilder<Constant>) =
            Ast.JoinInExpr(Ast.ConstantExpr(lhs), Ast.ConstantExpr(rhs))

        static member JoinInExpr(lhs: WidgetBuilder<Expr>, rhs: string) = Ast.JoinInExpr(lhs, Ast.Constant(rhs))

        static member JoinInExpr(lhs: string, rhs: WidgetBuilder<Expr>) = Ast.JoinInExpr(Ast.Constant(lhs), rhs)

        static member JoinInExpr(lhs: string, rhs: string) =
            Ast.JoinInExpr(Ast.Constant(lhs), Ast.Constant(rhs))
