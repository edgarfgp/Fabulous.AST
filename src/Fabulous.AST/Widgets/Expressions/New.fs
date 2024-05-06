namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module New =
    let Value = Attributes.defineWidget "Value"

    let TypeVal = Attributes.defineWidget "Type"

    let WidgetKey =
        Widgets.register "New" (fun widget ->
            let expr = Widgets.getNodeFromWidget widget Value
            let typ = Widgets.getNodeFromWidget widget TypeVal
            Expr.New(ExprNewNode(SingleTextNode.``new``, typ, expr, Range.Zero)))

[<AutoOpen>]
module NewBuilders =
    type Ast with

        static member NewExpr(t: WidgetBuilder<Type>, value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                New.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| New.Value.WithValue(value.Compile()); New.TypeVal.WithValue(t.Compile()) |],
                    Array.empty
                )
            )
