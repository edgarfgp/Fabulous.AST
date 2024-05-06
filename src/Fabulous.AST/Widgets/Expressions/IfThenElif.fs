namespace Fabulous.AST

open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module IfThenElif =
    let Branches = Attributes.defineScalar<Expr list> "ElifExpr"

    let ElseExpr = Attributes.defineWidget "ElseExpr"

    let WidgetKey =
        Widgets.register "IfThenElif" (fun widget ->
            let branches =
                Widgets.getScalarValue widget Branches
                |> List.choose(fun x ->
                    match Expr.Node(x) with
                    | :? ExprIfThenNode as node -> Some node
                    | _ -> None)

            let elseExpr = Widgets.tryGetNodeFromWidget widget ElseExpr

            let elseExpr =
                match elseExpr with
                | ValueNone -> None
                | ValueSome expr -> Some(SingleTextNode.``else``, expr)

            Expr.IfThenElif(ExprIfThenElifNode(branches, elseExpr, Range.Zero)))

[<AutoOpen>]
module IfThenElifBuilders =
    type Ast with

        static member inline IfThenElifExpr(branches: WidgetBuilder<Expr> list, elseExpr: WidgetBuilder<Expr>) =
            let branches = branches |> List.map Gen.mkOak

            WidgetBuilder<Expr>(
                IfThenElif.WidgetKey,
                AttributesBundle(
                    StackList.one(IfThenElif.Branches.WithValue(branches)),
                    [| IfThenElif.ElseExpr.WithValue(elseExpr.Compile()) |],
                    Array.empty
                )
            )

        static member inline IfThenElifExpr(branches: WidgetBuilder<Expr> list) =
            WidgetBuilder<Expr>(
                IfThenElif.WidgetKey,
                AttributesBundle(
                    StackList.one(IfThenElif.Branches.WithValue(branches |> List.map Gen.mkOak)),
                    Array.empty,
                    Array.empty
                )
            )
