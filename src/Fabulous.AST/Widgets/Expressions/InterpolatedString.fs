namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

type InterpolatedString =
    | Text of string
    | Expr of WidgetBuilder<FillExprNode> * int

module InterpolatedString =
    let Dollars = Attributes.defineScalar<string * bool> "Dollars"
    let Parts = Attributes.defineScalar<InterpolatedString list> "Parts"

    // Helper functions for creating nodes
    let private createBraces count braceFn =
        List.replicate count braceFn |> List.map Choice1Of2

    let private getQuotes isVerbatim =
        let quote =
            if isVerbatim then
                SingleTextNode.tripleQuote
            else
                SingleTextNode.doubleQuote

        (Choice1Of2 quote, Choice1Of2 quote)

    let private processPart =
        function
        | Text text -> [ Choice1Of2(SingleTextNode.Create text) ]
        | Expr(expr, braceCount) ->
            [ yield! createBraces braceCount SingleTextNode.leftCurlyBrace
              yield Choice2Of2(Gen.mkOak expr)
              yield! createBraces braceCount SingleTextNode.rightCurlyBrace ]

    let WidgetKey =
        Widgets.register "InterpolatedString" (fun widget ->
            let dollars, isVerbatim = Widgets.getScalarValue widget Dollars
            let parts = Widgets.tryGetScalarValue widget Parts |> ValueOption.defaultValue []

            let isVerbatim = isVerbatim || dollars.Length > 1
            let openQuote, closeQuote = getQuotes isVerbatim

            let processedParts =
                [ yield Choice1Of2(SingleTextNode.Create dollars)
                  yield openQuote
                  yield! parts |> List.collect processPart
                  yield closeQuote ]

            Expr.InterpolatedStringExpr(ExprInterpolatedStringExprNode(processedParts, Range.Zero)))

[<AutoOpen>]
module InterpolatedStringBuilders =
    type Ast with
        static member private BaseInterpolatedStringExpr
            (parts: InterpolatedString list, isVerbatim: bool option, dollars: string option)
            =
            let dollars = defaultArg dollars "$"
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

        static member InterpolatedStringExpr(part: InterpolatedString, ?isVerbatim: bool, ?dollars: string) =
            Ast.BaseInterpolatedStringExpr([ part ], isVerbatim, dollars)

        static member InterpolatedStringExpr(parts: WidgetBuilder<Expr> list, ?isVerbatim: bool, ?dollars: string) =
            let parts = parts |> List.map(fun x -> InterpolatedString.Expr(Ast.FillExpr(x), 1))
            Ast.BaseInterpolatedStringExpr(parts, isVerbatim, dollars)

        static member InterpolatedStringExpr(part: WidgetBuilder<Expr>, ?isVerbatim: bool, ?dollars: string) =
            Ast.BaseInterpolatedStringExpr([ InterpolatedString.Expr(Ast.FillExpr(part), 1) ], isVerbatim, dollars)

        static member InterpolatedStringExpr(parts: WidgetBuilder<Constant> list, ?isVerbatim: bool, ?dollars: string) =
            let parts = parts |> List.map(fun x -> InterpolatedString.Expr(Ast.FillExpr(x), 1))
            Ast.BaseInterpolatedStringExpr(parts, isVerbatim, dollars)

        static member InterpolatedStringExpr(part: WidgetBuilder<Constant>, ?isVerbatim: bool, ?dollars: string) =
            Ast.BaseInterpolatedStringExpr([ InterpolatedString.Expr(Ast.FillExpr(part), 1) ], isVerbatim, dollars)

        static member InterpolatedStringExpr(parts: string list, ?isVerbatim: bool, ?dollars: string) =
            let parts = parts |> List.map(fun x -> InterpolatedString.Expr(Ast.FillExpr(x), 1))
            Ast.BaseInterpolatedStringExpr(parts, isVerbatim, dollars)

        static member InterpolatedStringExpr(part: string, ?isVerbatim: bool, ?dollars: string) =
            Ast.BaseInterpolatedStringExpr([ InterpolatedString.Text(part) ], isVerbatim, dollars)
