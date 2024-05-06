namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module IndexRange =
    let FromExpr = Attributes.defineWidget "FromExpr"

    let ToExpr = Attributes.defineWidget "ToExpr"

    let WidgetKey =
        Widgets.register "IndexRange" (fun widget ->
            let fromExpr =
                Widgets.tryGetNodeFromWidget widget FromExpr
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let toExpr =
                Widgets.tryGetNodeFromWidget widget ToExpr
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            Expr.IndexRange(ExprIndexRangeNode(fromExpr, SingleTextNode.Create(".."), toExpr, Range.Zero)))

[<AutoOpen>]
module IndexRangeBuilders =
    type Ast with

        static member IndexRangeExpr() =
            WidgetBuilder<Expr>(IndexRange.WidgetKey, AttributesBundle(StackList.empty(), Array.empty, Array.empty))

        static member IndexFromRangeExpr(fromExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                IndexRange.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| IndexRange.FromExpr.WithValue(fromExpr.Compile()) |],
                    Array.empty
                )
            )

        static member IndexToRangeExpr(toExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                IndexRange.WidgetKey,
                AttributesBundle(StackList.empty(), [| IndexRange.ToExpr.WithValue(toExpr.Compile()) |], Array.empty)
            )

        static member IndexRangeExpr(from: WidgetBuilder<Expr>, toExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                IndexRange.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| IndexRange.FromExpr.WithValue(from.Compile())
                       IndexRange.ToExpr.WithValue(toExpr.Compile()) |],
                    Array.empty
                )
            )
