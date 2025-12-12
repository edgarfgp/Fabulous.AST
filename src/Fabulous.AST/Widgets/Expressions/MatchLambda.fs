namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module MatchLambda =
    let Clauses = Attributes.defineScalar<MatchClauseNode seq> "Clauses"

    let WidgetKey =
        Widgets.register "MatchLambda" (fun widget ->
            let clauses = Widgets.getScalarValue widget Clauses |> List.ofSeq

            Expr.MatchLambda(ExprMatchLambdaNode(SingleTextNode.``function``, clauses, Range.Zero)))

[<AutoOpen>]
module MatchLambdaBuilders =
    type Ast with

        static member MatchLambdaExpr(clauses: WidgetBuilder<MatchClauseNode> seq) =
            let clauses = clauses |> Seq.map Gen.mkOak

            WidgetBuilder<Expr>(MatchLambda.WidgetKey, MatchLambda.Clauses.WithValue(clauses))

        static member MatchLambdaExpr(clause: WidgetBuilder<MatchClauseNode>) = Ast.MatchLambdaExpr([ clause ])
