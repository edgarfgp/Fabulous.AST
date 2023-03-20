namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak


module Expr =
    let Constant = Attributes.defineWidget "Constant"
    let ExprAppNode = Attributes.defineWidget "ExprAppNode"
    
    let ConstantWidgetKey = Widgets.register "Expr.Constant" (fun widget ->
        let constant = Helpers.getNodeFromWidget<Constant> widget Constant
        Expr.Constant constant
    )
    
    let AppWidgetKey = Widgets.register "Expr.App" (fun widget ->
        let exprAppNode = Helpers.getNodeFromWidget<ExprAppNode> widget ExprAppNode
        Expr.App exprAppNode
    )
    
[<AutoOpen>]
module ExprBuilders =
    type Fabulous.AST.Ast with
        static member inline Expr(constant: WidgetBuilder<IFabConstant>) =
            WidgetBuilder<IFabExpr>(
                Expr.ConstantWidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| Expr.Constant.WithValue(constant.Compile()) |],
                    ValueNone
                )
            )
            
        static member inline Expr(exprApp: WidgetBuilder<IFabExprApp>) =
            WidgetBuilder<IFabExpr>(
                Expr.AppWidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| Expr.ExprAppNode.WithValue(exprApp.Compile()) |],
                    ValueNone
                )
            )