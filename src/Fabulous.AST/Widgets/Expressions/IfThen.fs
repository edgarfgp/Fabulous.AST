namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module IfThen =
    let IfNode = Attributes.defineScalar<IfKeywordNode> "IfNode"
    let IfExpr = Attributes.defineScalar<StringOrWidget<Expr>> "IfExpr"
    let ThenExpr = Attributes.defineScalar<StringOrWidget<Expr>> "ThenExpr"

    let WidgetKey =
        Widgets.register "IfThen" (fun widget ->
            let ifNode = Widgets.getScalarValue widget IfNode
            let ifExpr = Widgets.getScalarValue widget IfExpr

            let hasQuotes =
                Widgets.tryGetScalarValue widget Expr.HasQuotes |> ValueOption.defaultValue true

            let ifExpr =
                match ifExpr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(
                            SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value, hasQuotes))
                        )
                    )
                | StringOrWidget.WidgetExpr expr -> expr

            let thenExpr = Widgets.getScalarValue widget ThenExpr

            let thenExpr =
                match thenExpr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(
                            SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value, hasQuotes))
                        )
                    )
                | StringOrWidget.WidgetExpr expr -> expr

            Expr.IfThen(ExprIfThenNode(ifNode, ifExpr, SingleTextNode.``then``, thenExpr, Range.Zero)))

[<AutoOpen>]
module IfThenBuilders =
    type Ast with

        static member inline IfThenExpr(ifExpr: WidgetBuilder<Expr>, thenExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                IfThen.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        IfThen.IfNode.WithValue(IfKeywordNode.SingleWord(SingleTextNode.``if``)),
                        IfThen.IfExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak ifExpr)),
                        IfThen.ThenExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak thenExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member inline IfThenExpr(ifExpr: string, thenExpr: string) =
            WidgetBuilder<Expr>(
                IfThen.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        IfThen.IfNode.WithValue(IfKeywordNode.SingleWord(SingleTextNode.``if``)),
                        IfThen.IfExpr.WithValue(StringOrWidget.StringExpr(ifExpr)),
                        IfThen.ThenExpr.WithValue(StringOrWidget.StringExpr(thenExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member inline ElIfThenExpr(elIfExpr: WidgetBuilder<Expr>, thenExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                IfThen.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        IfThen.IfNode.WithValue(IfKeywordNode.SingleWord(SingleTextNode.``elif``)),
                        IfThen.IfExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak elIfExpr)),
                        IfThen.ThenExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak thenExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member inline ElIfThenExpr(elIfExpr: string, thenExpr: string) =
            WidgetBuilder<Expr>(
                IfThen.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        IfThen.IfNode.WithValue(IfKeywordNode.SingleWord(SingleTextNode.``elif``)),
                        IfThen.IfExpr.WithValue(StringOrWidget.StringExpr(elIfExpr)),
                        IfThen.ThenExpr.WithValue(StringOrWidget.StringExpr(thenExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member inline ElseIfThenExpr(elseIfExpr: WidgetBuilder<Expr>, thenExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                IfThen.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        IfThen.IfNode.WithValue(
                            IfKeywordNode.ElseIf(
                                ElseIfNode(Range.Zero, Range.Zero, Unchecked.defaultof<Node>, Range.Zero)
                            )
                        ),
                        IfThen.IfExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak elseIfExpr)),
                        IfThen.ThenExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak thenExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member inline ElseIfThenExpr(elseIfExpr: string, thenExpr: string) =
            WidgetBuilder<Expr>(
                IfThen.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        IfThen.IfNode.WithValue(
                            IfKeywordNode.ElseIf(
                                ElseIfNode(Range.Zero, Range.Zero, Unchecked.defaultof<Node>, Range.Zero)
                            )
                        ),
                        IfThen.IfExpr.WithValue(StringOrWidget.StringExpr(elseIfExpr)),
                        IfThen.ThenExpr.WithValue(StringOrWidget.StringExpr(thenExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )
