namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Typed =
    let Value = Attributes.defineWidget "Value"

    let Typed = Attributes.defineScalar<struct (string * Type)> "TypedValue"

    let WidgetKey =
        Widgets.register "Typed" (fun widget ->
            let expr = Helpers.getNodeFromWidget<Expr> widget Value
            let struct (text, typ) = Helpers.getScalarValue widget Typed
            Expr.Typed(ExprTypedNode(expr, text, typ, Range.Zero)))

[<AutoOpen>]
module TypedBuilders =
    type Ast with

        static member TypedExpr(value: WidgetBuilder<Expr>, operator: string, t: Type) =
            WidgetBuilder<Expr>(
                Typed.WidgetKey,
                AttributesBundle(
                    StackList.one(Typed.Typed.WithValue(operator, t)),
                    ValueSome [| Typed.Value.WithValue(value.Compile()) |],
                    ValueNone
                )
            )
