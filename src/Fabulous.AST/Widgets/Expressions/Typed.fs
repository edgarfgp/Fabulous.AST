namespace Fabulous.AST

open Fabulous.AST
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
                    [| Typed.Value.WithValue(value.Compile()); Typed.Typed.WithValue(t.Compile()) |],
                    Array.empty
                )
            )

        static member TypedExpr(value: WidgetBuilder<Constant>, operator: string, t: WidgetBuilder<Type>) =
            Ast.TypedExpr(Ast.ConstantExpr(value), operator, t)

        static member TypedExpr(value: WidgetBuilder<Constant>, operator: string, t: string) =
            Ast.TypedExpr(value, operator, Ast.EscapeHatch(Type.Create(t)))

        static member TypedExpr(value: string, operator: string, t: WidgetBuilder<Type>) =
            Ast.TypedExpr(Ast.Constant(value), operator, t)

        static member TypedExpr(value: string, operator: string, t: string) =
            Ast.TypedExpr(value, operator, Ast.EscapeHatch(Type.Create(t)))
