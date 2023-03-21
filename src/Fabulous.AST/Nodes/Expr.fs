namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak


module Expr =
    let Constant = Attributes.defineWidget "Constant"
    let ExprApp = Attributes.defineWidget "ExprApp"
    let ExprIfThen = Attributes.defineWidget "ExprIfThen"
    let ExprInfixApp = Attributes.defineWidget "ExprInfixApp"
    let SingleText = Attributes.defineWidget "SingleText"
    
    let ConstantWidgetKey = Widgets.register "Expr.Constant" (fun widget ->
        let constant = Helpers.getNodeFromWidget<Constant> widget Constant
        Expr.Constant constant
    )
    
    let AppWidgetKey = Widgets.register "Expr.App" (fun widget ->
        let exprApp = Helpers.getNodeFromWidget<ExprAppNode> widget ExprApp
        Expr.App exprApp
    )
    
    let IfThenWidgetKey = Widgets.register "Expr.IfThen" (fun widget ->
        let exprIfThen = Helpers.getNodeFromWidget<ExprIfThenNode> widget ExprIfThen
        Expr.IfThen exprIfThen
    )
    
    let InfixAppWidgetKey = Widgets.register "Expr.InfixApp" (fun widget ->
        let exprInfixApp = Helpers.getNodeFromWidget<ExprInfixAppNode> widget ExprInfixApp
        Expr.InfixApp exprInfixApp
    )
    
    let IdentWidgetKey = Widgets.register "Expr.Ident" (fun widget ->
        let singleText = Helpers.getNodeFromWidget<SingleTextNode> widget SingleText
        Expr.Ident singleText
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
                    ValueSome [| Expr.ExprApp.WithValue(exprApp.Compile()) |],
                    ValueNone
                )
            )
            
        static member inline Expr(exprIfThen: WidgetBuilder<IFabExprIfThen>) =
            WidgetBuilder<IFabExpr>(
                Expr.IfThenWidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| Expr.ExprIfThen.WithValue(exprIfThen.Compile()) |],
                    ValueNone
                )
            )
            
        static member inline Expr(exprInfixApp: WidgetBuilder<IFabExprInfixApp>) =
            WidgetBuilder<IFabExpr>(
                Expr.InfixAppWidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| Expr.ExprInfixApp.WithValue(exprInfixApp.Compile()) |],
                    ValueNone
                )
            )
            
        static member inline Expr_Ident(singleText: WidgetBuilder<IFabSingleText>) =
            WidgetBuilder<IFabExpr>(
                Expr.IdentWidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| Expr.SingleText.WithValue(singleText.Compile()) |],
                    ValueNone
                )
            )