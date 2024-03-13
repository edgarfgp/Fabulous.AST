namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module IfThenElse =
    let IfExpr = Attributes.defineScalar<StringOrWidget<Expr>> "IfExpr"
    let ThenExpr = Attributes.defineScalar<StringOrWidget<Expr>> "ThenExpr"
    let ElseExpr = Attributes.defineScalar<StringOrWidget<Expr>> "ElseExpr"

    let WidgetKey =
        Widgets.register "IfThenElse" (fun widget ->
            let ifExpr = Widgets.getScalarValue widget IfExpr
            let thenExpr = Widgets.getScalarValue widget ThenExpr
            let elseExpr = Widgets.getScalarValue widget ElseExpr

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

            let thenExpr =
                match thenExpr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(
                            SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value, hasQuotes))
                        )
                    )
                | StringOrWidget.WidgetExpr expr -> expr

            let elseExpr =
                match elseExpr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(
                            SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value, hasQuotes))
                        )
                    )
                | StringOrWidget.WidgetExpr expr -> expr

            Expr.IfThenElse(
                ExprIfThenElseNode(
                    IfKeywordNode.SingleWord(SingleTextNode.``if``),
                    ifExpr,
                    SingleTextNode.``then``,
                    thenExpr,
                    SingleTextNode.``else``,
                    elseExpr,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module IfThenElseBuilders =
    type Ast with

        static member inline IfThenElseExpr
            (ifExpr: WidgetBuilder<Expr>, thenExpr: WidgetBuilder<Expr>, elseExpr: WidgetBuilder<Expr>)
            =
            WidgetBuilder<Expr>(
                IfThenElse.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        IfThenElse.IfExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak ifExpr)),
                        IfThenElse.ThenExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak thenExpr)),
                        IfThenElse.ElseExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak elseExpr))
                    ),
                    ValueNone,
                    ValueNone
                )
            )

        static member inline IfThenElseExpr(ifExpr: string, thenExpr: string, elseExpr: string) =
            WidgetBuilder<Expr>(
                IfThenElse.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        IfThenElse.IfExpr.WithValue(StringOrWidget.StringExpr(ifExpr)),
                        IfThenElse.ThenExpr.WithValue(StringOrWidget.StringExpr(thenExpr)),
                        IfThenElse.ElseExpr.WithValue(StringOrWidget.StringExpr(elseExpr))
                    ),
                    ValueNone,
                    ValueNone
                )
            )
