namespace Fabulous.AST

open Fabulous.AST
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
            WidgetBuilder<Expr>(IndexRange.WidgetKey)

        static member IndexFromRangeExpr(fromExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(IndexRange.WidgetKey, IndexRange.FromExpr.WithValue(fromExpr.Compile()))

        static member IndexFromRangeExpr(fromExpr: WidgetBuilder<Constant>) =
            Ast.IndexFromRangeExpr(Ast.ConstantExpr(fromExpr))

        static member IndexFromRangeExpr(fromExpr: string) =
            Ast.IndexFromRangeExpr(Ast.Constant(fromExpr))

        static member IndexToRangeExpr(toExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(IndexRange.WidgetKey, IndexRange.ToExpr.WithValue(toExpr.Compile()))

        static member IndexToRangeExpr(toExpr: WidgetBuilder<Constant>) =
            Ast.IndexToRangeExpr(Ast.ConstantExpr(toExpr))

        static member IndexToRangeExpr(toExpr: string) =
            Ast.IndexToRangeExpr(Ast.Constant(toExpr))

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

        static member IndexRangeExpr(from: WidgetBuilder<Constant>, toExpr: WidgetBuilder<Expr>) =
            Ast.IndexRangeExpr(Ast.ConstantExpr(from), toExpr)

        static member IndexRangeExpr(from: string, toExpr: WidgetBuilder<Expr>) =
            Ast.IndexRangeExpr(Ast.Constant(from), toExpr)

        static member IndexRangeExpr(from: WidgetBuilder<Expr>, toExpr: WidgetBuilder<Constant>) =
            Ast.IndexRangeExpr(from, Ast.ConstantExpr(toExpr))

        static member IndexRangeExpr(from: WidgetBuilder<Constant>, toExpr: WidgetBuilder<Constant>) =
            Ast.IndexRangeExpr(Ast.ConstantExpr(from), Ast.ConstantExpr(toExpr))

        static member IndexRangeExpr(from: string, toExpr: string) =
            Ast.IndexRangeExpr(Ast.Constant(from), Ast.Constant(toExpr))
