namespace Fabulous.AST

open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

type IFabExprInfixApp = interface end

module ExprInfixApp =
    let LeftHandSide = Attributes.defineWidget "LeftHandSide"
    let Operator = Attributes.defineWidget "Operator"
    let RightHandSide = Attributes.defineWidget "RightHandSide"
    
    let WidgetKey = Widgets.register "ExprInfixApp" (fun widget ->
        let lhs = Helpers.getNodeFromWidget<Expr> widget LeftHandSide
        let operator = Helpers.getNodeFromWidget<SingleTextNode> widget Operator
        let rhs = Helpers.getNodeFromWidget<Expr> widget RightHandSide
        ExprInfixAppNode(lhs, operator, rhs, Range.Zero)
    )
    
[<AutoOpen>]
module ExprInfixAppBuilders =
    type Fabulous.AST.Ast with
        static member inline ExprInfixApp(lhs: WidgetBuilder<IFabExpr>, operator: WidgetBuilder<IFabSingleText>, rhs: WidgetBuilder<IFabExpr>) =
            WidgetBuilder<IFabExprInfixApp>(
                ExprInfixApp.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [|
                        ExprInfixApp.LeftHandSide.WithValue(lhs.Compile())
                        ExprInfixApp.Operator.WithValue(operator.Compile())
                        ExprInfixApp.RightHandSide.WithValue(rhs.Compile())
                    |],
                    ValueNone
                )
            )