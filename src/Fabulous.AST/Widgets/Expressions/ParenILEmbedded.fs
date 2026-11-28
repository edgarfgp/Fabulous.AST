namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak

module ParenILEmbedded =
    let Identifier = Attributes.defineScalar<string> "Identifier"

    let WidgetKey =
        Widgets.register "ParenILEmbedded" (fun widget ->
            let text = (Widgets.getScalarValue widget Identifier).Trim()

            // Accept either the full `(# ... #)` form or just the IL inside it.
            let text =
                if text.StartsWith("(#") && text.EndsWith("#)") then
                    text
                else
                    $"(# %s{text} #)"

            Expr.ParenILEmbedded(SingleTextNode.Create(text)))

[<AutoOpen>]
module ParenILEmbeddedBuilders =
    type Ast with

        /// <summary>
        /// Create an inline IL expression (<c>(# "ldnull" : 'T #)</c>). The text is emitted as is, wrapped in
        /// <c>(# ... #)</c> unless it already has them. Inline IL only compiles inside FSharp.Core
        /// (elsewhere it is an error unless warning 42 is suppressed), so it is mainly useful for generating that code.
        /// </summary>
        /// <param name="value">The IL, with or without the surrounding <c>(# ... #)</c>.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Value("x", ParenILEmbeddedExpr("\"ldnull\" : 'T"))
        ///     }
        /// }
        /// </code>
        static member ParenILEmbeddedExpr(value: string) =
            WidgetBuilder<Expr>(ParenILEmbedded.WidgetKey, ParenILEmbedded.Identifier.WithValue(value))
