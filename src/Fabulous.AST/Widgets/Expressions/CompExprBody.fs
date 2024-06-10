namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
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

        static member CompExprBodyExpr(value: WidgetBuilder<ComputationExpressionStatement> list) =
            let statements = value |> List.map Gen.mkOak

            WidgetBuilder<Expr>(
                CompExprBody.WidgetKey,
                AttributesBundle(StackList.one(CompExprBody.Statements.WithValue(statements)), Array.empty, Array.empty)
            )

        static member CompExprBodyExpr(values: WidgetBuilder<Expr> list) =
            let values = values |> List.map(fun e -> Ast.OtherExpr(e))
            Ast.CompExprBodyExpr(values)

        static member CompExprBodyExpr(values: string list) =
            let values = values |> List.map(fun e -> Ast.OtherExpr(e))
            Ast.CompExprBodyExpr(values)

        static member CompExprBodyExpr(values: WidgetBuilder<BindingNode> list) =
            let values = values |> List.map(fun e -> Ast.LetOrUseExpr(e))
            Ast.CompExprBodyExpr(values)
