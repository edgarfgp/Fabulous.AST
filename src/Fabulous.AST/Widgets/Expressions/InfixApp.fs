namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module InfixApp =
    let LeftHandSide = Attributes.defineWidget "LeftHandSide"
    let Operator = Attributes.defineScalar<string> "Operator"
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
        static member InfixAppExpr(lhs: WidgetBuilder<Expr>, operator: string, rhs: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                InfixApp.WidgetKey,
                AttributesBundle(
                    StackList.one(InfixApp.Operator.WithValue(operator)),
                    [| InfixApp.LeftHandSide.WithValue(lhs.Compile())
                       InfixApp.RightHandSide.WithValue(rhs.Compile()) |],
                    Array.empty
                )
            )

        static member InfixAppExpr(lhs: WidgetBuilder<Constant>, operator: string, rhs: WidgetBuilder<Expr>) =
            Ast.InfixAppExpr(Ast.ConstantExpr(lhs), operator, rhs)

        static member InfixAppExpr(lhs: string, operator: string, rhs: WidgetBuilder<Expr>) =
            Ast.InfixAppExpr(Ast.Constant(lhs), operator, rhs)

        static member InfixAppExpr(lhs: WidgetBuilder<Constant>, operator: string, rhs: WidgetBuilder<Constant>) =
            Ast.InfixAppExpr(lhs, operator, Ast.ConstantExpr(rhs))

        static member InfixAppExpr(lhs: string, operator: string, rhs: WidgetBuilder<Constant>) =
            Ast.InfixAppExpr(Ast.Constant(lhs), operator, rhs)

        static member InfixAppExpr(lhs: string, operator: string, rhs: string) =
            Ast.InfixAppExpr(lhs, operator, Ast.Constant(rhs))
