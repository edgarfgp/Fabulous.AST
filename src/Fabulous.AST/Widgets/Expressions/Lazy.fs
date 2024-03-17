namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Lazy =
    let Value = Attributes.defineScalar<StringOrWidget<Expr>> "Value"

    let WidgetKey =
        Widgets.register "Lazy" (fun widget ->
            let expr = Widgets.getScalarValue widget Value

            let expr =
                match expr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value)))
                    )
                | StringOrWidget.WidgetExpr expr -> expr

            Expr.Lazy(ExprLazyNode(SingleTextNode.``lazy``, expr, Range.Zero)))

[<AutoOpen>]
module LazyBuilders =
    type Ast with

        static member LazyExpr(value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                Lazy.WidgetKey,
                AttributesBundle(
                    StackList.one(Lazy.Value.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak value))),
                    Array.empty,
                    Array.empty
                )
            )

        static member LazyExpr(value: StringVariant) =
            WidgetBuilder<Expr>(
                Lazy.WidgetKey,
                AttributesBundle(
                    StackList.one(Lazy.Value.WithValue(StringOrWidget.StringExpr(value))),
                    Array.empty,
                    Array.empty
                )
            )
