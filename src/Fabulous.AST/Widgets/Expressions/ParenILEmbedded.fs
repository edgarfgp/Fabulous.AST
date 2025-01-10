namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak

module ParenILEmbedded =
    let Identifier = Attributes.defineScalar<string> "Identifier"

    let WidgetKey =
        Widgets.register "ParenILEmbedded" (fun widget ->
            let identifier = Widgets.getScalarValue widget Identifier
            Expr.Ident(SingleTextNode.Create(identifier)))

[<AutoOpen>]
module ParenILEmbeddedBuilders =
    type Ast with

        static member ParenILEmbeddedExpr(identifier: string) =
            WidgetBuilder<Expr>(ParenILEmbedded.WidgetKey, ParenILEmbedded.Identifier.WithValue(identifier))
