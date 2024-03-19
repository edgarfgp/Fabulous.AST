namespace Fabulous.AST

open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module MatchClause =
    let PatternNamed = Attributes.defineScalar<StringOrWidget<Pattern>> "PatternNamed"
    let WhenExpr = Attributes.defineScalar<StringOrWidget<Expr>> "WhenExpr"
    let BodyExpr = Attributes.defineScalar<StringOrWidget<Expr>> "BodyExpr"

    let WidgetKey =
        Widgets.register "MatchClause" (fun widget ->
            let pattern = Widgets.getScalarValue widget PatternNamed

            let pattern =
                match pattern with
                | StringOrWidget.StringExpr name ->
                    let name = StringParsing.normalizeIdentifierQuotes name
                    Pattern.Named(PatNamedNode(None, SingleTextNode.Create name, Range.Zero))
                | StringOrWidget.WidgetExpr pattern -> pattern

            let whenExpr = Widgets.tryGetScalarValue widget WhenExpr
            let bodyExpr = Widgets.getScalarValue widget BodyExpr

            let bodyExpr =
                match bodyExpr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value)))
                    )
                | StringOrWidget.WidgetExpr expr -> expr

            let whenExpr =
                match whenExpr with
                | ValueNone -> None
                | ValueSome whenExpr ->
                    match whenExpr with
                    | StringOrWidget.StringExpr value ->
                        Expr.Constant(
                            Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value)))
                        )
                        |> Some
                    | StringOrWidget.WidgetExpr expr -> expr |> Some

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
                    StackList.three(
                        MatchClause.PatternNamed.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak pattern)),
                        MatchClause.BodyExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak bodyExpr)),
                        MatchClause.WhenExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak whenExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member MatchClauseExpr
            (pattern: StringVariant, whenExpr: WidgetBuilder<Expr>, bodyExpr: WidgetBuilder<Expr>)
            =
            WidgetBuilder<MatchClauseNode>(
                MatchClause.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        MatchClause.PatternNamed.WithValue(StringOrWidget.StringExpr(pattern)),
                        MatchClause.BodyExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak bodyExpr)),
                        MatchClause.WhenExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak whenExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member MatchClauseExpr(pattern: StringVariant, whenExpr: StringVariant, bodyExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<MatchClauseNode>(
                MatchClause.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        MatchClause.PatternNamed.WithValue(StringOrWidget.StringExpr(pattern)),
                        MatchClause.BodyExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak bodyExpr)),
                        MatchClause.WhenExpr.WithValue(StringOrWidget.StringExpr(whenExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member MatchClauseExpr(pattern: StringVariant, whenExpr: StringVariant, bodyExpr: StringVariant) =
            WidgetBuilder<MatchClauseNode>(
                MatchClause.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        MatchClause.PatternNamed.WithValue(StringOrWidget.StringExpr(pattern)),
                        MatchClause.BodyExpr.WithValue(StringOrWidget.StringExpr(bodyExpr)),
                        MatchClause.WhenExpr.WithValue(StringOrWidget.StringExpr(whenExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member MatchClauseExpr(pattern: WidgetBuilder<Pattern>, bodyExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<MatchClauseNode>(
                MatchClause.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        MatchClause.PatternNamed.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak pattern)),
                        MatchClause.BodyExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak bodyExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member MatchClauseExpr(pattern: StringVariant, bodyExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<MatchClauseNode>(
                MatchClause.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        MatchClause.PatternNamed.WithValue(StringOrWidget.StringExpr(pattern)),
                        MatchClause.BodyExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak bodyExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member MatchClauseExpr(pattern: WidgetBuilder<Pattern>, bodyExpr: StringVariant) =
            WidgetBuilder<MatchClauseNode>(
                MatchClause.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        MatchClause.PatternNamed.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak pattern)),
                        MatchClause.BodyExpr.WithValue(StringOrWidget.StringExpr(bodyExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member MatchClauseExpr(pattern: StringVariant, bodyExpr: StringVariant) =
            WidgetBuilder<MatchClauseNode>(
                MatchClause.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        MatchClause.PatternNamed.WithValue(StringOrWidget.StringExpr(pattern)),
                        MatchClause.BodyExpr.WithValue(StringOrWidget.StringExpr(bodyExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )
