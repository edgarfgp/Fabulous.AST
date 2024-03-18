namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module AppLongIdentAndSingleParenArg =
    let ExprVal = Attributes.defineScalar<StringOrWidget<Expr>> "Name"
    let FunctionName = Attributes.defineScalar<string list> "FunctionName"

    let WidgetKey =
        Widgets.register "AppLongIdentAndSingleParenArg" (fun widget ->
            let expr = Widgets.getScalarValue widget ExprVal

            let functionName =
                Widgets.getScalarValue widget FunctionName
                |> List.map(SingleTextNode.Create)
                |> List.intersperse(SingleTextNode.Create ".")

            let expr =
                match expr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value)))
                    )
                | StringOrWidget.WidgetExpr expr -> expr

            Expr.AppLongIdentAndSingleParenArg(
                ExprAppLongIdentAndSingleParenArgNode(
                    IdentListNode(
                        [ for name in functionName do
                              IdentifierOrDot.Ident(name) ],
                        Range.Zero
                    ),
                    expr,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module AppLongIdentAndSingleParenArgBuilders =
    type Ast with

        static member AppLongIdentAndSingleParenArg(name: string list, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                AppLongIdentAndSingleParenArg.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        AppLongIdentAndSingleParenArg.FunctionName.WithValue(name),
                        AppLongIdentAndSingleParenArg.ExprVal.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak expr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member AppLongIdentAndSingleParenArg(name: string, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                AppLongIdentAndSingleParenArg.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        AppLongIdentAndSingleParenArg.FunctionName.WithValue([ name ]),
                        AppLongIdentAndSingleParenArg.ExprVal.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak expr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member AppLongIdentAndSingleParenArg(name: string list, expr: string) =
            WidgetBuilder<Expr>(
                AppLongIdentAndSingleParenArg.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        AppLongIdentAndSingleParenArg.FunctionName.WithValue(name),
                        AppLongIdentAndSingleParenArg.ExprVal.WithValue(StringOrWidget.StringExpr(Unquoted expr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member AppLongIdentAndSingleParenArg(name: string, expr: string) =
            WidgetBuilder<Expr>(
                AppLongIdentAndSingleParenArg.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        AppLongIdentAndSingleParenArg.FunctionName.WithValue([ name ]),
                        AppLongIdentAndSingleParenArg.ExprVal.WithValue(StringOrWidget.StringExpr(Unquoted expr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )
