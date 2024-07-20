namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Computation =
    let BodyExpr = Attributes.defineWidget "BodyExpr"

    let WidgetKey =
        Widgets.register "Computation" (fun widget ->
            let bodyExpr = Widgets.getNodeFromWidget<Expr> widget BodyExpr

            Expr.Computation(
                ExprComputationNode(
                    SingleTextNode.leftCurlyBrace,
                    bodyExpr,
                    SingleTextNode.rightCurlyBrace,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module ComputationBuilders =
    type Ast with

        static member ComputationExpr(body: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                Computation.WidgetKey,
                AttributesBundle(StackList.empty(), [| Computation.BodyExpr.WithValue(body.Compile()) |], Array.empty)
            )

        static member ComputationExpr(body: WidgetBuilder<Constant>) =
            Ast.ComputationExpr(Ast.ConstantExpr(body))

        static member ComputationExpr(body: string) = Ast.ComputationExpr(Ast.Constant(body))
