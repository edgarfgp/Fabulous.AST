namespace Fabulous.AST

open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module MatchClause =
    let PatternNamed = Attributes.defineScalar "PatternNamed"
    let WhenExpr = Attributes.defineWidget "WhenExpr"
    let BodyExpr = Attributes.defineWidget "BodyExpr"

    let WidgetKey =
        Widgets.register "MatchClause" (fun widget ->
            let patternNamed = Helpers.getScalarValue widget PatternNamed
            let whenExpr = Helpers.tryGetNodeFromWidget<Expr> widget WhenExpr
            let bodyExpr = Helpers.getNodeFromWidget<Expr> widget BodyExpr

            let whenExpr =
                match whenExpr with
                | ValueNone -> None
                | ValueSome value -> Some value

            MatchClauseNode(
                Some(SingleTextNode("|", Range.Zero)),
                Pattern.Named(PatNamedNode(None, SingleTextNode(patternNamed, Range.Zero), Range.Zero)),
                whenExpr,
                SingleTextNode("->", Range.Zero),
                bodyExpr,
                Range.Zero
            ))

[<AutoOpen>]
module MatchClauseBuilders =
    type Fabulous.AST.Ast with

        static member inline MatchClause
            (
                patternNamed: string,
                whenExpr: WidgetBuilder<Expr>,
                bodyExpr: WidgetBuilder<Expr>
            ) =
            WidgetBuilder<MatchClauseNode>(
                MatchClause.WidgetKey,
                AttributesBundle(
                    StackList.one(MatchClause.PatternNamed.WithValue(patternNamed)),
                    ValueSome
                        [| MatchClause.WhenExpr.WithValue(whenExpr.Compile())
                           MatchClause.BodyExpr.WithValue(bodyExpr.Compile()) |],
                    ValueNone
                )
            )

        static member inline MatchClause(patternNamed: string, bodyExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<MatchClauseNode>(
                MatchClause.WidgetKey,
                AttributesBundle(
                    StackList.one(MatchClause.PatternNamed.WithValue(patternNamed)),
                    ValueSome [| MatchClause.BodyExpr.WithValue(bodyExpr.Compile()) |],
                    ValueNone
                )
            )
