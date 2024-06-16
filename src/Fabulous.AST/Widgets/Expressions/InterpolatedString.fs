namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module InterpolatedString =
    let Dollars = Attributes.defineScalar<string> "Dollars"
    let Quotes = Attributes.defineScalar<SingleTextNode> "Quotes"
    let Parts = Attributes.defineScalar<(string list * Expr list)> "Parts"

    let WidgetKey =
        Widgets.register "InterpolatedString" (fun widget ->
            let dollars = Widgets.getScalarValue widget Dollars
            let quotes = Widgets.getScalarValue widget Quotes
            let parts = Widgets.tryGetScalarValue widget Parts

            let parts =
                parts
                |> ValueOption.map(fun exprs ->
                    exprs
                    |> (fun (values, exprs) ->
                        [ yield Choice1Of2(SingleTextNode.Create(dollars))
                          yield Choice1Of2(quotes)

                          match values, exprs with
                          | [], [] -> ()
                          | values, [] ->
                              for value in values do
                                  yield Choice1Of2(SingleTextNode.Create(value))

                          | [], exprs ->
                              for expr in exprs do
                                  yield Choice1Of2(SingleTextNode.leftCurlyBrace)
                                  yield Choice2Of2(FillExprNode(expr, None, Range.Zero))
                                  yield Choice1Of2(SingleTextNode.rightCurlyBrace)

                          | values, exprs ->
                              for i in 0 .. values.Length - 1 do
                                  yield Choice1Of2(SingleTextNode.Create(values.[i]))
                                  yield Choice1Of2(SingleTextNode.leftCurlyBrace)
                                  yield Choice2Of2(FillExprNode(exprs.[i], None, Range.Zero))
                                  yield Choice1Of2(SingleTextNode.rightCurlyBrace)

                          yield Choice1Of2(quotes) ]))
                |> ValueOption.defaultValue []

            Expr.InterpolatedStringExpr(ExprInterpolatedStringExprNode(parts, Range.Zero)))

[<AutoOpen>]
module InterpolatedStringBuilders =
    type Ast with

        static member private BaseInterpolatedStringExpr
            (values: string list, parts: WidgetBuilder<Expr> list, ?dollars: string, ?quote: SingleTextNode)
            =
            let dollars = defaultArg dollars "$"
            let quote = defaultArg quote SingleTextNode.doubleQuote
            let parts = parts |> List.map(Gen.mkOak)

            WidgetBuilder<Expr>(
                InterpolatedString.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        InterpolatedString.Dollars.WithValue(dollars),
                        InterpolatedString.Quotes.WithValue(quote),
                        InterpolatedString.Parts.WithValue((values, parts))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member InterpolatedStringExpr(expr: WidgetBuilder<Expr>) =
            Ast.BaseInterpolatedStringExpr(List.empty, [ expr ])

        static member InterpolatedStringExpr(expr: WidgetBuilder<Constant>) =
            Ast.InterpolatedStringExpr(Ast.ConstantExpr(expr))

        static member InterpolatedStringExpr(expr: string) =
            Ast.InterpolatedStringExpr(Ast.ConstantExpr(expr))

        static member InterpolatedStringExpr(exprs: WidgetBuilder<Expr> list) =
            Ast.BaseInterpolatedStringExpr(List.empty, exprs)

        static member InterpolatedStringExpr(exprs: WidgetBuilder<Constant> list) =
            let exprs = exprs |> List.map Ast.ConstantExpr
            Ast.BaseInterpolatedStringExpr(List.empty, exprs)

        static member InterpolatedStringExpr(exprs: string list) =
            let exprs = exprs |> List.map Ast.ConstantExpr
            Ast.BaseInterpolatedStringExpr(List.empty, exprs)

        static member InterpolatedRawStringExpr(expr: WidgetBuilder<Expr>) =
            Ast.BaseInterpolatedStringExpr(List.empty, [ expr ], quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr(expr: WidgetBuilder<Constant>) =
            Ast.InterpolatedRawStringExpr(Ast.ConstantExpr(expr))

        static member InterpolatedRawStringExpr(expr: string) =
            Ast.InterpolatedRawStringExpr(Ast.ConstantExpr(expr))

        static member InterpolatedRawStringExpr(exprs: WidgetBuilder<Expr> list) =
            Ast.BaseInterpolatedStringExpr(List.empty, exprs, quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr(exprs: WidgetBuilder<Constant> list) =
            let exprs = exprs |> List.map Ast.ConstantExpr
            Ast.InterpolatedRawStringExpr(exprs)

        static member InterpolatedRawStringExpr(exprs: string list) =
            let exprs = exprs |> List.map Ast.Constant
            Ast.InterpolatedRawStringExpr(exprs)

        static member InterpolatedRawStringExpr(dollars: string, expr: WidgetBuilder<Expr>) =
            Ast.BaseInterpolatedStringExpr(List.empty, [ expr ], dollars = dollars, quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr(dollars: string, expr: WidgetBuilder<Constant>) =
            Ast.InterpolatedRawStringExpr(dollars, Ast.ConstantExpr(expr))

        static member InterpolatedRawStringExpr(dollars: string, expr: string) =
            Ast.InterpolatedRawStringExpr(dollars, Ast.Constant(expr))

        static member InterpolatedRawStringExpr(dollars: string, exprs: WidgetBuilder<Expr> list) =
            Ast.BaseInterpolatedStringExpr(List.empty, exprs, dollars = dollars, quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr(dollars: string, exprs: WidgetBuilder<Constant> list) =
            let exprs = exprs |> List.map Ast.ConstantExpr
            Ast.InterpolatedRawStringExpr(dollars, exprs)

        static member InterpolatedRawStringExpr(dollars: string, exprs: string list) =
            let exprs = exprs |> List.map Ast.Constant
            Ast.InterpolatedRawStringExpr(dollars, exprs)

        static member InterpolatedStringExpr(value: string, expr: WidgetBuilder<Expr>) =
            Ast.BaseInterpolatedStringExpr([ value ], [ expr ])

        static member InterpolatedStringExpr(value: string, expr: WidgetBuilder<Constant>) =
            Ast.InterpolatedStringExpr(value, Ast.ConstantExpr(expr))

        static member InterpolatedStringExpr(value: string, expr: string) =
            Ast.InterpolatedStringExpr(value, Ast.Constant(expr))

        static member InterpolatedStringExpr(value: string list, expr: WidgetBuilder<Expr> list) =
            Ast.BaseInterpolatedStringExpr(value, expr)

        static member InterpolatedStringExpr(value: string list, exprs: WidgetBuilder<Constant> list) =
            let exprs = exprs |> List.map Ast.ConstantExpr
            Ast.InterpolatedStringExpr(value, exprs)

        static member InterpolatedStringExpr(value: string list, exprs: string list) =
            let exprs = exprs |> List.map Ast.Constant
            Ast.InterpolatedStringExpr(value, exprs)

        static member InterpolatedRawStringExpr(value: string list, exprs: WidgetBuilder<Expr> list) =
            Ast.BaseInterpolatedStringExpr(value, exprs, quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr(value: string list, exprs: WidgetBuilder<Constant> list) =
            let exprs = exprs |> List.map Ast.ConstantExpr
            Ast.InterpolatedRawStringExpr(value, exprs)

        static member InterpolatedRawStringExpr(value: string list, exprs: string list) =
            let exprs = exprs |> List.map Ast.Constant
            Ast.InterpolatedRawStringExpr(value, exprs)

        static member InterpolatedRawStringExpr(dollars: string, value: string, exprs: WidgetBuilder<Expr>) =
            Ast.BaseInterpolatedStringExpr([ value ], [ exprs ], dollars = dollars, quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr(dollars: string, value: string, exprs: WidgetBuilder<Constant>) =
            Ast.InterpolatedRawStringExpr(dollars, value, Ast.ConstantExpr(exprs))

        static member InterpolatedRawStringExpr(dollars: string, value: string, exprs: string) =
            Ast.InterpolatedRawStringExpr(dollars, value, Ast.Constant(exprs))

        static member InterpolatedRawStringExpr(dollars: string, value: string list, exprs: WidgetBuilder<Expr> list) =
            Ast.BaseInterpolatedStringExpr(value, exprs, dollars = dollars, quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr
            (dollars: string, value: string list, exprs: WidgetBuilder<Constant> list)
            =
            let exprs = exprs |> List.map Ast.ConstantExpr
            Ast.InterpolatedRawStringExpr(dollars, value, exprs)

        static member InterpolatedRawStringExpr(dollars: string, value: string list, exprs: string list) =
            let exprs = exprs |> List.map Ast.Constant
            Ast.InterpolatedRawStringExpr(dollars, value, exprs)
