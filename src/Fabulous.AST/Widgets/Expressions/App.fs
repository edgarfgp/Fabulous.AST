namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

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

        static member AppExpr(name: WidgetBuilder<Constant>, items: WidgetBuilder<Expr> list) =
            Ast.AppExpr(Ast.ConstantExpr(name), items)

        static member AppExpr(name: string, items: WidgetBuilder<Expr> list) = Ast.AppExpr(Ast.Constant(name), items)

        static member AppExpr(name: WidgetBuilder<Expr>, items: WidgetBuilder<Constant> list) =
            Ast.AppExpr(name, items |> List.map Ast.ConstantExpr)

        static member AppExpr(name: WidgetBuilder<Expr>, items: string list) =
            Ast.AppExpr(name, items |> List.map Ast.Constant)

        static member AppExpr(name: WidgetBuilder<Constant>, items: WidgetBuilder<Constant> list) =
            Ast.AppExpr(name, items |> List.map Ast.ConstantExpr)

        static member AppExpr(name: WidgetBuilder<Constant>, items: string list) =
            Ast.AppExpr(name, items |> List.map Ast.Constant)

        static member AppExpr(name: string, items: WidgetBuilder<Constant> list) =
            Ast.AppExpr(Ast.Constant(name), items |> List.map Ast.ConstantExpr)

        static member AppExpr(name: string, items: string list) =
            Ast.AppExpr(name, items |> List.map Ast.Constant)

        static member AppExpr(name: WidgetBuilder<Expr>, item: WidgetBuilder<Expr>) = Ast.AppExpr(name, [ item ])

        static member AppExpr(name: WidgetBuilder<Constant>, item: WidgetBuilder<Expr>) = Ast.AppExpr(name, [ item ])

        static member AppExpr(name: string, item: WidgetBuilder<Expr>) = Ast.AppExpr(name, [ item ])

        static member AppExpr(name: WidgetBuilder<Expr>, item: WidgetBuilder<Constant>) = Ast.AppExpr(name, [ item ])

        static member AppExpr(name: WidgetBuilder<Constant>, item: WidgetBuilder<Constant>) =
            Ast.AppExpr(name, [ item ])

        static member AppExpr(name: string, item: WidgetBuilder<Constant>) = Ast.AppExpr(name, [ item ])

        static member AppExpr(name: WidgetBuilder<Expr>, item: string) =
            Ast.AppExpr(name, [ Ast.Constant(item) ])

        static member AppExpr(name: WidgetBuilder<Constant>, item: string) =
            Ast.AppExpr(name, [ Ast.Constant(item) ])

        static member AppExpr(name: string, item: string) =
            Ast.AppExpr(name, [ Ast.Constant(item) ])

        static member FailWithExpr(items: WidgetBuilder<Expr> list) =
            Ast.AppExpr(Ast.Constant("failwith"), items)

        static member FailWithExpr(items: WidgetBuilder<Constant> list) =
            Ast.AppExpr(Ast.Constant("failwith"), items |> List.map Ast.ConstantExpr)

        static member FailWithExpr(items: string list) =
            Ast.AppExpr(Ast.Constant("failwith"), items |> List.map Ast.Constant)

        static member FailWithExpr(item: WidgetBuilder<Expr>) = Ast.FailWithExpr([ item ])

        static member FailWithExpr(item: WidgetBuilder<Constant>) = Ast.FailWithExpr([ item ])

        static member FailWithExpr(item: string) = Ast.FailWithExpr([ Ast.String(item) ])
