namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

type TextSegment =
    | Text of string
    | Expr of WidgetBuilder<FillExprNode> * int

module InterpolatedString =
    /// The `$` prefix, whether the string is triple-quoted, and whether it is verbatim (`@`).
    let Dollars = Attributes.defineScalar<string * bool * bool> "Dollars"
    let Parts = Attributes.defineScalar<TextSegment seq> "Parts"

    let private createBraces count braceFn =
        List.replicate count braceFn |> List.map Choice1Of2

    let private getQuotes isTripleQuoted =
        let quote =
            if isTripleQuoted then
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
    let private isTextOnly(parts: TextSegment seq) =
        parts
        |> Seq.forall(function
            | Text _ -> true
            | _ -> false)

    // Combine text-only segments for optimization
    let private combineTextOnly(parts: TextSegment seq) =
        parts
        |> Seq.map(function
            | Text t -> t
            | _ -> "")
        |> String.concat ""

    let WidgetKey =
        Widgets.register "InterpolatedString" (fun widget ->
            let dollars, isTripleQuoted, isVerbatim = Widgets.getScalarValue widget Dollars
            let parts = Widgets.tryGetScalarValue widget Parts |> ValueOption.defaultValue []

            if isVerbatim && isTripleQuoted then
                invalidArg "isVerbatim" "An interpolated string cannot be both triple-quoted and verbatim."

            // F# only allows extra `$` (extended interpolation) on triple-quoted strings.
            if isVerbatim && dollars.Length > 1 then
                invalidArg "dollars" "A verbatim interpolated string ($@\"...\") cannot have more than one $."

            // `$@"..."`: the `@` follows the dollars.
            let dollars = if isVerbatim then dollars + "@" else dollars

            let openQuote, closeQuote = getQuotes isTripleQuoted

            if isTextOnly parts then
                let combinedText = combineTextOnly parts

                let processedParts =
                    [ yield Choice1Of2(SingleTextNode.Create dollars)
                      yield openQuote
                      yield Choice1Of2(SingleTextNode.Create(combinedText))
                      yield closeQuote ]

                Expr.InterpolatedStringExpr(ExprInterpolatedStringExprNode(processedParts, Range.range0))
            else
                let processedParts =
                    [ yield Choice1Of2(SingleTextNode.Create dollars)
                      yield openQuote
                      yield! parts |> Seq.collect processPart
                      yield closeQuote ]

                Expr.InterpolatedStringExpr(ExprInterpolatedStringExprNode(processedParts, Range.range0)))

[<AutoOpen>]
module InterpolatedStringBuilders =
    type Ast with
        static member private BaseInterpolatedStringExpr
            (parts: TextSegment seq, isTripleQuoted: bool option, isVerbatim: bool option, dollars: int option)
            =
            let dollars =
                dollars
                |> Option.map(fun count -> String.replicate count "$")
                |> Option.defaultValue "$"

            let isTripleQuoted = defaultArg isTripleQuoted false
            let isVerbatim = defaultArg isVerbatim false

            WidgetBuilder<Expr>(
                InterpolatedString.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        InterpolatedString.Dollars.WithValue(dollars, isTripleQuoted, isVerbatim),
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
        /// <param name="isTripleQuoted">Whether the string is triple-quoted (<c>$"""..."""</c>).</param>
        /// <param name="isVerbatim">Whether the string is verbatim (<c>$@"..."</c>). It cannot also be triple-quoted or have more than one <c>$</c>.</param>
        /// <param name="dollars">The number of dollars in the string.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InterpolatedStringExpr([ Text("Hello "); Expr(FillExpr(Int(12)), 1); Text(" world") ])
        ///         InterpolatedStringExpr([ Expr(FillExpr(Int(12)), 1) ])
        ///         InterpolatedStringExpr([ Expr(FillExpr(Int(12)), 1) ], isTripleQuoted = true)
        ///         InterpolatedStringExpr([ Expr(FillExpr(Int(12)), 1) ], isVerbatim = true)
        ///         InterpolatedStringExpr([ Expr(FillExpr(Int(12)), 1) ], dollars = 2)
        ///
        ///     }
        /// }
        /// </code>
        static member InterpolatedStringExpr
            (parts: TextSegment seq, ?isTripleQuoted: bool, ?isVerbatim: bool, ?dollars: int)
            =
            Ast.BaseInterpolatedStringExpr(parts, isTripleQuoted, isVerbatim, dollars)

        /// <summary>
        /// Create an interpolated string expression.
        /// </summary>
        /// <param name="part">The parts of the interpolated string.</param>
        /// <param name="isTripleQuoted">Whether the string is triple-quoted (<c>$"""..."""</c>).</param>
        /// <param name="isVerbatim">Whether the string is verbatim (<c>$@"..."</c>). It cannot also be triple-quoted or have more than one <c>$</c>.</param>
        /// <param name="dollars">The number of dollars in the string.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InterpolatedStringExpr([ Expr(FillExpr(Int(12)), 1) ])
        ///         InterpolatedStringExpr([ Expr(FillExpr(Int(12)), 1) ], isTripleQuoted = true)
        ///         InterpolatedStringExpr([ Expr(FillExpr(Int(12)), 1) ], isVerbatim = true)
        ///         InterpolatedStringExpr([ Expr(FillExpr(Int(12)), 1) ], dollars = 2)
        ///     }
        /// }
        /// </code>
        static member InterpolatedStringExpr
            (part: TextSegment, ?isTripleQuoted: bool, ?isVerbatim: bool, ?dollars: int)
            =
            Ast.BaseInterpolatedStringExpr([ part ], isTripleQuoted, isVerbatim, dollars)

        /// <summary>
        /// Create an interpolated string expression.
        /// </summary>
        /// <param name="parts">The parts of the interpolated string.</param>
        /// <param name="isTripleQuoted">Whether the string is triple-quoted (<c>$"""..."""</c>).</param>
        /// <param name="isVerbatim">Whether the string is verbatim (<c>$@"..."</c>). It cannot also be triple-quoted or have more than one <c>$</c>.</param>
        /// <param name="dollars">The number of dollars in the string.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InterpolatedStringExpr([ ConstantExpr("12") ])
        ///         InterpolatedStringExpr([ ConstantExpr("12") ], isTripleQuoted = true)
        ///         InterpolatedStringExpr([ ConstantExpr("12") ], isVerbatim = true)
        ///         InterpolatedStringExpr([ ConstantExpr("12") ], dollars = 2)
        ///     }
        /// }
        /// </code>
        static member InterpolatedStringExpr
            (parts: WidgetBuilder<Expr> seq, ?isTripleQuoted: bool, ?isVerbatim: bool, ?dollars: int)
            =
            let parts = parts |> Seq.map(fun x -> TextSegment.Expr(Ast.FillExpr(x), 1))
            Ast.BaseInterpolatedStringExpr(parts, isTripleQuoted, isVerbatim, dollars)

        /// <summary>
        /// Create an interpolated string expression.
        /// </summary>
        /// <param name="part">The parts of the interpolated string.</param>
        /// <param name="isTripleQuoted">Whether the string is triple-quoted (<c>$"""..."""</c>).</param>
        /// <param name="isVerbatim">Whether the string is verbatim (<c>$@"..."</c>). It cannot also be triple-quoted or have more than one <c>$</c>.</param>
        /// <param name="dollars">The number of dollars in the string.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InterpolatedStringExpr(ConstantExpr("12"))
        ///         InterpolatedStringExpr(ConstantExpr("12"), isTripleQuoted = true)
        ///         InterpolatedStringExpr(ConstantExpr("12"), isVerbatim = true)
        ///         InterpolatedStringExpr(ConstantExpr("12"), dollars = 2)
        ///     }
        /// }
        /// </code>
        static member InterpolatedStringExpr
            (part: WidgetBuilder<Expr>, ?isTripleQuoted: bool, ?isVerbatim: bool, ?dollars: int)
            =
            Ast.BaseInterpolatedStringExpr(
                [ TextSegment.Expr(Ast.FillExpr(part), 1) ],
                isTripleQuoted,
                isVerbatim,
                dollars
            )

        /// <summary>
        /// Create an interpolated string expression.
        /// </summary>
        /// <param name="parts">The parts of the interpolated string.</param>
        /// <param name="isTripleQuoted">Whether the string is triple-quoted (<c>$"""..."""</c>).</param>
        /// <param name="isVerbatim">Whether the string is verbatim (<c>$@"..."</c>). It cannot also be triple-quoted or have more than one <c>$</c>.</param>
        /// <param name="dollars">The number of dollars in the string.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InterpolatedStringExpr([ Int(12) ])
        ///         InterpolatedStringExpr([ Int(12) ], isTripleQuoted = true)
        ///         InterpolatedStringExpr([ Int(12) ], isVerbatim = true)
        ///         InterpolatedStringExpr([ Int(12) ], dollars = 2)
        ///     }
        /// }
        /// </code>
        static member InterpolatedStringExpr
            (parts: WidgetBuilder<Constant> seq, ?isTripleQuoted: bool, ?isVerbatim: bool, ?dollars: int)
            =
            let parts = parts |> Seq.map(fun x -> TextSegment.Expr(Ast.FillExpr(x), 1))
            Ast.BaseInterpolatedStringExpr(parts, isTripleQuoted, isVerbatim, dollars)

        /// <summary>
        /// Create an interpolated string expression.
        /// </summary>
        /// <param name="part">The parts of the interpolated string.</param>
        /// <param name="isTripleQuoted">Whether the string is triple-quoted (<c>$"""..."""</c>).</param>
        /// <param name="isVerbatim">Whether the string is verbatim (<c>$@"..."</c>). It cannot also be triple-quoted or have more than one <c>$</c>.</param>
        /// <param name="dollars">The number of dollars in the string.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InterpolatedStringExpr(Int(12))
        ///         InterpolatedStringExpr(Int(12), isTripleQuoted = true)
        ///         InterpolatedStringExpr(Int(12), isVerbatim = true)
        ///         InterpolatedStringExpr(Int(12), dollars = 2)
        ///     }
        /// }
        /// </code>
        static member InterpolatedStringExpr
            (part: WidgetBuilder<Constant>, ?isTripleQuoted: bool, ?isVerbatim: bool, ?dollars: int)
            =
            Ast.BaseInterpolatedStringExpr(
                [ TextSegment.Expr(Ast.FillExpr(part), 1) ],
                isTripleQuoted,
                isVerbatim,
                dollars
            )

        /// <summary>
        /// Create an interpolated string expression.
        /// </summary>
        /// <param name="parts">The parts of the interpolated string.</param>
        /// <param name="isTripleQuoted">Whether the string is triple-quoted (<c>$"""..."""</c>).</param>
        /// <param name="isVerbatim">Whether the string is verbatim (<c>$@"..."</c>). It cannot also be triple-quoted or have more than one <c>$</c>.</param>
        /// <param name="dollars">The number of dollars in the string.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InterpolatedStringExpr([ "12" ])
        ///         InterpolatedStringExpr([ "12" ], isTripleQuoted = true)
        ///         InterpolatedStringExpr([ "12" ], isVerbatim = true)
        ///         InterpolatedStringExpr([ "12" ], dollars = 2)
        ///     }
        /// }
        /// </code>
        static member InterpolatedStringExpr
            (parts: string seq, ?isTripleQuoted: bool, ?isVerbatim: bool, ?dollars: int)
            =
            let parts = parts |> Seq.map(fun x -> TextSegment.Expr(Ast.FillExpr(x), 1))
            Ast.BaseInterpolatedStringExpr(parts, isTripleQuoted, isVerbatim, dollars)

        /// <summary>
        /// Create an interpolated string expression.
        /// </summary>
        /// <param name="part">The parts of the interpolated string.</param>
        /// <param name="isTripleQuoted">Whether the string is triple-quoted (<c>$"""..."""</c>).</param>
        /// <param name="isVerbatim">Whether the string is verbatim (<c>$@"..."</c>). It cannot also be triple-quoted or have more than one <c>$</c>.</param>
        /// <param name="dollars">The number of dollars in the string.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InterpolatedStringExpr("12")
        ///         InterpolatedStringExpr("12", isTripleQuoted = true)
        ///         InterpolatedStringExpr("12", isVerbatim = true)
        ///         InterpolatedStringExpr("12", dollars = 2)
        ///     }
        /// }
        /// </code>
        static member InterpolatedStringExpr(part: string, ?isTripleQuoted: bool, ?isVerbatim: bool, ?dollars: int) =
            Ast.BaseInterpolatedStringExpr([ TextSegment.Text(part) ], isTripleQuoted, isVerbatim, dollars)
