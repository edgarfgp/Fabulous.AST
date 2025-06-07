namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module StructTuple =
    let Items = Attributes.defineScalar<Expr seq> "Items"

    let WidgetKey =
        Widgets.register "StructTuple" (fun widget ->
            let values = Widgets.getScalarValue widget Items

            let values =
                values
                |> Seq.map Choice1Of2
                |> Seq.intersperse(Choice2Of2(SingleTextNode.comma))

            Expr.StructTuple(
                ExprStructTupleNode(
                    SingleTextNode.``struct``,
                    ExprTupleNode(values, Range.Zero),
                    SingleTextNode.rightParenthesis,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module StructTupleBuilders =
    type Ast with

        static member StructTupleExpr(value: WidgetBuilder<Expr> seq) =
            let parameters = value |> Seq.map Gen.mkOak

            WidgetBuilder<Expr>(StructTuple.WidgetKey, StructTuple.Items.WithValue(parameters))

        static member StructTupleExpr(value: WidgetBuilder<Constant> seq) =
            Ast.StructTupleExpr(value |> Seq.map Ast.ConstantExpr)

        static member StructTupleExpr(value: string seq) =
            Ast.StructTupleExpr(value |> Seq.map Ast.Constant)
