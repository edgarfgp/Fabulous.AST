namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak

module Typar =
    let Value = Attributes.defineScalar<string> "Typar"

    let WidgetKey =
        Widgets.register "Typar" (fun widget ->
            let value = Widgets.getScalarValue widget Value
            Expr.Typar(SingleTextNode.Create(value)))

[<AutoOpen>]
module TyparBuilders =
    type Ast with

        static member TyparExpr(value: string) =
            WidgetBuilder<Expr>(Typar.WidgetKey, Typar.Value.WithValue(value))
