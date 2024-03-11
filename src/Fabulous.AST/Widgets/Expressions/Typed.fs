namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Typed =
    let Value = Attributes.defineWidget "Value"

    let Typed = Attributes.defineWidget "TypedValue"

    let Text = Attributes.defineScalar "Text"

    let WidgetKey =
        Widgets.register "Typed" (fun widget ->
            let expr = Widgets.getNodeFromWidget<Expr> widget Value
            let typed = Widgets.getNodeFromWidget widget Typed
            let text = Widgets.getScalarValue widget Text
            Expr.Typed(ExprTypedNode(expr, text, typed, Range.Zero)))

[<AutoOpen>]
module TypedBuilders =
    type Ast with

        static member TypedExpr(value: WidgetBuilder<Expr>, operator: string, t: WidgetBuilder<Type>) =
            WidgetBuilder<Expr>(
                Typed.WidgetKey,
                AttributesBundle(
                    StackList.one(Typed.Text.WithValue(operator)),
                    ValueSome [| Typed.Value.WithValue(value.Compile()); Typed.Typed.WithValue(t.Compile()) |],
                    ValueNone
                )
            )
