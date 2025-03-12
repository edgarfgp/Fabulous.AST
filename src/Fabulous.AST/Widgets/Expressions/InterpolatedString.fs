namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

type FormatSpec =
    { Alignment: int option
      FormatString: string option }

module InterpolatedString =
    let Dollars = Attributes.defineScalar<string> "Dollars"
    let Quotes = Attributes.defineScalar<SingleTextNode> "Quotes"

    let Parts =
        Attributes.defineScalar<string list * ((FillExprNode * int option) * FormatSpec option) list> "Parts"

    let WidgetKey =
        Widgets.register "InterpolatedString" (fun widget ->
            let dollars = Widgets.getScalarValue widget Dollars
            let quotes = Widgets.getScalarValue widget Quotes
            let parts = Widgets.tryGetScalarValue widget Parts

            let textParts, expressionParts =
                match parts with
                | ValueSome(texts, exprs) -> texts, exprs
                | ValueNone -> [], []

            let nodeList = ResizeArray<Choice<SingleTextNode, FillExprNode>>()

            nodeList.Add(Choice1Of2(SingleTextNode.Create(dollars)))

            let isRawString = quotes.Text.Contains("\"\"\"")

            nodeList.Add(Choice1Of2(quotes))

            let mutable textIndex = 0
            let mutable exprIndex = 0

            if textParts.Length > 0 then
                nodeList.Add(Choice1Of2(SingleTextNode.Create(textParts[textIndex])))
                textIndex <- textIndex + 1

            while exprIndex < expressionParts.Length do
                let (expr, numberOfBraces), formatSpec = expressionParts[exprIndex]

                match numberOfBraces with
                | None -> nodeList.Add(Choice1Of2(SingleTextNode.leftCurlyBrace))
                | Some numberOfBraces ->
                    for _ in 1..numberOfBraces do
                        nodeList.Add(Choice1Of2(SingleTextNode.leftCurlyBrace))

                nodeList.Add(Choice2Of2(expr))

                let formatPrefix =
                    match formatSpec with
                    | Some spec ->
                        let alignStr =
                            match spec.Alignment with
                            | Some align ->
                                if align >= 0 then
                                    "," + string align
                                else
                                    "," + string align
                            | None -> ""

                        let formatStr =
                            match spec.FormatString with
                            | Some format -> ":" + format
                            | None -> ""

                        alignStr + formatStr
                    | None -> ""

                if not(System.String.IsNullOrEmpty(formatPrefix)) then
                    nodeList.Add(Choice1Of2(SingleTextNode.Create(formatPrefix)))

                match numberOfBraces with
                | None -> nodeList.Add(Choice1Of2(SingleTextNode.rightCurlyBrace))
                | Some numberOfBraces ->
                    for _ in 1..numberOfBraces do
                        nodeList.Add(Choice1Of2(SingleTextNode.rightCurlyBrace))

                exprIndex <- exprIndex + 1

                if textIndex < textParts.Length then
                    nodeList.Add(Choice1Of2(SingleTextNode.Create(textParts[textIndex])))
                    textIndex <- textIndex + 1

            if isRawString then
                nodeList.Add(Choice1Of2(SingleTextNode.Create("\"\"\"")))
            else
                nodeList.Add(Choice1Of2(SingleTextNode.Create("\"")))

            let processedParts = nodeList.ToArray() |> List.ofArray

            Expr.InterpolatedStringExpr(ExprInterpolatedStringExprNode(processedParts, Range.Zero)))

[<AutoOpen>]
module InterpolatedStringBuilders =
    type Ast with
        static member private ParseFormatSpec(formatStr: string) =
            if formatStr.Contains(",") then
                let parts = formatStr.Split([| ',' |], 2)
                let alignStr = parts[1].TrimStart()

                let alignment =
                    if alignStr.StartsWith("-") then
                        let success, value = System.Int32.TryParse(alignStr)
                        if success then Some value else None
                    else
                        let success, value = System.Int32.TryParse(alignStr)
                        if success then Some value else None

                let formatString =
                    if parts[0].Contains(":") || (parts.Length > 1 && parts[1].Contains(":")) then
                        let formatPart = if parts[0].Contains(":") then parts[0] else parts[1]
                        let formatIndex = formatPart.IndexOf(":")

                        if formatIndex >= 0 then
                            Some(formatPart.Substring(formatIndex + 1))
                        else
                            None
                    else
                        None

                { Alignment = alignment
                  FormatString = formatString }
            else if formatStr.Contains(":") then
                let formatIndex = formatStr.IndexOf(":")

                { Alignment = None
                  FormatString = Some(formatStr.Substring(formatIndex + 1)) }
            else
                { Alignment = None
                  FormatString = None }

        static member private BaseInterpolatedStringExpr
            (
                textParts: string list,
                exprParts: ((WidgetBuilder<FillExprNode> * int option) * string option) list,
                ?dollars: string,
                ?quote: SingleTextNode
            ) =
            let dollars = defaultArg dollars "$"
            let quote = defaultArg quote SingleTextNode.doubleQuote

            let processedExpressions =
                exprParts
                |> List.map(fun ((expr, numberOfBraces), format) ->
                    let node = Gen.mkOak expr
                    let formatSpec = format |> Option.map Ast.ParseFormatSpec
                    ((node, numberOfBraces), formatSpec))

            WidgetBuilder<Expr>(
                InterpolatedString.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        InterpolatedString.Dollars.WithValue(dollars),
                        InterpolatedString.Quotes.WithValue(quote),
                        InterpolatedString.Parts.WithValue((textParts, processedExpressions))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member InterpolatedStringExpr(exprs: WidgetBuilder<FillExprNode> list) =
            let exprPairs = exprs |> List.map(fun e -> ((e, None), None))
            Ast.BaseInterpolatedStringExpr([], exprPairs)

        static member InterpolatedStringExpr(expr: WidgetBuilder<FillExprNode>) =
            Ast.BaseInterpolatedStringExpr([], [ ((expr, None), None) ])

        static member InterpolatedStringExpr(expr: WidgetBuilder<Expr>) =
            Ast.BaseInterpolatedStringExpr([], [ ((Ast.FillExpr(expr), None), None) ])

        static member InterpolatedStringExpr(expr: WidgetBuilder<Constant>) =
            Ast.BaseInterpolatedStringExpr([], [ ((Ast.FillExpr(expr), None), None) ])

        static member InterpolatedStringExpr(expr: string) =
            Ast.BaseInterpolatedStringExpr([], [ ((Ast.FillExpr(expr), None), None) ])

        static member InterpolatedStringExpr(exprs: WidgetBuilder<Expr> list) =
            let exprPairs = exprs |> List.map(fun e -> ((Ast.FillExpr(e), None), None))
            Ast.BaseInterpolatedStringExpr([], exprPairs)

        static member InterpolatedStringExpr(exprs: WidgetBuilder<Constant> list) =
            let exprPairs = exprs |> List.map(fun e -> ((Ast.FillExpr(e), None), None))
            Ast.BaseInterpolatedStringExpr([], exprPairs)

        static member InterpolatedStringExpr(exprs: string list) =
            let exprPairs = exprs |> List.map(fun e -> ((Ast.FillExpr(e), None), None))
            Ast.BaseInterpolatedStringExpr([], exprPairs)

        static member InterpolatedRawStringExpr(exprs: WidgetBuilder<FillExprNode> list, ?numberOfBraces: int) =
            let exprPairs = exprs |> List.map(fun e -> ((e, numberOfBraces), None))
            Ast.BaseInterpolatedStringExpr([], exprPairs, quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr(expr: WidgetBuilder<FillExprNode>, ?numberOfBraces: int) =
            Ast.BaseInterpolatedStringExpr([], [ ((expr, numberOfBraces), None) ], quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr(expr: WidgetBuilder<Expr>, ?numberOfBraces: int) =
            Ast.BaseInterpolatedStringExpr(
                [],
                [ ((Ast.FillExpr(expr), numberOfBraces), None) ],
                quote = SingleTextNode.tripleQuote
            )

        static member InterpolatedRawStringExpr(expr: WidgetBuilder<Constant>, ?numberOfBraces: int) =
            Ast.BaseInterpolatedStringExpr(
                [],
                [ ((Ast.FillExpr(expr), numberOfBraces), None) ],
                quote = SingleTextNode.tripleQuote
            )

        static member InterpolatedRawStringExpr(expr: string, ?numberOfBraces: int) =
            Ast.BaseInterpolatedStringExpr(
                [],
                [ ((Ast.FillExpr(expr), numberOfBraces), None) ],
                quote = SingleTextNode.tripleQuote
            )

        static member InterpolatedRawStringExpr(exprs: WidgetBuilder<Expr> list, ?numberOfBraces: int) =
            let exprPairs =
                exprs |> List.map(fun e -> ((Ast.FillExpr(e), numberOfBraces), None))

            Ast.BaseInterpolatedStringExpr([], exprPairs, quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr(exprs: WidgetBuilder<Constant> list, ?numberOfBraces: int) =
            let exprPairs =
                exprs |> List.map(fun e -> ((Ast.FillExpr(e), numberOfBraces), None))

            Ast.BaseInterpolatedStringExpr([], exprPairs, quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr(exprs: string list, ?numberOfBraces: int) =
            let exprPairs =
                exprs |> List.map(fun e -> ((Ast.FillExpr(e), numberOfBraces), None))

            Ast.BaseInterpolatedStringExpr([], exprPairs, quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr
            (dollars: string, exprs: WidgetBuilder<FillExprNode> list, ?numberOfBraces: int)
            =
            let exprPairs = exprs |> List.map(fun e -> ((e, numberOfBraces), None))
            Ast.BaseInterpolatedStringExpr([], exprPairs, dollars = dollars, quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr
            (dollars: string, exprs: WidgetBuilder<Expr> list, ?numberOfBraces: int)
            =
            let exprPairs =
                exprs |> List.map(fun e -> ((Ast.FillExpr(e), numberOfBraces), None))

            Ast.BaseInterpolatedStringExpr([], exprPairs, dollars = dollars, quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr
            (dollars: string, exprs: WidgetBuilder<Constant> list, ?numberOfBraces: int)
            =
            let exprPairs =
                exprs |> List.map(fun e -> ((Ast.FillExpr(e), numberOfBraces), None))

            Ast.BaseInterpolatedStringExpr([], exprPairs, dollars = dollars, quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr(dollars: string, expr: string list, ?numberOfBraces: int) =
            let exprPairs = expr |> List.map(fun e -> ((Ast.FillExpr(e), numberOfBraces), None))
            Ast.BaseInterpolatedStringExpr([], exprPairs, dollars = dollars, quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr
            (dollars: string, expr: WidgetBuilder<FillExprNode>, ?numberOfBraces: int)
            =
            Ast.BaseInterpolatedStringExpr(
                [],
                [ ((expr, numberOfBraces), None) ],
                dollars = dollars,
                quote = SingleTextNode.tripleQuote
            )

        static member InterpolatedRawStringExpr(dollars: string, expr: WidgetBuilder<Expr>, ?numberOfBraces: int) =
            Ast.BaseInterpolatedStringExpr(
                [],
                [ ((Ast.FillExpr(expr), numberOfBraces), None) ],
                dollars = dollars,
                quote = SingleTextNode.tripleQuote
            )

        static member InterpolatedRawStringExpr(dollars: string, expr: WidgetBuilder<Constant>, ?numberOfBraces: int) =
            Ast.BaseInterpolatedStringExpr(
                [],
                [ ((Ast.FillExpr(expr), numberOfBraces), None) ],
                dollars = dollars,
                quote = SingleTextNode.tripleQuote
            )

        static member InterpolatedRawStringExpr(dollars: string, expr: string, ?numberOfBraces: int) =
            Ast.BaseInterpolatedStringExpr(
                [],
                [ ((Ast.FillExpr(expr), numberOfBraces), None) ],
                dollars = dollars,
                quote = SingleTextNode.tripleQuote
            )

        static member InterpolatedStringExpr
            (values: string list, expr: WidgetBuilder<FillExprNode> list, ?numberOfBraces: int)
            =
            let exprPairs = expr |> List.map(fun e -> ((e, numberOfBraces), None))
            Ast.BaseInterpolatedStringExpr(values, exprPairs)

        static member InterpolatedStringExpr(expr: WidgetBuilder<FillExprNode>, format: string, ?numberOfBraces: int) =
            Ast.BaseInterpolatedStringExpr([], [ ((expr, numberOfBraces), Some format) ])

        static member InterpolatedStringExpr
            (exprFormats: (WidgetBuilder<FillExprNode> * string) list, ?numberOfBraces: int)
            =
            let exprPairs = exprFormats |> List.map(fun (e, f) -> ((e, numberOfBraces), Some f))
            Ast.BaseInterpolatedStringExpr([], exprPairs)

        static member InterpolatedRawStringExpr
            (expr: WidgetBuilder<FillExprNode>, format: string, ?numberOfBraces: int)
            =
            Ast.BaseInterpolatedStringExpr(
                [],
                [ ((expr, numberOfBraces), Some format) ],
                quote = SingleTextNode.tripleQuote
            )

        static member InterpolatedStringExpr
            (values: string list, exprFormats: (WidgetBuilder<FillExprNode> * string) list, ?numberOfBraces: int) =
            let exprPairs = exprFormats |> List.map(fun (e, f) -> ((e, numberOfBraces), Some f))
            Ast.BaseInterpolatedStringExpr(values, exprPairs)

        static member InterpolatedStringExpr
            (values: string list, exprs: WidgetBuilder<Expr> list, ?numberOfBraces: int)
            =
            let exprPairs =
                exprs |> List.map(fun e -> ((Ast.FillExpr(e), numberOfBraces), None))

            Ast.BaseInterpolatedStringExpr(values, exprPairs)

        static member InterpolatedStringExpr
            (values: string list, exprs: WidgetBuilder<Constant> list, ?numberOfBraces: int)
            =
            let exprPairs =
                exprs |> List.map(fun e -> ((Ast.FillExpr(e), numberOfBraces), None))

            Ast.BaseInterpolatedStringExpr(values, exprPairs)

        static member InterpolatedStringExpr(values: string list, exprs: string list, ?numberOfBraces: int) =
            let exprPairs =
                exprs |> List.map(fun e -> ((Ast.FillExpr(e), numberOfBraces), None))

            Ast.BaseInterpolatedStringExpr(values, exprPairs)

        static member InterpolatedRawStringExpr
            (values: string list, exprs: WidgetBuilder<FillExprNode> list, ?numberOfBraces: int)
            =
            let exprPairs = exprs |> List.map(fun e -> ((e, numberOfBraces), None))
            Ast.BaseInterpolatedStringExpr(values, exprPairs, quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr
            (values: string list, exprs: WidgetBuilder<Expr> list, ?numberOfBraces: int)
            =
            let exprPairs =
                exprs |> List.map(fun e -> ((Ast.FillExpr(e), numberOfBraces), None))

            Ast.BaseInterpolatedStringExpr(values, exprPairs, quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr
            (values: string list, exprs: WidgetBuilder<Constant> list, ?numberOfBraces: int)
            =
            let exprPairs =
                exprs |> List.map(fun e -> ((Ast.FillExpr(e), numberOfBraces), None))

            Ast.BaseInterpolatedStringExpr(values, exprPairs, quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr(values: string list, exprs: string list, ?numberOfBraces: int) =
            let exprPairs =
                exprs |> List.map(fun e -> ((Ast.FillExpr(e), numberOfBraces), None))

            Ast.BaseInterpolatedStringExpr(values, exprPairs, quote = SingleTextNode.tripleQuote)

        static member InterpolatedStringExpr(value: string, expr: WidgetBuilder<Expr>) =
            Ast.BaseInterpolatedStringExpr([ value ], [ ((Ast.FillExpr(expr), None), None) ])

        static member InterpolatedStringExpr(value: string, expr: WidgetBuilder<Constant>) =
            Ast.BaseInterpolatedStringExpr([ value ], [ ((Ast.FillExpr(expr), None), None) ])

        static member InterpolatedStringExpr(value: string, expr: string) =
            Ast.BaseInterpolatedStringExpr([ value ], [ ((Ast.FillExpr(expr), None), None) ])

        static member InterpolatedRawStringExpr
            (dollars: string, values: string list, exprs: WidgetBuilder<FillExprNode> list)
            =
            let exprPairs = exprs |> List.map(fun e -> ((e, None), None))
            Ast.BaseInterpolatedStringExpr(values, exprPairs, dollars = dollars, quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr(dollars: string, values: string list, exprs: WidgetBuilder<Expr> list) =
            let exprPairs = exprs |> List.map(fun e -> ((Ast.FillExpr(e), None), None))
            Ast.BaseInterpolatedStringExpr(values, exprPairs, dollars = dollars, quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr
            (dollars: string, values: string list, exprs: WidgetBuilder<Constant> list)
            =
            let exprPairs = exprs |> List.map(fun e -> ((Ast.FillExpr(e), None), None))

            Ast.BaseInterpolatedStringExpr(values, exprPairs, dollars = dollars, quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr(dollars: string, values: string list, exprs: string list) =
            let exprPairs = exprs |> List.map(fun e -> ((Ast.FillExpr(e), None), None))
            Ast.BaseInterpolatedStringExpr(values, exprPairs, dollars = dollars, quote = SingleTextNode.tripleQuote)

        static member InterpolatedRawStringExpr(dollars: string, value: string, expr: string) =
            Ast.BaseInterpolatedStringExpr(
                [ value ],
                [ ((Ast.FillExpr(expr), None), None) ],
                dollars = dollars,
                quote = SingleTextNode.tripleQuote
            )
