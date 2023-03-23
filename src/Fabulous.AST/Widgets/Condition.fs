namespace Fabulous.AST

open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module Condition =
    let LeftHandSide = Attributes.defineWidget "LeftHandSide"
    let Operator = Attributes.defineScalar "Operator"
    let RightHandSide = Attributes.defineWidget "RightHandSide"

    let WidgetKey =
        Widgets.register "Condition" (fun widget ->
            let lhs = Helpers.getNodeFromWidget<Expr> widget LeftHandSide
            let operator = Helpers.getScalarValue widget Operator
            let rhs = Helpers.getNodeFromWidget<Expr> widget RightHandSide

            Expr.InfixApp(ExprInfixAppNode(lhs, SingleTextNode(operator, Range.Zero), rhs, Range.Zero)))

[<AutoOpen>]
module ConditionBuilders =
    type Fabulous.AST.Ast with

        static member inline Condition(lhs: WidgetBuilder<Expr>, operator: string, rhs: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                Condition.WidgetKey,
                AttributesBundle(
                    StackList.one(Condition.Operator.WithValue(operator)),
                    ValueSome
                        [| Condition.LeftHandSide.WithValue(lhs.Compile())
                           Condition.RightHandSide.WithValue(rhs.Compile()) |],
                    ValueNone
                )
            )
