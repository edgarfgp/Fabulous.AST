namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module App =
    let Name = Attributes.defineScalar<StringOrWidget<Expr>> "Name"
    let Items = Attributes.defineScalar<Expr list> "Items"

    let WidgetKey =
        Widgets.register "Call" (fun widget ->
            let expr = Widgets.getScalarValue widget Name

            let expr =
                match expr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value)))
                    )
                | StringOrWidget.WidgetExpr expr -> expr

            let items = Widgets.getScalarValue widget Items
            Expr.App(ExprAppNode(expr, items, Range.Zero)))

[<AutoOpen>]
module AppBuilders =
    type Ast with

        static member AppExpr(name: WidgetBuilder<Expr>, items: WidgetBuilder<Expr> list) =
            let items = items |> List.map Gen.mkOak

            WidgetBuilder<Expr>(
                App.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        App.Name.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak name)),
                        App.Items.WithValue(items)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member AppExpr(name: WidgetBuilder<Expr>, item: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                App.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        App.Name.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak name)),
                        App.Items.WithValue([ Gen.mkOak item ])
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member AppExpr(name: string, items: WidgetBuilder<Expr> list) =
            let items = items |> List.map Gen.mkOak

            WidgetBuilder<Expr>(
                App.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        App.Name.WithValue(StringOrWidget.StringExpr(Unquoted(name))),
                        App.Items.WithValue(items)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member AppExpr(name: string, item: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                App.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        App.Name.WithValue(StringOrWidget.StringExpr(Unquoted(name))),
                        App.Items.WithValue([ Gen.mkOak item ])
                    ),
                    Array.empty,
                    Array.empty
                )
            )
