namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

type TextSegment =
    | Text of string
    | Expr of WidgetBuilder<FillExprNode> * int

module InterpolatedString =
    let Dollars = Attributes.defineScalar<string * bool> "Dollars"
    let Parts = Attributes.defineScalar<TextSegment list> "Parts"

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
        | Text text -> [ Choice1Of2(SingleTextNode.Create(text)) ]
        | Expr(expr, braceCount) ->
            [ yield! createBraces braceCount SingleTextNode.leftCurlyBrace
              yield Choice2Of2(Gen.mkOak expr)
              yield! createBraces braceCount SingleTextNode.rightCurlyBrace ]
    // Check if an interpolated string contains only text segments
    let private isTextOnly(parts: TextSegment list) =
        parts
        |> List.forall (function
            | Text _ -> true
            | _ -> false)

    // Combine text-only segments for optimization
    let private combineTextOnly(parts: TextSegment list) =
        parts
        |> List.map (function
            | Text t -> t
            | _ -> "")
        |> String.concat ""

    let WidgetKey =
        Widgets.register "InterpolatedString" (fun widget ->
            let dollars, isVerbatim = Widgets.getScalarValue widget Dollars
            let parts = Widgets.tryGetScalarValue widget Parts |> ValueOption.defaultValue []

            let openQuote, closeQuote = getQuotes isVerbatim

            if isTextOnly parts then
                let combinedText = combineTextOnly parts

                let processedParts =
                    [ yield Choice1Of2(SingleTextNode.Create dollars)
                      yield openQuote
                      yield Choice1Of2(SingleTextNode.Create(combinedText))
                      yield closeQuote ]

                Expr.InterpolatedStringExpr(ExprInterpolatedStringExprNode(processedParts, Range.Zero))
            else
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
            (parts: TextSegment list, isVerbatim: bool option, dollars: int option)
            =
            let dollars =
                dollars
                |> Option.map(fun count -> String.replicate count "$")
                |> Option.defaultValue "$"

            let isVerbatim = defaultArg isVerbatim false

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

        /// <summary>
        /// Create an interpolated string expression.
        /// </summary>
        /// <param name="parts">The parts of the interpolated string.</param>
        /// <param name="isVerbatim">Whether the string is verbatim.</param>
        /// <param name="dollars">The number of dollars in the string.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InterpolatedStringExpr([ Text("Hello "); Expr(FillExpr(Int(12)), 1); Text(" world") ])
        ///         InterpolatedStringExpr([ Expr(FillExpr(Int(12)), 1) ])
        ///         InterpolatedStringExpr([ Expr(FillExpr(Int(12)), 1) ], isVerbatim = true)
        ///         InterpolatedStringExpr([ Expr(FillExpr(Int(12)), 1) ], dollars = "$$")
        ///
        ///     }
        /// }
        /// </code>
        static member InterpolatedStringExpr(parts: TextSegment list, ?isVerbatim: bool, ?dollars: int) =
            Ast.BaseInterpolatedStringExpr(parts, isVerbatim, dollars)

        /// <summary>
        /// Create an interpolated string expression.
        /// </summary>
        /// <param name="part">The parts of the interpolated string.</param>
        /// <param name="isVerbatim">Whether the string is verbatim.</param>
        /// <param name="dollars">The number of dollars in the string.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InterpolatedStringExpr([ Expr(FillExpr(Int(12)), 1) ])
        ///         InterpolatedStringExpr([ Expr(FillExpr(Int(12)), 1) ], isVerbatim = true)
        ///         InterpolatedStringExpr([ Expr(FillExpr(Int(12)), 1) ], dollars = "$$")
        ///     }
        /// }
        /// </code>
        static member InterpolatedStringExpr(part: TextSegment, ?isVerbatim: bool, ?dollars: int) =
            Ast.BaseInterpolatedStringExpr([ part ], isVerbatim, dollars)

        /// <summary>
        /// Create an interpolated string expression.
        /// </summary>
        /// <param name="parts">The parts of the interpolated string.</param>
        /// <param name="isVerbatim">Whether the string is verbatim.</param>
        /// <param name="dollars">The number of dollars in the string.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InterpolatedStringExpr([ ConstantExpr("12") ])
        ///         InterpolatedStringExpr([ ConstantExpr("12") ], isVerbatim = true)
        ///         InterpolatedStringExpr([ ConstantExpr("12") ], dollars = "$$")
        ///     }
        /// }
        /// </code>
        static member InterpolatedStringExpr(parts: WidgetBuilder<Expr> list, ?isVerbatim: bool, ?dollars: int) =
            let parts = parts |> List.map(fun x -> TextSegment.Expr(Ast.FillExpr(x), 1))
            Ast.BaseInterpolatedStringExpr(parts, isVerbatim, dollars)

        /// <summary>
        /// Create an interpolated string expression.
        /// </summary>
        /// <param name="part">The parts of the interpolated string.</param>
        /// <param name="isVerbatim">Whether the string is verbatim.</param>
        /// <param name="dollars">The number of dollars in the string.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InterpolatedStringExpr(ConstantExpr("12"))
        ///         InterpolatedStringExpr(ConstantExpr("12"), isVerbatim = true)
        ///         InterpolatedStringExpr(ConstantExpr("12"), dollars = "$$")
        ///     }
        /// }
        /// </code>
        static member InterpolatedStringExpr(part: WidgetBuilder<Expr>, ?isVerbatim: bool, ?dollars: int) =
            Ast.BaseInterpolatedStringExpr([ TextSegment.Expr(Ast.FillExpr(part), 1) ], isVerbatim, dollars)

        /// <summary>
        /// Create an interpolated string expression.
        /// </summary>
        /// <param name="parts">The parts of the interpolated string.</param>
        /// <param name="isVerbatim">Whether the string is verbatim.</param>
        /// <param name="dollars">The number of dollars in the string.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InterpolatedStringExpr([ Int(12) ])
        ///         InterpolatedStringExpr([ Int(12) ], isVerbatim = true)
        ///         InterpolatedStringExpr([ Int(12) ], dollars = "$$")
        ///     }
        /// }
        /// </code>
        static member InterpolatedStringExpr(parts: WidgetBuilder<Constant> list, ?isVerbatim: bool, ?dollars: int) =
            let parts = parts |> List.map(fun x -> TextSegment.Expr(Ast.FillExpr(x), 1))
            Ast.BaseInterpolatedStringExpr(parts, isVerbatim, dollars)

        /// <summary>
        /// Create an interpolated string expression.
        /// </summary>
        /// <param name="part">The parts of the interpolated string.</param>
        /// <param name="isVerbatim">Whether the string is verbatim.</param>
        /// <param name="dollars">The number of dollars in the string.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InterpolatedStringExpr(Int(12))
        ///         InterpolatedStringExpr(Int(12), isVerbatim = true)
        ///         InterpolatedStringExpr(Int(12), dollars = "$$")
        ///     }
        /// }
        /// </code>
        static member InterpolatedStringExpr(part: WidgetBuilder<Constant>, ?isVerbatim: bool, ?dollars: int) =
            Ast.BaseInterpolatedStringExpr([ TextSegment.Expr(Ast.FillExpr(part), 1) ], isVerbatim, dollars)

        /// <summary>
        /// Create an interpolated string expression.
        /// </summary>
        /// <param name="parts">The parts of the interpolated string.</param>
        /// <param name="isVerbatim">Whether the string is verbatim.</param>
        /// <param name="dollars">The number of dollars in the string.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InterpolatedStringExpr([ "12" ])
        ///         InterpolatedStringExpr([ "12" ], isVerbatim = true)
        ///         InterpolatedStringExpr([ "12" ], dollars = "$$")
        ///     }
        /// }
        /// </code>
        static member InterpolatedStringExpr(parts: string list, ?isVerbatim: bool, ?dollars: int) =
            let parts = parts |> List.map(fun x -> TextSegment.Expr(Ast.FillExpr(x), 1))
            Ast.BaseInterpolatedStringExpr(parts, isVerbatim, dollars)

        /// <summary>
        /// Create an interpolated string expression.
        /// </summary>
        /// <param name="part">The parts of the interpolated string.</param>
        /// <param name="isVerbatim">Whether the string is verbatim.</param>
        /// <param name="dollars">The number of dollars in the string.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InterpolatedStringExpr("12")
        ///         InterpolatedStringExpr("12", isVerbatim = true)
        ///         InterpolatedStringExpr("12", dollars = "$$")
        ///     }
        /// }
        /// </code>
        static member InterpolatedStringExpr(part: string, ?isVerbatim: bool, ?dollars: int) =
            Ast.BaseInterpolatedStringExpr([ TextSegment.Text(part) ], isVerbatim, dollars)
