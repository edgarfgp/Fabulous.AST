namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module Match =
    let MatchExpr = Attributes.defineWidget "MatchExpr"
    let MatchClauses = Attributes.defineWidgetCollection "MatchClauses"

    let WidgetKey =
        Widgets.register "Match" (fun widget ->
            let matchExpr = Helpers.getNodeFromWidget<Expr> widget MatchExpr

            let matchClauses =
                Helpers.getNodesFromWidgetCollection<MatchClauseNode> widget MatchClauses

            Expr.Match(
                ExprMatchNode(SingleTextNode.``match``, matchExpr, SingleTextNode.``with``, matchClauses, Range.Zero)
            ))

[<AutoOpen>]
module MatchBuilders =
    type Ast with

        static member inline MatchExpr(matchExpr: WidgetBuilder<Expr>) =
            CollectionBuilder<Expr, MatchClauseNode>(
                Match.WidgetKey,
                Match.MatchClauses,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| Match.MatchExpr.WithValue(matchExpr.Compile()) |],
                    ValueNone
                )
            )

        static member inline MatchExpr(matchExpr: string, ?hasQuotes: bool) =
            match hasQuotes with
            | None
            | Some true ->
                CollectionBuilder<Expr, MatchClauseNode>(
                    Match.WidgetKey,
                    Match.MatchClauses,
                    AttributesBundle(
                        StackList.empty(),
                        ValueSome [| Match.MatchExpr.WithValue(Ast.ConstantExpr(matchExpr, true).Compile()) |],
                        ValueNone
                    )
                )
            | _ ->
                CollectionBuilder<Expr, MatchClauseNode>(
                    Match.WidgetKey,
                    Match.MatchClauses,
                    AttributesBundle(
                        StackList.empty(),
                        ValueSome [| Match.MatchExpr.WithValue(Ast.ConstantExpr(matchExpr, false).Compile()) |],
                        ValueNone
                    )
                )

[<Extension>]
type MatchYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<Expr, MatchClauseNode>,
            x: WidgetBuilder<MatchClauseNode>
        ) : CollectionContent =
        { Widgets = MutStackArray1.One(x.Compile()) }
