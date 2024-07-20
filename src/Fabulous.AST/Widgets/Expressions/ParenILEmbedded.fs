namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
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
            WidgetBuilder<Expr>(
                ParenILEmbedded.WidgetKey,
                AttributesBundle(
                    StackList.one(ParenILEmbedded.Identifier.WithValue(identifier)),
                    Array.empty,
                    Array.empty
                )
            )
