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
            (parts: TextSegment list, isVerbatim: bool option, dollars: string option)
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
        static member InterpolatedStringExpr(parts: TextSegment list, ?isVerbatim: bool, ?dollars: string) =
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
        static member InterpolatedStringExpr(part: TextSegment, ?isVerbatim: bool, ?dollars: string) =
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
        static member InterpolatedStringExpr(parts: WidgetBuilder<Expr> list, ?isVerbatim: bool, ?dollars: string) =
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
        static member InterpolatedStringExpr(part: WidgetBuilder<Expr>, ?isVerbatim: bool, ?dollars: string) =
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
        static member InterpolatedStringExpr(parts: WidgetBuilder<Constant> list, ?isVerbatim: bool, ?dollars: string) =
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
        static member InterpolatedStringExpr(part: WidgetBuilder<Constant>, ?isVerbatim: bool, ?dollars: string) =
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
        static member InterpolatedStringExpr(parts: string list, ?isVerbatim: bool, ?dollars: string) =
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
        static member InterpolatedStringExpr(part: string, ?isVerbatim: bool, ?dollars: string) =
            Ast.BaseInterpolatedStringExpr([ TextSegment.Text(part) ], isVerbatim, dollars)
