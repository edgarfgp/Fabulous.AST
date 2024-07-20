namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.Builders.StackAllocatedCollections
open Fantomas.FCS.Text
open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module Match =
    let MatchExpr = Attributes.defineWidget "MatchExpr"
    let MatchClauses = Attributes.defineScalar<MatchClauseNode list> "MatchClauses"

    let WidgetKey =
        Widgets.register "Match" (fun widget ->
            let expr = Widgets.getNodeFromWidget widget MatchExpr
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
                    StackList.one(Match.MatchClauses.WithValue(clauses |> List.map Gen.mkOak)),
                    [| Match.MatchExpr.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member MatchExpr(value: WidgetBuilder<Constant>, clauses: WidgetBuilder<MatchClauseNode> list) =
            Ast.MatchExpr(Ast.ConstantExpr(value), clauses)

        static member MatchExpr(value: string, clauses: WidgetBuilder<MatchClauseNode> list) =
            Ast.MatchExpr(Ast.Constant(value), clauses)

        static member MatchExpr(value: WidgetBuilder<Expr>, clause: WidgetBuilder<MatchClauseNode>) =
            Ast.MatchExpr(value, [ clause ])

        static member MatchExpr(value: WidgetBuilder<Constant>, clause: WidgetBuilder<MatchClauseNode>) =
            Ast.MatchExpr(Ast.ConstantExpr(value), clause)

        static member MatchExpr(value: string, clause: WidgetBuilder<MatchClauseNode>) =
            Ast.MatchExpr(Ast.Constant(value), clause)

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
