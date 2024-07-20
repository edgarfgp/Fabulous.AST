namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module TryFinally =
    let Value = Attributes.defineWidget "Value"

    let Finally = Attributes.defineWidget "Section"

    let WidgetKey =
        Widgets.register "TryFinally" (fun widget ->
            let expr = Widgets.getNodeFromWidget widget Value
            let ``finally`` = Widgets.getNodeFromWidget widget Finally

            Expr.TryFinally(
                ExprTryFinallyNode(SingleTextNode.``try``, expr, SingleTextNode.``finally``, ``finally``, Range.Zero)
            ))

[<AutoOpen>]
module TryFinallyBuilders =
    type Ast with

        static member TryFinallyExpr(value: WidgetBuilder<Expr>, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                TryFinally.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| TryFinally.Value.WithValue(value.Compile())
                       TryFinally.Finally.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member TryFinallyExpr(value: WidgetBuilder<Constant>, expr: WidgetBuilder<Expr>) =
            Ast.TryFinallyExpr(Ast.ConstantExpr(value), expr)

        static member TryFinallyExpr(value: WidgetBuilder<Constant>, expr: string) =
            Ast.TryFinallyExpr(Ast.ConstantExpr(value), Ast.Constant(expr))

        static member TryFinallyExpr(value: string, expr: WidgetBuilder<Expr>) =
            Ast.TryFinallyExpr(Ast.Constant(value), expr)

        static member TryFinallyExpr(value: WidgetBuilder<Expr>, expr: WidgetBuilder<Constant>) =
            Ast.TryFinallyExpr(value, Ast.ConstantExpr(expr))

        static member TryFinallyExpr(value: WidgetBuilder<Constant>, expr: WidgetBuilder<Constant>) =
            Ast.TryFinallyExpr(Ast.ConstantExpr(value), Ast.ConstantExpr(expr))

        static member TryFinallyExpr(value: string, expr: string) =
            Ast.TryFinallyExpr(Ast.Constant(value), Ast.Constant(expr))
