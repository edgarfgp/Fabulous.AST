namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module NamedComputation =
    let NameExpr = Attributes.defineWidget "NamedExpr"
    let BodyExpr = Attributes.defineWidget "BodyExpr"

    let WidgetKey =
        Widgets.register "NamedComputation" (fun widget ->
            let bodyExpr = Widgets.getNodeFromWidget<Expr> widget BodyExpr
            let nameExpr = Widgets.getNodeFromWidget<Expr> widget NameExpr

            Expr.NamedComputation(
                ExprNamedComputationNode(
                    nameExpr,
                    SingleTextNode.leftCurlyBrace,
                    bodyExpr,
                    SingleTextNode.rightCurlyBrace,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module NamedComputationBuilders =
    type Ast with

        static member NamedComputationExpr(name: WidgetBuilder<Expr>, body: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                NamedComputation.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| NamedComputation.NameExpr.WithValue(name.Compile())
                       NamedComputation.BodyExpr.WithValue(body.Compile()) |],
                    Array.empty
                )
            )
