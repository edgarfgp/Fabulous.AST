namespace Fabulous.AST

open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

type IFabExprIfThen = inherit IFabNodeBase

module ExprIfThen =
    let If = Attributes.defineWidget "If"
    let IfExpr = Attributes.defineWidget "IfExpr"
    let Then = Attributes.defineWidget "Then"
    let ThenExpr = Attributes.defineWidget "ThenExpr"
    
    let WidgetKey = Widgets.register "ExprIfThen" (fun widget ->
        let ``if`` = Helpers.getNodeFromWidget<IfKeywordNode> widget If
        let ifExpr = Helpers.getNodeFromWidget<Expr> widget IfExpr
        let ``then`` = Helpers.getNodeFromWidget<SingleTextNode> widget Then
        let thenExpr = Helpers.getNodeFromWidget<Expr> widget ThenExpr
        
        ExprIfThenNode(``if``, ifExpr, ``then``, thenExpr, Range.Zero)
    )

[<AutoOpen>]
module ExprIfThenBuilders =
    type Fabulous.AST.Ast with
        static member inline ExprIfThen(``if``: WidgetBuilder<#IFabIfKeyword>, ifExpr: WidgetBuilder<#IFabExpr>, ``then``: WidgetBuilder<#IFabSingleText>, thenExpr: WidgetBuilder<#IFabExpr>) =
            WidgetBuilder<IFabExprIfThen>(
                ExprIfThen.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [|
                        ExprIfThen.If.WithValue(``if``.Compile())
                        ExprIfThen.IfExpr.WithValue(ifExpr.Compile())
                        ExprIfThen.Then.WithValue(``then``.Compile())
                        ExprIfThen.ThenExpr.WithValue(thenExpr.Compile())
                    |],
                    ValueNone
                )
            )