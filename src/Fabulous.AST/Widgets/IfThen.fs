namespace Fabulous.AST

open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module IfThen =
    let LeftHandSide = Attributes.defineScalar "LeftHandSide"
    let Operator = Attributes.defineScalar "Operator"
    let RightHandSide = Attributes.defineScalar "RightHandSide"
    let ThenExpr = Attributes.defineWidget "ThenExpr"

    let WidgetKey =
        Widgets.register "IfThen" (fun widget ->
            let lhs = Helpers.getScalarValue widget LeftHandSide
            let operator = Helpers.getScalarValue widget Operator
            let rhs = Helpers.getScalarValue widget RightHandSide
            let thenExpr = Helpers.getNodeFromWidget<Expr> widget ThenExpr

            Expr.IfThen(
                ExprIfThenNode(
                    IfKeywordNode.SingleWord(SingleTextNode("if", Range.Zero)),
                    Expr.InfixApp(
                        ExprInfixAppNode(
                            Expr.Ident(SingleTextNode(lhs, Range.Zero)),
                            SingleTextNode(operator, Range.Zero),
                            Expr.Ident(SingleTextNode(rhs, Range.Zero)),
                            Range.Zero
                        )
                    ),
                    SingleTextNode("then", Range.Zero),
                    thenExpr,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module IfThenBuilders =
    type Fabulous.AST.Ast with

        static member inline IfThen(lhs: string, operator: string, rhs: string, thenExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                IfThen.WidgetKey,
                AttributesBundle(
                    StackList.three (
                        IfThen.LeftHandSide.WithValue(lhs),
                        IfThen.Operator.WithValue(operator),
                        IfThen.RightHandSide.WithValue(rhs)
                    ),
                    ValueSome [| IfThen.ThenExpr.WithValue(thenExpr.Compile()) |],
                    ValueNone
                )
            )
