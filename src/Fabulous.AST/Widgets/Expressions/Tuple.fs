namespace Fabulous.AST

open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Tuple =
    let Items = Attributes.defineWidgetCollection "Items"

    let WidgetKey =
        Widgets.register "Tuple" (fun widget ->
            let values = Helpers.getNodesFromWidgetCollection<Expr> widget Items

            let value =
                values
                |> List.map Choice1Of2
                |> List.intersperse(Choice2Of2(SingleTextNode.comma))

            Expr.Tuple(ExprTupleNode(value, Range.Zero)))

[<AutoOpen>]
module TupleBuilders =
    type Ast with

        static member TupleExpr() =
            CollectionBuilder<Expr, Expr>(Tuple.WidgetKey, Tuple.Items)
