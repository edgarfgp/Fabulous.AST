namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module TryWith =
    let Value = Attributes.defineWidget "Value"

    let Clauses = Attributes.defineScalar<MatchClauseNode list> "Clauses"

    let WidgetKey =
        Widgets.register "TryWith" (fun widget ->
            let expr = Widgets.getNodeFromWidget widget Value
            let clauses = Widgets.getScalarValue widget Clauses
            Expr.TryWith(ExprTryWithNode(SingleTextNode.``try``, expr, SingleTextNode.``with``, clauses, Range.Zero)))

[<AutoOpen>]
module TryWithBuilders =
    type Ast with

        static member TryWithExpr(value: WidgetBuilder<Expr>, clauses: WidgetBuilder<MatchClauseNode> list) =
            let clauses = clauses |> List.map Gen.mkOak

            WidgetBuilder<Expr>(
                TryWith.WidgetKey,
                AttributesBundle(
                    StackList.one(TryWith.Clauses.WithValue(clauses)),
                    [| TryWith.Value.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member TryWithExpr(value: WidgetBuilder<Constant>, clauses: WidgetBuilder<MatchClauseNode> list) =
            Ast.TryWithExpr(Ast.ConstantExpr(value), clauses)

        static member TryWithExpr(value: string, clauses: WidgetBuilder<MatchClauseNode> list) =
            Ast.TryWithExpr(Ast.ConstantExpr(value), clauses)
