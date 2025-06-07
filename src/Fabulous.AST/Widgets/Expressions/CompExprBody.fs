namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module CompExprBody =
    let Statements = Attributes.defineScalar<ComputationExpressionStatement seq> "Value"

    let WidgetKey =
        Widgets.register "CompExprBody" (fun widget ->
            let statements = Widgets.getScalarValue widget Statements |> List.ofSeq

            Expr.CompExprBody(ExprCompExprBodyNode(statements, Range.Zero)))

[<AutoOpen>]
module CompExprBodyBuilders =
    type Ast with

        static member CompExprBodyExpr(values: WidgetBuilder<ComputationExpressionStatement> seq) =
            let statements = values |> Seq.map Gen.mkOak

            WidgetBuilder<Expr>(CompExprBody.WidgetKey, CompExprBody.Statements.WithValue(statements))

        static member CompExprBodyExpr(value: WidgetBuilder<ComputationExpressionStatement>) =
            Ast.CompExprBodyExpr([ value ])

        static member CompExprBodyExpr(values: WidgetBuilder<Expr> seq) =
            let values = values |> Seq.map(Ast.OtherExpr)
            Ast.CompExprBodyExpr(values)

        static member CompExprBodyExpr(values: WidgetBuilder<Constant> seq) =
            let values = values |> Seq.map(Ast.ConstantExpr)
            Ast.CompExprBodyExpr(values)

        static member CompExprBodyExpr(value: WidgetBuilder<Expr>) = Ast.CompExprBodyExpr([ value ])

        static member CompExprBodyExpr(values: string seq) =
            let values = values |> Seq.map(Ast.OtherExpr)
            Ast.CompExprBodyExpr(values)

        static member CompExprBodyExpr(value: string) = Ast.CompExprBodyExpr([ value ])

        static member CompExprBodyExpr(values: WidgetBuilder<BindingNode> seq) =
            let values = values |> Seq.map(Ast.LetOrUseExpr)
            Ast.CompExprBodyExpr(values)

        static member CompExprBodyExpr(value: WidgetBuilder<BindingNode>) = Ast.CompExprBodyExpr([ value ])
