namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module IfThenElif =
    let Branches = Attributes.defineScalar<Expr seq> "ElifExpr"

    let ElseExpr = Attributes.defineWidget "ElseExpr"

    let WidgetKey =
        Widgets.register "IfThenElif" (fun widget ->
            let branches =
                Widgets.getScalarValue widget Branches
                |> Seq.choose(fun x ->
                    match Expr.Node(x) with
                    | :? ExprIfThenNode as node -> Some node
                    | _ -> None)
                |> List.ofSeq

            let elseExpr = Widgets.tryGetNodeFromWidget widget ElseExpr

            let elseExpr =
                match elseExpr with
                | ValueNone -> None
                | ValueSome expr -> Some(SingleTextNode.``else``, expr)

            Expr.IfThenElif(ExprIfThenElifNode(branches, elseExpr, Range.Zero)))

[<AutoOpen>]
module IfThenElifBuilders =
    type Ast with
        static member IfThenElifExpr(branches: WidgetBuilder<Expr> seq, elseExpr: WidgetBuilder<Expr>) =
            let branches = branches |> Seq.map Gen.mkOak

            WidgetBuilder<Expr>(
                IfThenElif.WidgetKey,
                AttributesBundle(
                    StackList.one(IfThenElif.Branches.WithValue(branches)),
                    [| IfThenElif.ElseExpr.WithValue(elseExpr.Compile()) |],
                    Array.empty
                )
            )

        static member IfThenElifExpr(branches: WidgetBuilder<Constant> seq, elseExpr: WidgetBuilder<Expr>) =
            let branches = branches |> Seq.map Ast.ConstantExpr
            Ast.IfThenElifExpr(branches, elseExpr)

        static member IfThenElifExpr(branches: string seq, elseExpr: WidgetBuilder<Expr>) =
            let branches = branches |> Seq.map Ast.Constant
            Ast.IfThenElifExpr(branches, elseExpr)

        static member IfThenElifExpr(branches: WidgetBuilder<Expr> seq, elseExpr: WidgetBuilder<Constant>) =
            Ast.IfThenElifExpr(branches, Ast.ConstantExpr(elseExpr))

        static member IfThenElifExpr(branches: WidgetBuilder<Expr> seq, elseExpr: string) =
            Ast.IfThenElifExpr(branches, Ast.Constant(elseExpr))

        static member IfThenElifExpr(branches: WidgetBuilder<Constant> seq, elseExpr: WidgetBuilder<Constant>) =
            Ast.IfThenElifExpr(branches |> Seq.map Ast.ConstantExpr, Ast.ConstantExpr(elseExpr))

        static member IfThenElifExpr(branches: WidgetBuilder<Constant> seq, elseExpr: string) =
            Ast.IfThenElifExpr(branches |> Seq.map Ast.ConstantExpr, Ast.Constant(elseExpr))

        static member IfThenElifExpr(branches: string seq, elseExpr: WidgetBuilder<Constant>) =
            Ast.IfThenElifExpr(branches |> Seq.map Ast.Constant, Ast.ConstantExpr(elseExpr))

        static member IfThenElifExpr(branches: string seq, elseExpr: string) =
            Ast.IfThenElifExpr(branches, Ast.Constant(elseExpr))

        static member IfThenElifExpr(branches: WidgetBuilder<Expr> seq) =
            WidgetBuilder<Expr>(IfThenElif.WidgetKey, IfThenElif.Branches.WithValue(branches |> Seq.map Gen.mkOak))

        static member IfThenElifExpr(branches: WidgetBuilder<Constant> seq) =
            Ast.IfThenElifExpr(branches |> Seq.map Ast.ConstantExpr)

        static member IfThenElifExpr(branches: string seq) =
            Ast.IfThenElifExpr(branches |> Seq.map Ast.Constant)
