namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Tuple =
    let Items = Attributes.defineScalar<Expr list> "Items"

    let WidgetKey =
        Widgets.register "Tuple" (fun widget ->
            let values = Widgets.getScalarValue widget Items

            let value =
                values
                |> List.map Choice1Of2
                |> List.intersperse(Choice2Of2(SingleTextNode.comma))

            Expr.Tuple(ExprTupleNode(value, Range.Zero)))

[<AutoOpen>]
module TupleBuilders =
    type Ast with

        static member TupleExpr(value: WidgetBuilder<Expr> list) =
            let parameters = value |> List.map Gen.mkOak

            WidgetBuilder<Expr>(Tuple.WidgetKey, Tuple.Items.WithValue(parameters))

        static member TupleExpr(value: WidgetBuilder<Constant> list) =
            Ast.TupleExpr(value |> List.map Ast.ConstantExpr)

        static member TupleExpr(value: string list) =
            Ast.TupleExpr(value |> List.map Ast.Constant)
