namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fantomas.FCS.Text
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module Match =
    let MatchExpr = Attributes.defineWidget "MatchExpr"
    let MatchClauses = Attributes.defineScalar<MatchClauseNode seq> "MatchClauses"

    let WidgetKey =
        Widgets.register "Match" (fun widget ->
            let expr = Widgets.getNodeFromWidget widget MatchExpr
            let matchClauses = Widgets.getScalarValue widget MatchClauses |> List.ofSeq

            Expr.Match(
                ExprMatchNode(SingleTextNode.``match``, expr, SingleTextNode.``with``, matchClauses, Range.Zero)
            ))

[<AutoOpen>]
module MatchBuilders =
    type Ast with

        static member MatchExpr(value: WidgetBuilder<Expr>, clauses: WidgetBuilder<MatchClauseNode> seq) =
            WidgetBuilder<Expr>(
                Match.WidgetKey,
                AttributesBundle(
                    StackList.one(Match.MatchClauses.WithValue(clauses |> Seq.map Gen.mkOak)),
                    [| Match.MatchExpr.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member MatchExpr(value: WidgetBuilder<Constant>, clauses: WidgetBuilder<MatchClauseNode> seq) =
            Ast.MatchExpr(Ast.ConstantExpr(value), clauses)

        static member MatchExpr(value: string, clauses: WidgetBuilder<MatchClauseNode> seq) =
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
    static member inline YieldFrom
        (_: CollectionBuilder<Expr, MatchClauseNode>, x: WidgetBuilder<MatchClauseNode> seq)
        : CollectionContent =
        let widgets =
            x |> Seq.map(fun wb -> wb.Compile()) |> Seq.toArray |> MutStackArray1.fromArray

        { Widgets = widgets }
