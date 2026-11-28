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

    let private matchNode (keyword: SingleTextNode) (widget: Widget) =
        let expr = Widgets.getNodeFromWidget widget MatchExpr
        let matchClauses = Widgets.getScalarValue widget MatchClauses |> List.ofSeq
        Expr.Match(ExprMatchNode(keyword, expr, SingleTextNode.``with``, matchClauses, Range.range0))

    let WidgetKey = Widgets.register "Match" (matchNode SingleTextNode.``match``)

    let BangWidgetKey =
        Widgets.register "MatchBang" (matchNode SingleTextNode.matchBang)

[<AutoOpen>]
module MatchBuilders =
    type Ast with

        static member MatchExpr(value: WidgetBuilder<Expr>, clauses: WidgetBuilder<MatchClauseNode> seq) =
            WidgetBuilder<Expr>(
                Match.WidgetKey,
                AttributesBundle(
                    StackList.one(Match.MatchClauses.WithValue(clauses |> Seq.map Gen.mkOak |> Seq.toArray)),
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

        /// <summary>
        /// Create a <c>match!</c> expression, which awaits a computation inside a computation expression
        /// and matches on its result.
        /// </summary>
        /// <param name="value">The computation whose result is matched.</param>
        /// <param name="clauses">The match clauses.</param>
        /// <code language="fsharp">
        /// NamedComputationExpr(
        ///     "async",
        ///     MatchBangExpr("fetch ()", [ MatchClauseExpr(LongIdentPat("Some", "x"), "x"); MatchClauseExpr(NamedPat("None"), Int(0)) ])
        /// )
        /// </code>
        static member MatchBangExpr(value: WidgetBuilder<Expr>, clauses: WidgetBuilder<MatchClauseNode> seq) =
            WidgetBuilder<Expr>(
                Match.BangWidgetKey,
                AttributesBundle(
                    StackList.one(Match.MatchClauses.WithValue(clauses |> Seq.map Gen.mkOak |> Seq.toArray)),
                    [| Match.MatchExpr.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member MatchBangExpr(value: WidgetBuilder<Constant>, clauses: WidgetBuilder<MatchClauseNode> seq) =
            Ast.MatchBangExpr(Ast.ConstantExpr(value), clauses)

        static member MatchBangExpr(value: string, clauses: WidgetBuilder<MatchClauseNode> seq) =
            Ast.MatchBangExpr(Ast.Constant(value), clauses)

        static member MatchBangExpr(value: WidgetBuilder<Expr>, clause: WidgetBuilder<MatchClauseNode>) =
            Ast.MatchBangExpr(value, [ clause ])

        static member MatchBangExpr(value: WidgetBuilder<Constant>, clause: WidgetBuilder<MatchClauseNode>) =
            Ast.MatchBangExpr(Ast.ConstantExpr(value), clause)

        static member MatchBangExpr(value: string, clause: WidgetBuilder<MatchClauseNode>) =
            Ast.MatchBangExpr(Ast.Constant(value), clause)

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
