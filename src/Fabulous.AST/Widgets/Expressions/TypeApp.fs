namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module TypeApp =
    let IdentifierExpr = Attributes.defineScalar<StringOrWidget<Expr>> "Value"

    let TypeParameters = Attributes.defineScalar<Type list> "TypeParameters"

    let WidgetKey =
        Widgets.register "TypeApp" (fun widget ->
            let parameters = Widgets.getScalarValue widget TypeParameters
            let identifierExpr = Widgets.getScalarValue widget IdentifierExpr

            let identifierExpr =
                match identifierExpr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value)))
                    )
                | StringOrWidget.WidgetExpr expr -> expr

            Expr.TypeApp(
                ExprTypeAppNode(
                    identifierExpr,
                    SingleTextNode.lessThan,
                    parameters,
                    SingleTextNode.greaterThan,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module TypeAppBuilders =
    type Ast with

        static member TypeAppExpr(value: WidgetBuilder<Expr>, parameters: WidgetBuilder<Type> list) =
            let parameters = parameters |> List.map Gen.mkOak

            WidgetBuilder<Expr>(
                TypeApp.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        TypeApp.IdentifierExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak value)),
                        TypeApp.TypeParameters.WithValue(parameters)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member TypeAppExpr(value: WidgetBuilder<Expr>, parameter: WidgetBuilder<Type>) =
            WidgetBuilder<Expr>(
                TypeApp.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        TypeApp.IdentifierExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak value)),
                        TypeApp.TypeParameters.WithValue([ Gen.mkOak parameter ])
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member TypeAppExpr(value: string, parameters: WidgetBuilder<Type> list) =
            let parameters = parameters |> List.map Gen.mkOak

            WidgetBuilder<Expr>(
                TypeApp.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        TypeApp.IdentifierExpr.WithValue(StringOrWidget.StringExpr(Unquoted(value))),
                        TypeApp.TypeParameters.WithValue(parameters)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member TypeAppExpr(value: string, parameter: WidgetBuilder<Type>) =
            WidgetBuilder<Expr>(
                TypeApp.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        TypeApp.IdentifierExpr.WithValue(StringOrWidget.StringExpr(Unquoted(value))),
                        TypeApp.TypeParameters.WithValue([ Gen.mkOak parameter ])
                    ),
                    Array.empty,
                    Array.empty
                )
            )
