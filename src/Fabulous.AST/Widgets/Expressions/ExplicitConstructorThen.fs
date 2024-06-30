namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ExplicitConstructorThen =
    let Value = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "ExplicitConstructorThen" (fun widget ->
            let expr = Widgets.getNodeFromWidget widget Value

            Expr.ExplicitConstructorThenExpr(
                ExprExplicitConstructorThenExpr(SingleTextNode.``then``, expr, Range.Zero)
            ))

[<AutoOpen>]
module ExplicitConstructorThenBuilders =
    type Ast with

        static member ExplicitConstructorThenExpr(value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                ExplicitConstructorThen.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| ExplicitConstructorThen.Value.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member ExplicitConstructorThenExpr(value: WidgetBuilder<Constant>) =
            Ast.ExplicitConstructorThenExpr(Ast.ConstantExpr(value))

        static member ExplicitConstructorThenExpr(value: string) =
            Ast.ExplicitConstructorThenExpr(Ast.Constant(value))
