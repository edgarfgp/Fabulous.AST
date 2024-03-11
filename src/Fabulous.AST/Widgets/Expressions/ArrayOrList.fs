namespace Fabulous.AST

open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ArrayOrList =
    let Items = Attributes.defineWidgetCollection "Items"

    let OpeningNode = Attributes.defineScalar<SingleTextNode> "OpeningNode"

    let ClosingNode = Attributes.defineScalar<SingleTextNode> "ClosingNode"

    let WidgetKey =
        Widgets.register "ArrayOrList" (fun widget ->
            let values = Widgets.getNodesFromWidgetCollection<Expr> widget Items
            let openNode = Widgets.getScalarValue widget OpeningNode
            let closeNode = Widgets.getScalarValue widget ClosingNode
            Expr.ArrayOrList(ExprArrayOrListNode(openNode, values, closeNode, Range.Zero)))

[<AutoOpen>]
module ArrayOrListBuilders =
    type Ast with

        static member ListExpr() =
            CollectionBuilder<Expr, Expr>(
                ArrayOrList.WidgetKey,
                ArrayOrList.Items,
                ArrayOrList.OpeningNode.WithValue(SingleTextNode.leftBracket),
                ArrayOrList.ClosingNode.WithValue(SingleTextNode.rightBracket)
            )

        static member ArrayExpr() =
            CollectionBuilder<Expr, Expr>(
                ArrayOrList.WidgetKey,
                ArrayOrList.Items,
                ArrayOrList.OpeningNode.WithValue(SingleTextNode.leftArray),
                ArrayOrList.ClosingNode.WithValue(SingleTextNode.rightArray)
            )
