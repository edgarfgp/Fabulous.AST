namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

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
        static member IfThenElifExpr(branches: WidgetBuilder<Expr> list, elseExpr: WidgetBuilder<Expr>) =
            let branches = branches |> List.map Gen.mkOak

            WidgetBuilder<Expr>(
                IfThenElif.WidgetKey,
                AttributesBundle(
                    StackList.one(IfThenElif.Branches.WithValue(branches)),
                    [| IfThenElif.ElseExpr.WithValue(elseExpr.Compile()) |],
                    Array.empty
                )
            )

        static member IfThenElifExpr(branches: WidgetBuilder<Constant> list, elseExpr: WidgetBuilder<Expr>) =
            let branches = branches |> List.map Ast.ConstantExpr
            Ast.IfThenElifExpr(branches, elseExpr)

        static member IfThenElifExpr(branches: string list, elseExpr: WidgetBuilder<Expr>) =
            let branches = branches |> List.map Ast.Constant
            Ast.IfThenElifExpr(branches, elseExpr)

        static member IfThenElifExpr(branches: WidgetBuilder<Expr> list, elseExpr: WidgetBuilder<Constant>) =
            Ast.IfThenElifExpr(branches, Ast.ConstantExpr(elseExpr))

        static member IfThenElifExpr(branches: WidgetBuilder<Expr> list, elseExpr: string) =
            Ast.IfThenElifExpr(branches, Ast.Constant(elseExpr))

        static member IfThenElifExpr(branches: WidgetBuilder<Constant> list, elseExpr: WidgetBuilder<Constant>) =
            Ast.IfThenElifExpr(branches |> List.map Ast.ConstantExpr, Ast.ConstantExpr(elseExpr))

        static member IfThenElifExpr(branches: WidgetBuilder<Constant> list, elseExpr: string) =
            Ast.IfThenElifExpr(branches |> List.map Ast.ConstantExpr, Ast.Constant(elseExpr))

        static member IfThenElifExpr(branches: string list, elseExpr: WidgetBuilder<Constant>) =
            Ast.IfThenElifExpr(branches |> List.map Ast.Constant, Ast.ConstantExpr(elseExpr))

        static member IfThenElifExpr(branches: string list, elseExpr: string) =
            Ast.IfThenElifExpr(branches, Ast.Constant(elseExpr))

        static member IfThenElifExpr(branches: WidgetBuilder<Expr> list) =
            WidgetBuilder<Expr>(
                IfThenElif.WidgetKey,
                AttributesBundle(
                    StackList.one(IfThenElif.Branches.WithValue(branches |> List.map Gen.mkOak)),
                    Array.empty,
                    Array.empty
                )
            )

        static member IfThenElifExpr(branches: WidgetBuilder<Constant> list) =
            Ast.IfThenElifExpr(branches |> List.map Ast.ConstantExpr)

        static member IfThenElifExpr(branches: string list) =
            Ast.IfThenElifExpr(branches |> List.map Ast.Constant)
