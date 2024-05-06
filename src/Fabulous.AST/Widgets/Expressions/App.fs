namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module App =
    let Name = Attributes.defineWidget "Name"
    let Items = Attributes.defineScalar<Expr list> "Items"

    let WidgetKey =
        Widgets.register "Call" (fun widget ->
            let expr = Widgets.getNodeFromWidget<Expr> widget Name
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
                    StackList.one(App.Items.WithValue(items)),
                    [| App.Name.WithValue(name.Compile()) |],
                    Array.empty
                )
            )

        static member AppExpr(name: WidgetBuilder<Expr>, item: WidgetBuilder<Expr>) = Ast.AppExpr(name, [ item ])
