namespace Fabulous.AST

open System
open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Single =
    let SingleNode = Attributes.defineWidget "SingleNode"

    let WidgetKey =
        Widgets.register "Single" (fun widget ->
            let singleNode = Widgets.getNodeFromWidget widget SingleNode
            Expr.Single(singleNode))

[<AutoOpen>]
module SingleBuilders =
    type Ast with

        static member SingleExpr(value: WidgetBuilder<ExprSingleNode>) =
            WidgetBuilder<Expr>(
                Single.WidgetKey,
                AttributesBundle(StackList.empty(), [| Single.SingleNode.WithValue(value.Compile()) |], Array.empty)
            )
