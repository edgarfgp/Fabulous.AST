namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module MatchLambda =
    let Clauses = Attributes.defineScalar<MatchClauseNode list> "Clauses"

    let WidgetKey =
        Widgets.register "MatchLambda" (fun widget ->
            let clauses = Widgets.getScalarValue widget Clauses

            Expr.MatchLambda(ExprMatchLambdaNode(SingleTextNode.``function``, clauses, Range.Zero)))

[<AutoOpen>]
module MatchLambdaBuilders =
    type Ast with

        static member MatchLambdaExpr(clauses: WidgetBuilder<MatchClauseNode> list) =
            let clauses = clauses |> List.map Gen.mkOak

            WidgetBuilder<Expr>(
                MatchLambda.WidgetKey,
                AttributesBundle(StackList.one(MatchLambda.Clauses.WithValue(clauses)), Array.empty, Array.empty)
            )
