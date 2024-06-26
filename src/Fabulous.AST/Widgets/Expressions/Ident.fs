namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module Ident =
    let Identifier = Attributes.defineScalar<string> "Identifier"

    let WidgetKey =
        Widgets.register "Ident" (fun widget ->
            let identifier = Widgets.getScalarValue widget Identifier
            Expr.Ident(SingleTextNode.Create(identifier)))

[<AutoOpen>]
module IdentBuilders =
    type Ast with

        static member IdentExpr(identifier: string) =
            WidgetBuilder<Expr>(
                Ident.WidgetKey,
                AttributesBundle(StackList.one(Ident.Identifier.WithValue(identifier)), Array.empty, Array.empty)
            )
