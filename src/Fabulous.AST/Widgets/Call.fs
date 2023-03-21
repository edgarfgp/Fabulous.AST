namespace Fabulous.AST

open System
open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module Call =
    let FunctionName = Attributes.defineScalar "FunctionName"
    let Arguments = Attributes.defineScalar<string array> "Arguments"
    
    let WidgetKey = Widgets.register "Call" (fun widget ->
        let functionName = Helpers.getScalarValue widget FunctionName
        let args = Helpers.getScalarValue widget Arguments
        
        Expr.App(
            ExprAppNode(
                Expr.Constant(Constant.FromText(SingleTextNode(functionName, Range.Zero))),
                [
                    for arg in args do
                        Expr.Constant(Constant.FromText(SingleTextNode(arg, Range.Zero)))
                ],
                Range.Zero
            )
        )
    )

[<AutoOpen>]
module CallBuilders =
    type Fabulous.AST.Ast with
        static member inline Call(functionName: string, [<ParamArray>] args: string[]) =
            WidgetBuilder<Expr>(
                Call.WidgetKey,
                Call.FunctionName.WithValue(functionName),
                Call.Arguments.WithValue(args)
            )
            
[<Extension>]
type CallYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<Oak, ModuleDecl>, x: WidgetBuilder<Expr>) : Content =
        { Widgets = MutStackArray1.One(DeclExpr(x).Compile()) }
