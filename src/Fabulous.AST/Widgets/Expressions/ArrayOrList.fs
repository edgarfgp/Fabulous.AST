namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ArrayOrList =
    let Items = Attributes.defineScalar<Expr list> "Items"

    let OpeningNode = Attributes.defineScalar<SingleTextNode> "OpeningNode"

    let ClosingNode = Attributes.defineScalar<SingleTextNode> "ClosingNode"

    let WidgetKey =
        Widgets.register "ArrayOrList" (fun widget ->
            let values = Widgets.getScalarValue widget Items
            let openNode = Widgets.getScalarValue widget OpeningNode
            let closeNode = Widgets.getScalarValue widget ClosingNode
            Expr.ArrayOrList(ExprArrayOrListNode(openNode, values, closeNode, Range.Zero)))

[<AutoOpen>]
module ArrayOrListBuilders =
    type Ast with

        static member ListExpr(value: WidgetBuilder<Expr> list) =
            let parameters = value |> List.map Gen.mkOak

            WidgetBuilder<Expr>(
                ArrayOrList.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        ArrayOrList.Items.WithValue(parameters),
                        ArrayOrList.OpeningNode.WithValue(SingleTextNode.leftBracket),
                        ArrayOrList.ClosingNode.WithValue(SingleTextNode.rightBracket)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member ArrayExpr(value: WidgetBuilder<Expr> list) =
            let parameters = value |> List.map Gen.mkOak

            WidgetBuilder<Expr>(
                ArrayOrList.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        ArrayOrList.Items.WithValue(parameters),
                        ArrayOrList.OpeningNode.WithValue(SingleTextNode.leftArray),
                        ArrayOrList.ClosingNode.WithValue(SingleTextNode.rightArray)
                    ),
                    Array.empty,
                    Array.empty
                )
            )
