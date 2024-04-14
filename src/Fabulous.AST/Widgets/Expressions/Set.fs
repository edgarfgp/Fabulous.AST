namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Set =
    let IdentifierExpr = Attributes.defineScalar<StringOrWidget<Expr>> "Identifier"

    let SetExpr = Attributes.defineScalar<StringOrWidget<Expr>> "SetExpr"

    let WidgetKey =
        Widgets.register "Lazy" (fun widget ->
            let identifierExpr = Widgets.getScalarValue widget IdentifierExpr
            let setExpr = Widgets.getScalarValue widget SetExpr

            let identifierExpr =
                match identifierExpr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value)))
                    )
                | StringOrWidget.WidgetExpr expr -> expr

            let setExpr =
                match setExpr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value)))
                    )
                | StringOrWidget.WidgetExpr expr -> expr

            Expr.Set(ExprSetNode(identifierExpr, setExpr, Range.Zero)))

[<AutoOpen>]
module SetBuilders =
    type Ast with

        static member SetExpr(identifier: WidgetBuilder<Expr>, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                Set.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        Set.IdentifierExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak identifier)),
                        Set.SetExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak expr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member SetExpr(identifier: StringVariant, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                Set.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        Set.IdentifierExpr.WithValue(StringOrWidget.StringExpr(identifier)),
                        Set.SetExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak expr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member SetExpr(identifier: WidgetBuilder<Expr>, expr: StringVariant) =
            WidgetBuilder<Expr>(
                Set.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        Set.IdentifierExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak identifier)),
                        Set.SetExpr.WithValue(StringOrWidget.StringExpr(expr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member SetExpr(identifier: StringVariant, expr: StringVariant) =
            WidgetBuilder<Expr>(
                Set.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        Set.IdentifierExpr.WithValue(StringOrWidget.StringExpr(identifier)),
                        Set.SetExpr.WithValue(StringOrWidget.StringExpr(expr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )
