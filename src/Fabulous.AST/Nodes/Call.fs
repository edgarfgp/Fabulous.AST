namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core
open Fantomas.Core.SyntaxOak
open Fabulous

open type Fabulous.AST.Node

type IFabCall = inherit IFabExpr

module Call =
    let FunctionName = Attributes.defineScalar<string> "FunctionName"
    let Argument1 = Attributes.defineScalar<string> "Argument1"
    let Argument2 = Attributes.defineScalar<string> "Argument2"
    
    let WidgetKey = Widgets.register "Call" (fun (widget: Widget) ->
        let functionName = Helpers.getScalarValue widget FunctionName
        let argument1 = Helpers.getScalarValue widget Argument1
        let argument2 = Helpers.getScalarValue widget Argument2
        
        let functionExpr =
            Expr.Constant(Constant.FromText(SingleTextNode(functionName, Range.Zero)))
            
        let argument1Expr =
            Expr.Constant(Constant.FromText(SingleTextNode(argument1, Range.Zero)))
            
        let argument2Expr =
            Expr.Constant(Constant.FromText(SingleTextNode(argument2, Range.Zero)))
        
        Expr.App(
            ExprAppNode(
                functionExpr,
                [ argument1Expr; argument2Expr ],
                Range.Zero
            )
        )
    )

[<AutoOpen>]
module CallBuilders =
    type Fabulous.AST.Node with
        static member inline Call(functionName: string, argument1: string, argument2: string) =
            WidgetBuilder<IFabCall>(
                Call.WidgetKey,
                Call.FunctionName.WithValue(functionName),
                Call.Argument1.WithValue(argument1),
                Call.Argument2.WithValue(argument2)
            )
            
[<Extension>]
type CallYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<#IFabModuleOrNamespace, IFabNodeBase>, x: WidgetBuilder<IFabCall>) : Content =
        { Widgets = MutStackArray1.One(DeclExpr(x).Compile()) }