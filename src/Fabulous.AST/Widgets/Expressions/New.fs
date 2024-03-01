namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module New =
    let Value = Attributes.defineWidget "Value"

    let Type = Attributes.defineWidget "Type"

    let WidgetKey =
        Widgets.register "New" (fun widget ->
            let expr = Helpers.getNodeFromWidget<Expr> widget Value
            let typ = Helpers.getNodeFromWidget widget Type
            Expr.New(ExprNewNode(SingleTextNode.``new``, typ, expr, Range.Zero)))

[<AutoOpen>]
module NewBuilders =
    type Ast with

        static member NewExpr(t: WidgetBuilder<Type>, value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                New.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| New.Value.WithValue(value.Compile()); New.Type.WithValue(t.Compile()) |],
                    ValueNone
                )
            )

        static member NewExpr(t: string, value: WidgetBuilder<Expr>) =
            Ast.NewExpr(Ast.TypeLongIdent(t), value)
