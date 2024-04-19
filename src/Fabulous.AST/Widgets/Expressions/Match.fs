namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module Match =
    let MatchExpr = Attributes.defineScalar<StringOrWidget<Expr>> "MatchExpr"
    let MatchClauses = Attributes.defineScalar<MatchClauseNode list> "MatchClauses"

    let WidgetKey =
        Widgets.register "Match" (fun widget ->
            let expr = Widgets.getScalarValue widget MatchExpr

            let expr =
                match expr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value)))
                    )
                | StringOrWidget.WidgetExpr expr -> expr

            let matchClauses = Widgets.getScalarValue widget MatchClauses

            Expr.Match(
                ExprMatchNode(SingleTextNode.``match``, expr, SingleTextNode.``with``, matchClauses, Range.Zero)
            ))

[<AutoOpen>]
module MatchBuilders =
    type Ast with

        static member MatchExpr(value: WidgetBuilder<Expr>, clauses: WidgetBuilder<MatchClauseNode> list) =
            WidgetBuilder<Expr>(
                Match.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        Match.MatchExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak(value))),
                        Match.MatchClauses.WithValue(clauses |> List.map Gen.mkOak)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member MatchExpr(matchExpr: StringVariant, clauses: WidgetBuilder<MatchClauseNode> list) =
            WidgetBuilder<Expr>(
                Match.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        Match.MatchExpr.WithValue(StringOrWidget.StringExpr(matchExpr)),
                        Match.MatchClauses.WithValue(clauses |> List.map Gen.mkOak)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

type MatchYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<Expr, MatchClauseNode>, x: WidgetBuilder<MatchClauseNode>)
        : CollectionContent =
        { Widgets = MutStackArray1.One(x.Compile()) }

    [<Extension>]
    static member inline Yield(_: CollectionBuilder<Expr, MatchClauseNode>, x: MatchClauseNode) : CollectionContent =
        let widget = Ast.EscapeHatch(x).Compile()
        { Widgets = MutStackArray1.One(widget) }
