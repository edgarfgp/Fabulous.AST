namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Quoted =
    let Value = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "Quoted" (fun widget ->
            let expr = Widgets.getNodeFromWidget widget Value
            Expr.Quote(ExprQuoteNode(SingleTextNode.leftQuotation, expr, SingleTextNode.rightQuotation, Range.range0)))

    let UntypedWidgetKey =
        Widgets.register "UntypedQuoted" (fun widget ->
            let expr = Widgets.getNodeFromWidget widget Value

            Expr.Quote(
                ExprQuoteNode(
                    SingleTextNode.leftUntypedQuotation,
                    expr,
                    SingleTextNode.rightUntypedQuotation,
                    Range.range0
                )
            ))

[<AutoOpen>]
module QuotedBuilders =
    type Ast with

        static member QuotedExpr(value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(Quoted.WidgetKey, Quoted.Value.WithValue(value.Compile()))

        static member QuotedExpr(value: WidgetBuilder<Constant>) = Ast.QuotedExpr(Ast.ConstantExpr(value))

        static member QuotedExpr(value: string) = Ast.QuotedExpr(Ast.Constant(value))

        /// <summary>Create an untyped code quotation (<c>&lt;@@ 1 + 1 @@&gt;</c>), of type <c>Expr</c> rather than <c>Expr&lt;'T&gt;</c>.</summary>
        /// <param name="value">The quoted expression.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         UntypedQuotedExpr(InfixAppExpr(Int(1), "+", Int(1)))
        ///     }
        /// }
        /// </code>
        static member UntypedQuotedExpr(value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(Quoted.UntypedWidgetKey, Quoted.Value.WithValue(value.Compile()))

        /// <summary>Create an untyped code quotation (<c>&lt;@@ 1 @@&gt;</c>).</summary>
        /// <param name="value">The quoted expression.</param>
        static member UntypedQuotedExpr(value: WidgetBuilder<Constant>) =
            Ast.UntypedQuotedExpr(Ast.ConstantExpr(value))

        /// <summary>Create an untyped code quotation (<c>&lt;@@ x @@&gt;</c>).</summary>
        /// <param name="value">The quoted expression.</param>
        static member UntypedQuotedExpr(value: string) =
            Ast.UntypedQuotedExpr(Ast.Constant(value))
