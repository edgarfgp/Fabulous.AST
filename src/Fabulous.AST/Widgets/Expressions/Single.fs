namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

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

        static member SingleExpr(leading: string, value: WidgetBuilder<Expr>) =
            Ast.SingleExpr(Ast.SingleNode(leading, value))

        static member SingleExpr(leading: string, value: WidgetBuilder<Constant>) =
            Ast.SingleExpr(leading, Ast.ConstantExpr(value))

        static member SingleExpr(leading: string, value: string) =
            Ast.SingleExpr(leading, Ast.Constant(value))
