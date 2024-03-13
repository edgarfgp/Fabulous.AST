namespace Fabulous.AST

open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module InfixApp =
    let LeftHandSide = Attributes.defineScalar<StringOrWidget<Expr>> "LeftHandSide"
    let Operator = Attributes.defineScalar "Operator"
    let RightHandSide = Attributes.defineScalar<StringOrWidget<Expr>> "RightHandSide"

    let WidgetKey =
        Widgets.register "Condition" (fun widget ->
            let lhs = Widgets.getScalarValue widget LeftHandSide

            let hasQuotes =
                Widgets.tryGetScalarValue widget Expr.HasQuotes |> ValueOption.defaultValue true

            let lhs =
                match lhs with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(
                            SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value, hasQuotes))
                        )
                    )
                | StringOrWidget.WidgetExpr expr -> expr

            let operator = Widgets.getScalarValue widget Operator
            let rhs = Widgets.getScalarValue widget RightHandSide

            let rhs =
                match rhs with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(
                            SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value, hasQuotes))
                        )
                    )
                | StringOrWidget.WidgetExpr expr -> expr

            Expr.InfixApp(ExprInfixAppNode(lhs, SingleTextNode.Create(operator), rhs, Range.Zero)))

[<AutoOpen>]
module InfixAppBuilders =
    type Ast with

        static member inline InfixAppExpr(lhs: WidgetBuilder<Expr>, operator: string, rhs: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                InfixApp.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        InfixApp.Operator.WithValue(operator),
                        InfixApp.LeftHandSide.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak(lhs))),
                        InfixApp.RightHandSide.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak(rhs)))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member inline InfixAppExpr(lhs: string, operator: string, rhs: string) =
            WidgetBuilder<Expr>(
                InfixApp.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        InfixApp.Operator.WithValue(operator),
                        InfixApp.LeftHandSide.WithValue(StringOrWidget.StringExpr(lhs)),
                        InfixApp.RightHandSide.WithValue(StringOrWidget.StringExpr(rhs))
                    ),
                    Array.empty,
                    Array.empty
                )
            )
