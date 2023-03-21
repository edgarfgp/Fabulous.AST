namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Microsoft.FSharp.Core

type IFabExprApp = inherit IFabNodeBase
type IFabExpr = interface end

module ExprApp =
    let FunctionExpr = Attributes.defineWidget "FunctionExpr"
    let Arguments = Attributes.defineWidgetCollection "Arguments"
    
    let WidgetKey = Widgets.register "ExprApp" (fun widget ->
        let functionExpr = Helpers.getNodeFromWidget<Expr> widget FunctionExpr
        let arguments = Helpers.getNodesFromWidgetCollection<Expr> widget Arguments
        ExprAppNode(functionExpr, arguments, Range.Zero)
    )

[<AutoOpen>]
module ExprAppBuilders =
    type Fabulous.AST.Ast with
        static member inline ExprApp(functionExpr: WidgetBuilder<IFabExpr>) =
            CollectionBuilder<IFabExprApp, IFabExpr>(
                ExprApp.WidgetKey,
                StackList.empty(),
                ValueSome [|
                    ExprApp.FunctionExpr.WithValue(functionExpr.Compile())
                |],
                ExprApp.Arguments
            )
            
[<Extension>]
type ExprAppYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<#IFabExprApp, IFabExpr>, x: WidgetBuilder<#IFabExpr>) : Content =
        { Widgets = MutStackArray1.One(x.Compile()) }