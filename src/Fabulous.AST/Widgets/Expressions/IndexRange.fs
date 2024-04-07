namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module IndexRange =
    let FromExpr = Attributes.defineScalar<StringOrWidget<Expr>> "FromExpr"

    let ToExpr = Attributes.defineScalar<StringOrWidget<Expr>> "ToExpr"

    let WidgetKey =
        Widgets.register "IndexRange" (fun widget ->
            let fromExpr = Widgets.tryGetScalarValue widget FromExpr

            let fromExpr =
                match fromExpr with
                | ValueSome(StringOrWidget.StringExpr value) ->
                    Expr.Constant(
                        Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value)))
                    )
                    |> Some
                | ValueSome(StringOrWidget.WidgetExpr expr) -> Some expr
                | ValueNone -> None

            let toExpr = Widgets.tryGetScalarValue widget ToExpr

            let toExpr =
                match toExpr with
                | ValueSome(StringOrWidget.StringExpr value) ->
                    Expr.Constant(
                        Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value)))
                    )
                    |> Some
                | ValueSome(StringOrWidget.WidgetExpr expr) -> Some expr
                | ValueNone -> None

            Expr.IndexRange(ExprIndexRangeNode(fromExpr, SingleTextNode.Create(".."), toExpr, Range.Zero)))

[<AutoOpen>]
module IndexRangeBuilders =
    type Ast with

        static member IndexRangeExpr() =
            WidgetBuilder<Expr>(IndexRange.WidgetKey, AttributesBundle(StackList.empty(), Array.empty, Array.empty))

        static member IndexFromRangeExpr(fromExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                IndexRange.WidgetKey,
                AttributesBundle(
                    StackList.one(IndexRange.FromExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak(fromExpr)))),
                    Array.empty,
                    Array.empty
                )
            )

        static member IndexFromRangeExpr(fromExpr: string) =
            WidgetBuilder<Expr>(
                IndexRange.WidgetKey,
                AttributesBundle(
                    StackList.one(IndexRange.FromExpr.WithValue(StringOrWidget.StringExpr(Unquoted(fromExpr)))),
                    Array.empty,
                    Array.empty
                )
            )

        static member IndexToRangeExpr(toExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                IndexRange.WidgetKey,
                AttributesBundle(
                    StackList.one(IndexRange.ToExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak(toExpr)))),
                    Array.empty,
                    Array.empty
                )
            )

        static member IndexToRangeExpr(toExpr: string) =
            WidgetBuilder<Expr>(
                IndexRange.WidgetKey,
                AttributesBundle(
                    StackList.one(IndexRange.ToExpr.WithValue(StringOrWidget.StringExpr(Unquoted(toExpr)))),
                    Array.empty,
                    Array.empty
                )
            )

        static member IndexRangeExpr(from: WidgetBuilder<Expr>, toExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                IndexRange.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        IndexRange.FromExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak(from))),
                        IndexRange.ToExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak(toExpr)))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member IndexRangeExpr(from: WidgetBuilder<Expr>, toExpr: string) =
            WidgetBuilder<Expr>(
                IndexRange.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        IndexRange.FromExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak(from))),
                        IndexRange.ToExpr.WithValue(StringOrWidget.StringExpr(Unquoted toExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member IndexRangeExpr(from: string, toExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                IndexRange.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        IndexRange.FromExpr.WithValue(StringOrWidget.StringExpr(Unquoted from)),
                        IndexRange.ToExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak toExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member IndexRangeExpr(fromExpr: string, toExpr: string) =
            WidgetBuilder<Expr>(
                IndexRange.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        IndexRange.FromExpr.WithValue(StringOrWidget.StringExpr(Unquoted(fromExpr))),
                        IndexRange.ToExpr.WithValue(StringOrWidget.StringExpr(Unquoted(toExpr)))
                    ),
                    Array.empty,
                    Array.empty
                )
            )
