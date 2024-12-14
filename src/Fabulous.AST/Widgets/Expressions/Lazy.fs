namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Lazy =
    let Value = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "Lazy" (fun widget ->
            let expr = Widgets.getNodeFromWidget widget Value
            Expr.Lazy(ExprLazyNode(SingleTextNode.``lazy``, expr, Range.Zero)))

[<AutoOpen>]
module LazyBuilders =
    type Ast with

        static member LazyExpr(value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(Lazy.WidgetKey, Lazy.Value.WithValue(value.Compile()))

        static member LazyExpr(value: WidgetBuilder<Constant>) = Ast.LazyExpr(Ast.ConstantExpr(value))

        static member LazyExpr(value: string) = Ast.LazyExpr(Ast.Constant(value))
