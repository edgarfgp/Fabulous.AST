namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module App =
    let Name = Attributes.defineWidget "Name"
    let Items = Attributes.defineWidgetCollection "Items"

    let WidgetKey =
        Widgets.register "Call" (fun widget ->
            let name = Widgets.getNodeFromWidget widget Name
            let items = Widgets.getNodesFromWidgetCollection<Expr> widget Items
            Expr.App(ExprAppNode(name, items, Range.Zero)))

[<AutoOpen>]
module AppBuilders =
    type Ast with

        static member inline AppExpr(name: WidgetBuilder<Expr>) =
            CollectionBuilder<Expr, Expr>(
                App.WidgetKey,
                App.Items,
                AttributesBundle(StackList.empty(), ValueSome [| App.Name.WithValue(name.Compile()) |], ValueNone)
            )

        static member inline AppExpr(name: string) =
            Ast.AppExpr(Ast.ConstantExpr(name, false))
