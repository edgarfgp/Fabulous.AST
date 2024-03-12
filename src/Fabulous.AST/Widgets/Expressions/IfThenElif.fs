namespace Fabulous.AST

open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module IfThenElif =
    let Branches = Attributes.defineWidgetCollection "ElifExpr"
    let Branch = Attributes.defineWidget "ElifExpr"

    let ElseExpr = Attributes.defineWidget "ElseExpr"

    let WidgetKey =
        Widgets.register "IfThenElif" (fun widget ->
            let branches =
                Widgets.getNodesFromWidgetCollection<Expr> widget Branches
                |> List.choose(fun x ->
                    match Expr.Node(x) with
                    | :? ExprIfThenNode as node -> Some node
                    | _ -> None)

            let elseExpr = Widgets.tryGetNodeFromWidget<Expr> widget ElseExpr

            let elseExpr =
                match elseExpr with
                | ValueSome elseExpr -> Some(SingleTextNode.``else``, elseExpr)
                | ValueNone -> None

            Expr.IfThenElif(ExprIfThenElifNode(branches, elseExpr, Range.Zero)))

[<AutoOpen>]
module IfThenElifBuilders =
    type Ast with

        static member inline IfThenElifExpr(elseExpr: WidgetBuilder<Expr>) =
            CollectionBuilder<Expr, Expr>(
                IfThenElif.WidgetKey,
                IfThenElif.Branches,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| IfThenElif.ElseExpr.WithValue(elseExpr.Compile()) |],
                    ValueNone
                )
            )

        static member inline IfThenElifExpr(elseExpr: string) =
            CollectionBuilder<Expr, Expr>(
                IfThenElif.WidgetKey,
                IfThenElif.Branches,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| IfThenElif.ElseExpr.WithValue(Ast.ConstantExpr(elseExpr, false).Compile()) |],
                    ValueNone
                )
            )

        static member inline IfThenElifExpr() =
            CollectionBuilder<Expr, Expr>(
                IfThenElif.WidgetKey,
                IfThenElif.Branches,
                AttributesBundle(StackList.empty(), ValueNone, ValueNone)
            )
