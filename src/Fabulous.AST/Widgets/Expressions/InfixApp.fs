namespace Fabulous.AST

open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module InfixApp =
    let LeftHandSide = Attributes.defineWidget "LeftHandSide"
    let Operator = Attributes.defineScalar "Operator"
    let RightHandSide = Attributes.defineWidget "RightHandSide"

    let WidgetKey =
        Widgets.register "Condition" (fun widget ->
            let lhs = Widgets.getNodeFromWidget widget LeftHandSide
            let operator = Widgets.getScalarValue widget Operator
            let rhs = Widgets.getNodeFromWidget widget RightHandSide
            Expr.InfixApp(ExprInfixAppNode(lhs, SingleTextNode.Create(operator), rhs, Range.Zero)))

[<AutoOpen>]
module InfixAppBuilders =
    type Ast with

        static member inline InfixAppExpr(lhs: WidgetBuilder<Expr>, operator: string, rhs: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                InfixApp.WidgetKey,
                AttributesBundle(
                    StackList.one(InfixApp.Operator.WithValue(operator)),
                    [| InfixApp.LeftHandSide.WithValue(lhs.Compile())
                       InfixApp.RightHandSide.WithValue(rhs.Compile()) |],
                    Array.empty
                )
            )
