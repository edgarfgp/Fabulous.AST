namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module App =
    let Name = Attributes.defineScalar<StringOrWidget<Expr>> "Name"
    let Items = Attributes.defineWidgetCollection "Items"

    let WidgetKey =
        Widgets.register "Call" (fun widget ->
            let expr = Widgets.getScalarValue widget Name

            let hasQuotes =
                Widgets.tryGetScalarValue widget Expr.HasQuotes |> ValueOption.defaultValue true

            let expr =
                match expr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(
                            SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value, hasQuotes))
                        )
                    )
                | StringOrWidget.WidgetExpr expr -> expr

            let items = Widgets.getNodesFromWidgetCollection<Expr> widget Items
            Expr.App(ExprAppNode(expr, items, Range.Zero)))

[<AutoOpen>]
module AppBuilders =
    type Ast with

        static member AppExpr(name: WidgetBuilder<Expr>) =
            CollectionBuilder<Expr, Expr>(
                App.WidgetKey,
                App.Items,
                AttributesBundle(
                    StackList.one(App.Name.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak name))),
                    Array.empty,
                    Array.empty
                )
            )

        static member AppExpr(name: string) =
            CollectionBuilder<Expr, Expr>(
                App.WidgetKey,
                App.Items,
                AttributesBundle(
                    StackList.one(App.Name.WithValue(StringOrWidget.StringExpr(name))),
                    Array.empty,
                    Array.empty
                )
            )
