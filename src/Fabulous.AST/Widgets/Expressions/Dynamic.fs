namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Dynamic =
    let FunExpr = Attributes.defineWidget "FunExpr"

    let ArgExpr = Attributes.defineWidget "ArgExpr"

    let WidgetKey =
        Widgets.register "Dynamic" (fun widget ->
            let funcExpr = Widgets.getNodeFromWidget widget FunExpr
            let argExpr = Widgets.getNodeFromWidget widget ArgExpr

            Expr.Dynamic(ExprDynamicNode(funcExpr, argExpr, Range.Zero)))

[<AutoOpen>]
module DynamicBuilders =
    type Ast with

        static member DynamicExpr(value: WidgetBuilder<Expr>, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                Dynamic.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| Dynamic.FunExpr.WithValue(value.Compile())
                       Dynamic.ArgExpr.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member DynamicExpr(value: WidgetBuilder<Constant>, expr: WidgetBuilder<Expr>) =
            Ast.DynamicExpr(Ast.ConstantExpr(value), expr)

        static member DynamicExpr(value: WidgetBuilder<Constant>, expr: string) =
            Ast.DynamicExpr(Ast.ConstantExpr(value), Ast.Constant(expr))

        static member DynamicExpr(value: string, expr: WidgetBuilder<Expr>) =
            Ast.DynamicExpr(Ast.Constant(value), expr)

        static member DynamicExpr(value: WidgetBuilder<Expr>, expr: WidgetBuilder<Constant>) =
            Ast.DynamicExpr(value, Ast.ConstantExpr(expr))

        static member DynamicExpr(value: WidgetBuilder<Expr>, expr: string) =
            Ast.DynamicExpr(value, Ast.Constant(expr))

        static member DynamicExpr(value: WidgetBuilder<Constant>, expr: WidgetBuilder<Constant>) =
            Ast.DynamicExpr(Ast.ConstantExpr(value), Ast.ConstantExpr(expr))

        static member DynamicExpr(value: string, expr: WidgetBuilder<Constant>) =
            Ast.DynamicExpr(Ast.Constant(value), expr)

        static member DynamicExpr(value: string, expr: string) =
            Ast.DynamicExpr(Ast.Constant(value), Ast.Constant(expr))
