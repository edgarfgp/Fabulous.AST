namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Lazy =
    let Value = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "Lazy" (fun widget ->
            let expr = Helpers.getNodeFromWidget<Expr> widget Value
            Expr.Lazy(ExprLazyNode(SingleTextNode.``lazy``, expr, Range.Zero)))

[<AutoOpen>]
module LazyBuilders =
    type Ast with

        static member LazyExpr(value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                Lazy.WidgetKey,
                AttributesBundle(StackList.empty(), ValueSome [| Lazy.Value.WithValue(value.Compile()) |], ValueNone)
            )
