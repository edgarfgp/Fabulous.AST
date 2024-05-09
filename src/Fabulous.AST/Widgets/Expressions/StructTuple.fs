namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module StructTuple =
    let Items = Attributes.defineScalar<Expr list> "Items"

    let WidgetKey =
        Widgets.register "StructTuple" (fun widget ->
            let values = Widgets.getScalarValue widget Items

            let values =
                values
                |> List.map Choice1Of2
                |> List.intersperse(Choice2Of2(SingleTextNode.comma))

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

        static member StructTupleExpr(value: WidgetBuilder<Expr> list) =
            let parameters = value |> List.map Gen.mkOak

            WidgetBuilder<Expr>(
                StructTuple.WidgetKey,
                AttributesBundle(StackList.one(StructTuple.Items.WithValue(parameters)), Array.empty, Array.empty)
            )

        static member StructTupleExpr(value: WidgetBuilder<Constant> list) =
            Ast.StructTupleExpr(value |> List.map Ast.ConstantExpr)

        static member StructTupleExpr(value: string list) =
            Ast.StructTupleExpr(value |> List.map Ast.Constant)
