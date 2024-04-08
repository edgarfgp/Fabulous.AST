namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module While =
    let WhileExpr = Attributes.defineScalar<StringOrWidget<Expr>> "EnumExpr"

    let BodyExpr = Attributes.defineScalar<StringOrWidget<Expr>> "BodyExpr"

    let WidgetKey =
        Widgets.register "While" (fun widget ->

            let whileExpr = Widgets.getScalarValue widget WhileExpr

            let whileExpr =
                match whileExpr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value)))
                    )
                | StringOrWidget.WidgetExpr expr -> expr

            let doExpr = Widgets.getScalarValue widget BodyExpr

            let doExpr =
                match doExpr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value)))
                    )
                | StringOrWidget.WidgetExpr expr -> expr

            Expr.While(ExprWhileNode(SingleTextNode.``while``, whileExpr, doExpr, Range.Zero)))

[<AutoOpen>]
module WhileBuilders =
    type Ast with

        static member WhileExpr(expr: WidgetBuilder<Expr>, bodyExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                While.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        While.WhileExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak expr)),
                        While.BodyExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak bodyExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member WhileExpr(expr: string, bodyExpr: string) =
            WidgetBuilder<Expr>(
                While.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        While.WhileExpr.WithValue(StringOrWidget.StringExpr(Unquoted expr)),
                        While.BodyExpr.WithValue(StringOrWidget.StringExpr(Unquoted bodyExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member WhileExpr(expr: WidgetBuilder<Expr>, bodyExpr: string) =
            WidgetBuilder<Expr>(
                While.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        While.WhileExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak expr)),
                        While.BodyExpr.WithValue(StringOrWidget.StringExpr(Unquoted bodyExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member WhileExpr(expr: string, bodyExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                While.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        While.WhileExpr.WithValue(StringOrWidget.StringExpr(Unquoted expr)),
                        While.BodyExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak bodyExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )
