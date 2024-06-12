namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module IndexFromEnd =
    let Value = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "IndexFromEnd" (fun widget ->
            let expr = Widgets.getNodeFromWidget widget Value
            Expr.IndexFromEnd(ExprIndexFromEndNode(expr, Range.Zero)))

[<AutoOpen>]
module IndexFromEndBuilders =
    type Ast with

        static member IndexFromEndExpr(value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                IndexFromEnd.WidgetKey,
                AttributesBundle(StackList.empty(), [| IndexFromEnd.Value.WithValue(value.Compile()) |], Array.empty)
            )

        static member IndexFromEndExpr(value: WidgetBuilder<Constant>) =
            Ast.IndexFromEndExpr(Ast.ConstantExpr(value))

        static member IndexFromEndExpr(value: string) =
            Ast.IndexFromEndExpr(Ast.Constant(value))
