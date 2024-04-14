namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module IndexWithoutDot =
    let IdentifierExpr = Attributes.defineScalar<StringOrWidget<Expr>> "Identifier"

    let IndexExpr = Attributes.defineScalar<StringOrWidget<Expr>> "IndexExpr"

    let WidgetKey =
        Widgets.register "IndexWithoutDotExpr" (fun widget ->
            let identifierExpr = Widgets.getScalarValue widget IdentifierExpr
            let indexExpr = Widgets.getScalarValue widget IndexExpr

            let identifierExpr =
                match identifierExpr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value)))
                    )
                | StringOrWidget.WidgetExpr expr -> expr

            let indexExpr =
                match indexExpr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value)))
                    )
                | StringOrWidget.WidgetExpr expr -> expr

            Expr.IndexWithoutDot(ExprIndexWithoutDotNode(identifierExpr, indexExpr, Range.Zero)))

[<AutoOpen>]
module IndexWithoutDotBuilders =
    type Ast with

        static member IndexWithoutDotExpr(identifier: WidgetBuilder<Expr>, indexer: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                IndexWithoutDot.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        IndexWithoutDot.IdentifierExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak identifier)),
                        IndexWithoutDot.IndexExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak indexer))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member IndexWithoutDotExpr(identifier: StringVariant, indexer: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                IndexWithoutDot.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        IndexWithoutDot.IdentifierExpr.WithValue(StringOrWidget.StringExpr(identifier)),
                        IndexWithoutDot.IndexExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak indexer))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member IndexWithoutDotExpr(identifier: WidgetBuilder<Expr>, indexer: StringVariant) =
            WidgetBuilder<Expr>(
                IndexWithoutDot.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        IndexWithoutDot.IdentifierExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak identifier)),
                        IndexWithoutDot.IndexExpr.WithValue(StringOrWidget.StringExpr(indexer))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member IndexWithoutDotExpr(identifier: StringVariant, indexer: StringVariant) =
            WidgetBuilder<Expr>(
                IndexWithoutDot.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        IndexWithoutDot.IdentifierExpr.WithValue(StringOrWidget.StringExpr(identifier)),
                        IndexWithoutDot.IndexExpr.WithValue(StringOrWidget.StringExpr(indexer))
                    ),
                    Array.empty,
                    Array.empty
                )
            )
