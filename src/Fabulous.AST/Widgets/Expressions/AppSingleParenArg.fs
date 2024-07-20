namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module AppSingleParenArg =
    let FuncExpr = Attributes.defineWidget "FuncExpr"

    let ArgExpr = Attributes.defineWidget "ArgExpr"

    let WidgetKey =
        Widgets.register "AppSingleParenArg" (fun widget ->
            let funcExpr = Widgets.getNodeFromWidget widget FuncExpr
            let argExpr = Widgets.getNodeFromWidget widget ArgExpr
            Expr.AppSingleParenArg(ExprAppSingleParenArgNode(funcExpr, argExpr, Range.Zero)))

[<AutoOpen>]
module AppSingleParenArgBuilders =
    type Ast with

        static member AppSingleParenArgExpr(funcExpr: WidgetBuilder<Expr>, argExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                AppSingleParenArg.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| AppSingleParenArg.FuncExpr.WithValue(funcExpr.Compile())
                       AppSingleParenArg.ArgExpr.WithValue(argExpr.Compile()) |],
                    Array.empty
                )
            )

        static member AppSingleParenArgExpr(value: WidgetBuilder<Constant>, argExpr: WidgetBuilder<Expr>) =
            Ast.AppSingleParenArgExpr(Ast.ConstantExpr(value), argExpr)

        static member AppSingleParenArgExpr(value: WidgetBuilder<Expr>, argExpr: WidgetBuilder<Constant>) =
            Ast.AppSingleParenArgExpr(value, Ast.ConstantExpr argExpr)

        static member AppSingleParenArgExpr(value: WidgetBuilder<Constant>, argExpr: WidgetBuilder<Constant>) =
            Ast.AppSingleParenArgExpr(value, Ast.ConstantExpr argExpr)

        static member AppSingleParenArgExpr(value: string, argExpr: WidgetBuilder<Expr>) =
            Ast.AppSingleParenArgExpr(Ast.ConstantExpr value, argExpr)

        static member AppSingleParenArgExpr(value: WidgetBuilder<Expr>, argExpr: string) =
            Ast.AppSingleParenArgExpr(value, Ast.ConstantExpr argExpr)

        static member AppSingleParenArgExpr(value: string, argExpr: WidgetBuilder<Constant>) =
            Ast.AppSingleParenArgExpr(Ast.ConstantExpr value, argExpr)

        static member AppSingleParenArgExpr(value: WidgetBuilder<Constant>, argExpr: string) =
            Ast.AppSingleParenArgExpr(Ast.ConstantExpr value, argExpr)

        static member AppSingleParenArgExpr(value: string, argExpr: string) =
            Ast.AppSingleParenArgExpr(Ast.ConstantExpr value, Ast.ConstantExpr argExpr)
