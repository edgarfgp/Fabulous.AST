namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module CompExprBody =
    let Statements =
        Attributes.defineScalar<ComputationExpressionStatement list> "Value"

    let WidgetKey =
        Widgets.register "CompExprBody" (fun widget ->
            let statements = Widgets.getScalarValue widget Statements

            Expr.CompExprBody(ExprCompExprBodyNode(statements, Range.Zero)))

[<AutoOpen>]
module CompExprBodyBuilders =
    type Ast with

        static member CompExprBodyExpr(values: WidgetBuilder<ComputationExpressionStatement> list) =
            let statements = values |> List.map Gen.mkOak

            WidgetBuilder<Expr>(CompExprBody.WidgetKey, CompExprBody.Statements.WithValue(statements))

        static member CompExprBodyExpr(value: WidgetBuilder<ComputationExpressionStatement>) =
            Ast.CompExprBodyExpr([ value ])

        static member CompExprBodyExpr(values: WidgetBuilder<Expr> list) =
            let values = values |> List.map(Ast.OtherExpr)
            Ast.CompExprBodyExpr(values)

        static member CompExprBodyExpr(values: WidgetBuilder<Constant> list) =
            let values = values |> List.map(Ast.ConstantExpr)
            Ast.CompExprBodyExpr(values)

        static member CompExprBodyExpr(value: WidgetBuilder<Expr>) = Ast.CompExprBodyExpr([ value ])

        static member CompExprBodyExpr(values: string list) =
            let values = values |> List.map(Ast.OtherExpr)
            Ast.CompExprBodyExpr(values)

        static member CompExprBodyExpr(value: string) = Ast.CompExprBodyExpr([ value ])

        static member CompExprBodyExpr(values: WidgetBuilder<BindingNode> list) =
            let values = values |> List.map(Ast.LetOrUseExpr)
            Ast.CompExprBodyExpr(values)

        static member CompExprBodyExpr(value: WidgetBuilder<BindingNode>) = Ast.CompExprBodyExpr([ value ])
