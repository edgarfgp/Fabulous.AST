namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
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
                ExprMatchNode(
                    SingleTextNode("match", Range.Zero),
                    matchExpr,
                    SingleTextNode("with", Range.Zero),
                    matchClauses,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module MatchBuilders =
    type Fabulous.AST.Ast with

        static member inline Match(matchExpr: WidgetBuilder<Expr>) =
            CollectionBuilder<Expr, MatchClauseNode>(
                Match.WidgetKey,
                Match.MatchClauses,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| Match.MatchExpr.WithValue(matchExpr.Compile()) |],
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
