namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module QuoteExpr =
    let Value = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "QuoteExprPat" (fun widget ->
            let value = Widgets.getNodeFromWidget widget Value

            Pattern.QuoteExpr(
                ExprQuoteNode(SingleTextNode.leftQuotation, value, SingleTextNode.rightQuotation, Range.range0)
            ))

[<AutoOpen>]
module QuoteExprBuilders =
    type Ast with

        /// <summary>Create a quotation pattern, such as <c>&lt;@ 1 @&gt;</c>.</summary>
        /// <remarks>
        /// The F# parser accepts quotation patterns, but the compiler rejects every one of them with
        /// FS0731 ("This is not a valid pattern"), so the generated code never compiles.
        /// Match on a quotation with the active patterns in <c>FSharp.Quotations.Patterns</c> instead.
        /// </remarks>
        /// <param name="value">The quoted expression.</param>
        [<System.Obsolete("Quotation patterns are rejected by the F# compiler (FS0731). Match on quotations with the active patterns in FSharp.Quotations.Patterns instead.")>]
        static member QuoteExprPat(value: WidgetBuilder<Expr>) =
            WidgetBuilder<Pattern>(QuoteExpr.WidgetKey, QuoteExpr.Value.WithValue(value.Compile()))
