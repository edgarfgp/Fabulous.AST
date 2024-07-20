namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module InterpolatedString =
    let Dollars = Attributes.defineScalar<string> "Dollars"
    let Quotes = Attributes.defineScalar<SingleTextNode> "Quotes"

    let Parts = Attributes.defineScalar<string list * FillExprNode list> "Parts"

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
                                  yield Choice2Of2(expr)
                                  yield Choice1Of2(SingleTextNode.rightCurlyBrace)

                          | values, exprs ->
                              let createNodes(value: string, expr: FillExprNode) =
                                  [ yield Choice1Of2(SingleTextNode.Create(value))
                                    yield Choice1Of2(SingleTextNode.leftCurlyBrace)
                                    yield Choice2Of2(expr)
                                    yield Choice1Of2(SingleTextNode.rightCurlyBrace) ]

                              let nodes = Seq.zip values exprs |> Seq.collect createNodes
                              yield! nodes

                          yield Choice1Of2(quotes) ]))
                |> ValueOption.defaultValue []

            Expr.InterpolatedStringExpr(ExprInterpolatedStringExprNode(parts, Range.Zero)))

[<AutoOpen>]
module InterpolatedStringBuilders =
    type Ast with

        static member private BaseInterpolatedStringExpr
            (values: string list, parts: WidgetBuilder<FillExprNode> list, ?dollars: string, ?quote: SingleTextNode) =
            let dollars = defaultArg dollars "$"
            let quote = defaultArg quote SingleTextNode.doubleQuote
            let parts = parts |> List.map(Gen.mkOak)

            WidgetBuilder<Expr>(
                InterpolatedString.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        InterpolatedString.Dollars.WithValue(dollars),
                        InterpolatedString.Quotes.WithValue(quote),
                        InterpolatedString.Parts.WithValue((values, (parts)))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member InterpolatedStringExpr(exprs: WidgetBuilder<FillExprNode> list) =
            Ast.BaseInterpolatedStringExpr(List.empty, exprs)

        static member InterpolatedStringExpr(expr: WidgetBuilder<FillExprNode>) = Ast.InterpolatedStringExpr([ expr ])

        static member InterpolatedStringExpr(expr: WidgetBuilder<Expr>) =
            Ast.InterpolatedStringExpr(Ast.FillExpr expr)

        static member InterpolatedStringExpr(expr: WidgetBuilder<Constant>) =
            Ast.InterpolatedStringExpr(Ast.ConstantExpr(expr))

        static member InterpolatedStringExpr(expr: string) =
            Ast.InterpolatedStringExpr(Ast.Constant(expr))

        static member InterpolatedStringExpr(exprs: WidgetBuilder<Expr> list) =
            let exprs = exprs |> List.map Ast.FillExpr
            Ast.InterpolatedStringExpr(exprs)

        static member InterpolatedStringExpr(exprs: WidgetBuilder<Constant> list) =
            let exprs = exprs |> List.map Ast.ConstantExpr
            Ast.InterpolatedStringExpr(exprs)

        static member InterpolatedStringExpr(exprs: string list) =
            let exprs = exprs |> List.map Ast.Constant
            Ast.InterpolatedStringExpr(exprs)

        static member InterpolatedRawStringExpr(exprs: WidgetBuilder<FillExprNode> list) =
            Ast.BaseInterpolatedStringExpr(List.empty, exprs, quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr(expr: WidgetBuilder<FillExprNode>) =
            Ast.InterpolatedRawStringExpr([ expr ])

        static member InterpolatedRawStringExpr(expr: WidgetBuilder<Expr>) =
            Ast.InterpolatedRawStringExpr(Ast.FillExpr(expr))

        static member InterpolatedRawStringExpr(expr: WidgetBuilder<Constant>) =
            Ast.InterpolatedRawStringExpr(Ast.ConstantExpr(expr))

        static member InterpolatedRawStringExpr(expr: string) =
            Ast.InterpolatedRawStringExpr(Ast.Constant(expr))

        static member InterpolatedRawStringExpr(exprs: WidgetBuilder<Expr> list) =
            let exprs = exprs |> List.map Ast.FillExpr
            Ast.InterpolatedRawStringExpr(exprs)

        static member InterpolatedRawStringExpr(exprs: WidgetBuilder<Constant> list) =
            let exprs = exprs |> List.map Ast.ConstantExpr
            Ast.InterpolatedRawStringExpr(exprs)

        static member InterpolatedRawStringExpr(exprs: string list) =
            let exprs = exprs |> List.map Ast.Constant
            Ast.InterpolatedRawStringExpr(exprs)

        static member InterpolatedRawStringExpr(dollars: string, exprs: WidgetBuilder<FillExprNode> list) =
            Ast.BaseInterpolatedStringExpr(List.empty, exprs, dollars = dollars, quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr(dollars: string, exprs: WidgetBuilder<Expr> list) =
            let exprs = exprs |> List.map Ast.FillExpr
            Ast.InterpolatedRawStringExpr(dollars, exprs)

        static member InterpolatedRawStringExpr(dollars: string, exprs: WidgetBuilder<Constant> list) =
            let exprs = exprs |> List.map Ast.ConstantExpr
            Ast.InterpolatedRawStringExpr(dollars, exprs)

        static member InterpolatedRawStringExpr(dollars: string, expr: string list) =
            let exprs = expr |> List.map Ast.Constant
            Ast.InterpolatedRawStringExpr(dollars, exprs)

        static member InterpolatedRawStringExpr(dollars: string, expr: WidgetBuilder<FillExprNode>) =
            Ast.InterpolatedRawStringExpr(dollars, [ expr ])

        static member InterpolatedRawStringExpr(dollars: string, expr: WidgetBuilder<Expr>) =
            Ast.InterpolatedRawStringExpr(dollars, Ast.FillExpr expr)

        static member InterpolatedRawStringExpr(dollars: string, expr: WidgetBuilder<Constant>) =
            Ast.InterpolatedRawStringExpr(dollars, Ast.ConstantExpr expr)

        static member InterpolatedRawStringExpr(dollars: string, expr: string) =
            Ast.InterpolatedRawStringExpr(dollars, Ast.Constant expr)

        static member InterpolatedStringExpr(values: string list, expr: WidgetBuilder<FillExprNode> list) =
            Ast.BaseInterpolatedStringExpr(values, expr)

        static member InterpolatedStringExpr(values: string list, exprs: WidgetBuilder<Expr> list) =
            let exprs = exprs |> List.map Ast.FillExpr
            Ast.InterpolatedStringExpr(values, exprs)

        static member InterpolatedStringExpr(values: string list, exprs: WidgetBuilder<Constant> list) =
            let exprs = exprs |> List.map Ast.ConstantExpr
            Ast.InterpolatedStringExpr(values, exprs)

        static member InterpolatedStringExpr(values: string list, exprs: string list) =
            let exprs = exprs |> List.map Ast.Constant
            Ast.InterpolatedStringExpr(values, exprs)

        static member InterpolatedRawStringExpr(values: string list, exprs: WidgetBuilder<FillExprNode> list) =
            Ast.BaseInterpolatedStringExpr(values, exprs, quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr(values: string list, exprs: WidgetBuilder<Expr> list) =
            let exprs = exprs |> List.map Ast.FillExpr
            Ast.InterpolatedRawStringExpr(values, exprs)

        static member InterpolatedRawStringExpr(values: string list, exprs: WidgetBuilder<Constant> list) =
            let exprs = exprs |> List.map Ast.ConstantExpr
            Ast.InterpolatedRawStringExpr(values, exprs)

        static member InterpolatedRawStringExpr(values: string list, exprs: string list) =
            let exprs = exprs |> List.map Ast.Constant
            Ast.InterpolatedRawStringExpr(values, exprs)

        static member InterpolatedStringExpr(value: string, expr: WidgetBuilder<Expr>) =
            Ast.InterpolatedStringExpr([ value ], [ Ast.FillExpr(expr) ])

        static member InterpolatedStringExpr(value: string, expr: WidgetBuilder<Constant>) =
            Ast.InterpolatedStringExpr(value, Ast.ConstantExpr(expr))

        static member InterpolatedStringExpr(value: string, expr: string) =
            Ast.InterpolatedStringExpr(value, Ast.Constant(expr))

        static member InterpolatedRawStringExpr
            (dollars: string, values: string list, exprs: WidgetBuilder<FillExprNode> list)
            =
            Ast.BaseInterpolatedStringExpr(values, exprs, dollars = dollars, quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr(dollars: string, values: string list, exprs: WidgetBuilder<Expr> list) =
            let exprs = exprs |> List.map Ast.FillExpr
            Ast.InterpolatedRawStringExpr(dollars, values, exprs)

        static member InterpolatedRawStringExpr
            (dollars: string, values: string list, exprs: WidgetBuilder<Constant> list)
            =
            let exprs = exprs |> List.map Ast.ConstantExpr
            Ast.InterpolatedRawStringExpr(dollars, values, exprs)

        static member InterpolatedRawStringExpr(dollars: string, values: string list, exprs: string list) =
            let exprs = exprs |> List.map Ast.Constant
            Ast.InterpolatedRawStringExpr(dollars, values, exprs)

        static member InterpolatedRawStringExpr(dollars: string, value: string, expr: string) =
            Ast.InterpolatedRawStringExpr(dollars, [ value ], [ expr ])
