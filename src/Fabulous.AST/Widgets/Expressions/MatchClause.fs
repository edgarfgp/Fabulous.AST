namespace Fabulous.AST

open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module MatchClause =
    let PatternNamed = Attributes.defineWidget "PatternNamed"
    let WhenExpr = Attributes.defineWidget "WhenExpr"
    let BodyExpr = Attributes.defineWidget "BodyExpr"

    let WidgetKey =
        Widgets.register "MatchClause" (fun widget ->
            let pattern = Widgets.getNodeFromWidget widget PatternNamed
            let whenExpr = Widgets.tryGetNodeFromWidget widget WhenExpr
            let bodyExpr = Widgets.getNodeFromWidget widget BodyExpr

            let whenExpr =
                match whenExpr with
                | ValueNone -> None
                | ValueSome whenExpr -> Some whenExpr

            MatchClauseNode(
                Some(SingleTextNode.bar),
                pattern,
                whenExpr,
                SingleTextNode.rightArrow,
                bodyExpr,
                Range.Zero
            ))

[<AutoOpen>]
module MatchClauseBuilders =
    type Ast with

        static member MatchClauseExpr
            (pattern: WidgetBuilder<Pattern>, whenExpr: WidgetBuilder<Expr>, bodyExpr: WidgetBuilder<Expr>)
            =
            WidgetBuilder<MatchClauseNode>(
                MatchClause.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| MatchClause.PatternNamed.WithValue(pattern.Compile())
                       MatchClause.BodyExpr.WithValue(bodyExpr.Compile())
                       MatchClause.WhenExpr.WithValue(whenExpr.Compile()) |],
                    Array.empty
                )
            )

        static member MatchClauseExpr(pattern: WidgetBuilder<Pattern>, bodyExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<MatchClauseNode>(
                MatchClause.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| MatchClause.PatternNamed.WithValue(pattern.Compile())
                       MatchClause.BodyExpr.WithValue(bodyExpr.Compile()) |],
                    Array.empty
                )
            )
