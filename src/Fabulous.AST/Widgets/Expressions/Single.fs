namespace Fabulous.AST

open Fabulous.AST
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
            WidgetBuilder<Expr>(Single.WidgetKey, Single.SingleNode.WithValue(value.Compile()))

        static member SingleExpr(leading: string, value: WidgetBuilder<Expr>) =
            Ast.SingleExpr(Ast.SingleNode(leading, value))

        static member SingleExpr(leading: string, value: WidgetBuilder<Constant>) =
            Ast.SingleExpr(leading, Ast.ConstantExpr(value))

        static member SingleExpr(leading: string, value: string) =
            Ast.SingleExpr(leading, Ast.Constant(value))
