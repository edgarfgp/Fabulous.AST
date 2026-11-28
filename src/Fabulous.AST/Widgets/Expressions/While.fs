namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module While =
    let WhileExpr = Attributes.defineWidget "WhileExpr"

    let BodyExpr = Attributes.defineWidget "BodyExpr"

    let private whileNode (keyword: SingleTextNode) (widget: Widget) =
        let whileExpr = Widgets.getNodeFromWidget widget WhileExpr
        let doExpr = Widgets.getNodeFromWidget widget BodyExpr
        Expr.While(ExprWhileNode(keyword, whileExpr, doExpr, Range.range0))

    let WidgetKey = Widgets.register "While" (whileNode SingleTextNode.``while``)

    let BangWidgetKey =
        Widgets.register "WhileBang" (whileNode SingleTextNode.whileBang)

[<AutoOpen>]
module WhileBuilders =
    type Ast with

        static member WhileExpr(expr: WidgetBuilder<Expr>, bodyExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                While.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| While.WhileExpr.WithValue(expr.Compile())
                       While.BodyExpr.WithValue(bodyExpr.Compile()) |],
                    Array.empty
                )
            )

        static member WhileExpr(expr: WidgetBuilder<Constant>, bodyExpr: WidgetBuilder<Expr>) =
            Ast.WhileExpr(Ast.ConstantExpr(expr), bodyExpr)

        static member WhileExpr(expr: string, bodyExpr: WidgetBuilder<Expr>) =
            Ast.WhileExpr(Ast.Constant(expr), bodyExpr)

        static member WhileExpr(expr: WidgetBuilder<Expr>, bodyExpr: WidgetBuilder<Constant>) =
            Ast.WhileExpr(expr, Ast.ConstantExpr(bodyExpr))

        static member WhileExpr(expr: WidgetBuilder<Constant>, bodyExpr: WidgetBuilder<Constant>) =
            Ast.WhileExpr(Ast.ConstantExpr(expr), Ast.ConstantExpr(bodyExpr))

        static member WhileExpr(expr: string, bodyExpr: WidgetBuilder<Constant>) =
            Ast.WhileExpr(Ast.Constant(expr), bodyExpr)

        static member WhileExpr(expr: WidgetBuilder<Expr>, bodyExpr: string) =
            Ast.WhileExpr(expr, Ast.Constant(bodyExpr))

        static member WhileExpr(expr: WidgetBuilder<Constant>, bodyExpr: string) =
            Ast.WhileExpr(expr, Ast.Constant(bodyExpr))

        static member WhileExpr(expr: string, bodyExpr: string) =
            Ast.WhileExpr(Ast.Constant(expr), Ast.Constant(bodyExpr))

        /// <summary>
        /// Create a <c>while!</c> loop, which awaits its condition on each iteration inside a computation expression.
        /// </summary>
        /// <param name="expr">The computation producing the loop condition.</param>
        /// <param name="bodyExpr">The loop body.</param>
        /// <code language="fsharp">
        /// NamedComputationExpr("task", WhileBangExpr("reader.ReadAsync()", "process reader"))
        /// </code>
        static member WhileBangExpr(expr: WidgetBuilder<Expr>, bodyExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                While.BangWidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| While.WhileExpr.WithValue(expr.Compile())
                       While.BodyExpr.WithValue(bodyExpr.Compile()) |],
                    Array.empty
                )
            )

        static member WhileBangExpr(expr: WidgetBuilder<Constant>, bodyExpr: WidgetBuilder<Expr>) =
            Ast.WhileBangExpr(Ast.ConstantExpr(expr), bodyExpr)

        static member WhileBangExpr(expr: string, bodyExpr: WidgetBuilder<Expr>) =
            Ast.WhileBangExpr(Ast.Constant(expr), bodyExpr)

        static member WhileBangExpr(expr: WidgetBuilder<Expr>, bodyExpr: WidgetBuilder<Constant>) =
            Ast.WhileBangExpr(expr, Ast.ConstantExpr(bodyExpr))

        static member WhileBangExpr(expr: WidgetBuilder<Constant>, bodyExpr: WidgetBuilder<Constant>) =
            Ast.WhileBangExpr(Ast.ConstantExpr(expr), Ast.ConstantExpr(bodyExpr))

        static member WhileBangExpr(expr: string, bodyExpr: WidgetBuilder<Constant>) =
            Ast.WhileBangExpr(Ast.Constant(expr), bodyExpr)

        static member WhileBangExpr(expr: WidgetBuilder<Expr>, bodyExpr: string) =
            Ast.WhileBangExpr(expr, Ast.Constant(bodyExpr))

        static member WhileBangExpr(expr: WidgetBuilder<Constant>, bodyExpr: string) =
            Ast.WhileBangExpr(expr, Ast.Constant(bodyExpr))

        static member WhileBangExpr(expr: string, bodyExpr: string) =
            Ast.WhileBangExpr(Ast.Constant(expr), Ast.Constant(bodyExpr))
