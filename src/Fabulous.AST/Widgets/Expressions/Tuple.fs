namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Tuple =
    let Items = Attributes.defineScalar<Expr seq> "Items"

    let WidgetKey =
        Widgets.register "Tuple" (fun widget ->
            let values = Widgets.getScalarValue widget Items

            let value =
                values
                |> Seq.map Choice1Of2
                |> Seq.intersperse(Choice2Of2(SingleTextNode.comma))

            Expr.Tuple(ExprTupleNode(value, Range.Zero)))

[<AutoOpen>]
module TupleBuilders =
    type Ast with

        static member TupleExpr(value: WidgetBuilder<Expr> seq) =
            let parameters = value |> Seq.map Gen.mkOak

            WidgetBuilder<Expr>(Tuple.WidgetKey, Tuple.Items.WithValue(parameters))

        static member TupleExpr(value: WidgetBuilder<Constant> seq) =
            Ast.TupleExpr(value |> Seq.map Ast.ConstantExpr)

        static member TupleExpr(value: string seq) =
            Ast.TupleExpr(value |> Seq.map Ast.Constant)
