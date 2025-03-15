namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

type InterpolatedString =
    | Text of string
    | Expr of WidgetBuilder<FillExprNode> * int

type FormatSpec =
    { Alignment: int option
      FormatString: string option }

module InterpolatedString =
    let Dollars = Attributes.defineScalar<string * bool> "Dollars"

    let Parts = Attributes.defineScalar<InterpolatedString list> "Parts"

    let WidgetKey =
        Widgets.register "InterpolatedString" (fun widget ->
            let dollars, isVerbatim = Widgets.getScalarValue widget Dollars
            // let numberOfBraces = Widgets.tryGetScalarValue widget NumberOfBraces
            let parts = Widgets.tryGetScalarValue widget Parts |> ValueOption.defaultValue []
            let isVerbatim = isVerbatim || dollars.Length > 1

            let processedParts =
                [ yield Choice1Of2(SingleTextNode.Create(dollars))
                  if isVerbatim then
                      yield Choice1Of2 SingleTextNode.tripleQuote
                  else
                      yield Choice1Of2 SingleTextNode.doubleQuote
                  for part in parts do

                      match part with
                      | Text text -> yield Choice1Of2(SingleTextNode.Create(text))

                      | Expr(expr, numberOfBraces) ->
                          for _ in 1..numberOfBraces do
                              yield Choice1Of2(SingleTextNode.leftCurlyBrace)

                          yield Choice2Of2(Gen.mkOak expr)

                          for _ in 1..numberOfBraces do
                              yield Choice1Of2(SingleTextNode.rightCurlyBrace)

                  if isVerbatim then
                      yield Choice1Of2 SingleTextNode.tripleQuote
                  else
                      yield Choice1Of2 SingleTextNode.doubleQuote ]

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
            (parts: InterpolatedString list, isVerbatim: bool option, dollars: string option)
            =
            let dollars = defaultArg dollars "$"
            // let numberOfBraces = defaultArg numberOfBraces dollars.Length
            let isVerbatim = defaultArg isVerbatim false
            let isVerbatim = isVerbatim || dollars.Length > 1

            WidgetBuilder<Expr>(
                InterpolatedString.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        InterpolatedString.Dollars.WithValue(dollars, isVerbatim),
                        InterpolatedString.Parts.WithValue(parts)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member InterpolatedStringExpr(parts: InterpolatedString list, ?isVerbatim: bool, ?dollars: string) =
            Ast.BaseInterpolatedStringExpr(parts, isVerbatim, dollars)

        static member InterpolatedStringExpr(parts: WidgetBuilder<Expr> list, ?isVerbatim: bool, ?dollars: string) =
            let parts = parts |> List.map(fun x -> InterpolatedString.Expr(Ast.FillExpr(x), 1))
            Ast.BaseInterpolatedStringExpr(parts, isVerbatim, dollars)

        static member InterpolatedStringExpr(parts: WidgetBuilder<Constant> list, ?isVerbatim: bool, ?dollars: string) =
            let parts = parts |> List.map(fun x -> InterpolatedString.Expr(Ast.FillExpr(x), 1))
            Ast.BaseInterpolatedStringExpr(parts, isVerbatim, dollars)

        static member InterpolatedStringExpr(parts: string list, ?isVerbatim: bool, ?dollars: string) =
            let parts = parts |> List.map(fun x -> InterpolatedString.Expr(Ast.FillExpr(x), 1))
            Ast.BaseInterpolatedStringExpr(parts, isVerbatim, dollars)

//
// static member InterpolatedStringExpr(exprs: WidgetBuilder<FillExprNode> list, ?numberOfBraces) =
//     let exprPairs = exprs |> List.map(fun e -> ((e, numberOfBraces), None))
//     Ast.BaseInterpolatedStringExpr([], exprPairs)
//
// static member InterpolatedStringExpr(expr: WidgetBuilder<FillExprNode>, ?numberOfBraces) =
//     Ast.BaseInterpolatedStringExpr([], [ ((expr, numberOfBraces), None) ])
//
// static member InterpolatedStringExpr(expr: WidgetBuilder<Expr>, ?numberOfBraces) =
//     Ast.BaseInterpolatedStringExpr([], [ ((Ast.FillExpr(expr), numberOfBraces), None) ])
//
// static member InterpolatedStringExpr(expr: WidgetBuilder<Constant>, ?numberOfBraces) =
//     Ast.BaseInterpolatedStringExpr([], [ ((Ast.FillExpr(expr), numberOfBraces), None) ])
//
// static member InterpolatedStringExpr(expr: string, ?numberOfBraces) =
//     Ast.BaseInterpolatedStringExpr([], [ ((Ast.FillExpr(expr), numberOfBraces), None) ])
//
// static member InterpolatedStringExpr(exprs: WidgetBuilder<Expr> list, ?numberOfBraces) =
//     let exprPairs =
//         exprs |> List.map(fun e -> ((Ast.FillExpr(e), numberOfBraces), None))
//
//     Ast.BaseInterpolatedStringExpr([], exprPairs)
//
// static member InterpolatedStringExpr(exprs: WidgetBuilder<Constant> list, ?numberOfBraces) =
//     let exprPairs =
//         exprs |> List.map(fun e -> ((Ast.FillExpr(e), numberOfBraces), None))
//
//     Ast.BaseInterpolatedStringExpr([], exprPairs)
//
// static member InterpolatedStringExpr(exprs: string list, ?numberOfBraces) =
//     let exprPairs =
//         exprs |> List.map(fun e -> ((Ast.FillExpr(e), numberOfBraces), None))
//
//     Ast.BaseInterpolatedStringExpr([], exprPairs)
//
// static member InterpolatedStringExpr
//     (dollars: string, exprs: WidgetBuilder<FillExprNode> list, ?numberOfBraces: int)
//     =
//     let exprPairs = exprs |> List.map(fun e -> ((e, numberOfBraces), None))
//     Ast.BaseInterpolatedStringExpr([], exprPairs, dollars = dollars)
//
// static member InterpolatedStringExpr(dollars: string, exprs: WidgetBuilder<Expr> list, ?numberOfBraces: int) =
//     let exprPairs =
//         exprs |> List.map(fun e -> ((Ast.FillExpr(e), numberOfBraces), None))
//
//     Ast.BaseInterpolatedStringExpr([], exprPairs, dollars = dollars)
//
// static member InterpolatedStringExpr
//     (dollars: string, exprs: WidgetBuilder<Constant> list, ?numberOfBraces: int)
//     =
//     let exprPairs =
//         exprs |> List.map(fun e -> ((Ast.FillExpr(e), numberOfBraces), None))
//
//     Ast.BaseInterpolatedStringExpr([], exprPairs, dollars = dollars)
//
// static member InterpolatedStringExpr(dollars: string, expr: string list, ?numberOfBraces: int) =
//     let exprPairs = expr |> List.map(fun e -> ((Ast.FillExpr(e), numberOfBraces), None))
//     Ast.BaseInterpolatedStringExpr([], exprPairs, dollars = dollars)
//
// static member InterpolatedStringExpr(dollars: string, expr: WidgetBuilder<FillExprNode>, ?numberOfBraces: int) =
//     Ast.BaseInterpolatedStringExpr(
//         [],
//         [ ((expr, numberOfBraces), None) ],
//         dollars = dollars
//     )
//
// static member InterpolatedStringExpr(dollars: string, expr: WidgetBuilder<Expr>, ?numberOfBraces: int) =
//     Ast.BaseInterpolatedStringExpr(
//         [],
//         [ ((Ast.FillExpr(expr), numberOfBraces), None) ],
//         dollars = dollars
//     )
//
// static member InterpolatedStringExpr(dollars: string, expr: WidgetBuilder<Constant>, ?numberOfBraces: int) =
//     Ast.BaseInterpolatedStringExpr(
//         [],
//         [ ((Ast.FillExpr(expr), numberOfBraces), None) ],
//         dollars = dollars
//     )
//
// static member InterpolatedStringExpr(dollars: string, expr: string, ?numberOfBraces: int) =
//     Ast.BaseInterpolatedStringExpr(
//         [],
//         [ ((Ast.FillExpr(expr), numberOfBraces), None) ],
//         dollars = dollars
//     )
//
// static member InterpolatedStringExpr(expr: WidgetBuilder<FillExprNode>, format: string, ?numberOfBraces: int) =
//     Ast.BaseInterpolatedStringExpr([], [ ((expr, numberOfBraces), Some format) ])
//
// static member InterpolatedStringExpr
//     (exprFormats: (WidgetBuilder<FillExprNode> * string) list, ?numberOfBraces: int)
//     =
//     let exprPairs = exprFormats |> List.map(fun (e, f) -> ((e, numberOfBraces), Some f))
//     Ast.BaseInterpolatedStringExpr([], exprPairs)
//
// static member InterpolatedStringExpr
//     (values: string list, exprFormats: (WidgetBuilder<FillExprNode> * string) list, ?numberOfBraces: int) =
//     let exprPairs = exprFormats |> List.map(fun (e, f) -> ((e, numberOfBraces), Some f))
//     Ast.BaseInterpolatedStringExpr(values, exprPairs)
//
// static member InterpolatedStringExpr(values: string list, exprs: string list, ?numberOfBraces: int) =
//     let exprPairs =
//         exprs |> List.map(fun e -> ((Ast.FillExpr(e), numberOfBraces), None))
//
//     Ast.BaseInterpolatedStringExpr(values, exprPairs)
//
// static member InterpolatedStringExpr
//     (values: string list, exprs: WidgetBuilder<Expr> list, ?numberOfBraces: int)
//     =
//     let exprPairs =
//         exprs |> List.map(fun e -> ((Ast.FillExpr(e), numberOfBraces), None))
//
//     Ast.BaseInterpolatedStringExpr(values, exprPairs)
//
// static member InterpolatedStringExpr
//     (values: string list, exprs: WidgetBuilder<Constant> list, ?numberOfBraces: int)
//     =
//     let exprPairs =
//         exprs |> List.map(fun e -> ((Ast.FillExpr(e), numberOfBraces), None))
//
//     Ast.BaseInterpolatedStringExpr(values, exprPairs)
//
// static member InterpolatedStringExpr(value: string, expr: WidgetBuilder<Expr>) =
//     Ast.BaseInterpolatedStringExpr([ value ], [ ((Ast.FillExpr(expr), None), None) ])
//
// static member InterpolatedStringExpr(value: string, expr: WidgetBuilder<Constant>) =
//     Ast.BaseInterpolatedStringExpr([ value ], [ ((Ast.FillExpr(expr), None), None) ])
//
// static member InterpolatedStringExpr(value: string, expr: string) =
//     Ast.BaseInterpolatedStringExpr([ value ], [ ((Ast.FillExpr(expr), None), None) ])
//
// static member InterpolatedStringExpr
//     (dollars: string, values: string list, exprs: WidgetBuilder<FillExprNode> list)
//     =
//     let exprPairs = exprs |> List.map(fun e -> ((e, None), None))
//     Ast.BaseInterpolatedStringExpr(values, exprPairs, dollars = dollars)
//
// static member InterpolatedStringExpr(dollars: string, values: string list, exprs: WidgetBuilder<Expr> list) =
//     let exprPairs = exprs |> List.map(fun e -> ((Ast.FillExpr(e), None), None))
//     Ast.BaseInterpolatedStringExpr(values, exprPairs, dollars = dollars)
//
// static member InterpolatedStringExpr
//     (dollars: string, values: string list, exprs: WidgetBuilder<Constant> list)
//     =
//     let exprPairs = exprs |> List.map(fun e -> ((Ast.FillExpr(e), None), None))
//
//     Ast.BaseInterpolatedStringExpr(values, exprPairs, dollars = dollars)
//
// static member InterpolatedStringExpr(dollars: string, values: string list, exprs: string list) =
//     let exprPairs = exprs |> List.map(fun e -> ((Ast.FillExpr(e), None), None))
//     Ast.BaseInterpolatedStringExpr(values, exprPairs, dollars = dollars)
//
// static member InterpolatedStringExpr(dollars: string, value: string, expr: string) =
//     Ast.BaseInterpolatedStringExpr(
//         [ value ],
//         [ ((Ast.FillExpr(expr), None), None) ],
//         dollars = dollars
//     )
