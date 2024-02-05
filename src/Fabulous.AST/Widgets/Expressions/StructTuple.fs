namespace Fabulous.AST

open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module StructTuple =
    let Items = Attributes.defineWidgetCollection "Items"

    let WidgetKey =
        Widgets.register "StructTuple" (fun widget ->
            let values = Helpers.getNodesFromWidgetCollection<Expr> widget Items

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

        static member StructTupleExpr() =
            CollectionBuilder<Expr, Expr>(StructTuple.WidgetKey, StructTuple.Items)
