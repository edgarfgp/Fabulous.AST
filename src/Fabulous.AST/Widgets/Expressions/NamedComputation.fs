namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module NamedComputation =
    let NameExpr = Attributes.defineWidget "NamedExpr"
    let BodyExpr = Attributes.defineWidget "BodyExpr"

    let WidgetKey =
        Widgets.register "NamedComputation" (fun widget ->
            let bodyExpr = Widgets.getNodeFromWidget<Expr> widget BodyExpr
            let nameExpr = Widgets.getNodeFromWidget<Expr> widget NameExpr

            Expr.NamedComputation(
                ExprNamedComputationNode(
                    nameExpr,
                    SingleTextNode.leftCurlyBrace,
                    bodyExpr,
                    SingleTextNode.rightCurlyBrace,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module NamedComputationBuilders =
    type Ast with

        static member NamedComputationExpr(name: WidgetBuilder<Expr>, body: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                NamedComputation.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| NamedComputation.NameExpr.WithValue(name.Compile())
                       NamedComputation.BodyExpr.WithValue(body.Compile()) |],
                    Array.empty
                )
            )

        static member NamedComputationExpr(name: WidgetBuilder<Constant>, body: WidgetBuilder<Expr>) =
            Ast.NamedComputationExpr(Ast.ConstantExpr(name), body)

        static member NamedComputationExpr(name: string, body: WidgetBuilder<Expr>) =
            Ast.NamedComputationExpr(Ast.ConstantExpr(name), body)

        static member NamedComputationExpr(name: string, body: WidgetBuilder<Constant>) =
            Ast.NamedComputationExpr(name, Ast.ConstantExpr(body))

        static member NamedComputationExpr(name: WidgetBuilder<Expr>, body: WidgetBuilder<Constant>) =
            Ast.NamedComputationExpr(name, Ast.ConstantExpr(body))

        static member NamedComputationExpr(name: WidgetBuilder<Constant>, body: WidgetBuilder<Constant>) =
            Ast.NamedComputationExpr(Ast.ConstantExpr(name), Ast.ConstantExpr(body))

        static member NamedComputationExpr(name: string, body: string) =
            Ast.NamedComputationExpr(Ast.Constant(name), Ast.Constant(body))

        static member SeqExpr(body: WidgetBuilder<Expr>) = Ast.NamedComputationExpr("seq", body)

        static member SeqExpr(body: WidgetBuilder<Expr> list) =
            Ast.NamedComputationExpr("seq", Ast.CompExprBodyExpr(body))

        static member SeqExpr(body: WidgetBuilder<Constant> list) =
            Ast.NamedComputationExpr("seq", Ast.CompExprBodyExpr(body))

        static member SeqExpr(body: string list) =
            Ast.NamedComputationExpr("seq", Ast.CompExprBodyExpr(body))
