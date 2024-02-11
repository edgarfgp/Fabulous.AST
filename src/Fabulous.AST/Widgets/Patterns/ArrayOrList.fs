namespace Fabulous.AST

open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ArrayOrListPat =

    let OpenTextNode = Attributes.defineScalar<SingleTextNode> "OpenTextNode"

    let CloseTextNode = Attributes.defineScalar<SingleTextNode> "CloseTextNode"

    let Parameters = Attributes.defineWidgetCollection "Parameters"

    let WidgetKey =
        Widgets.register "ArrayOrList" (fun widget ->
            let openTextNode = Helpers.getScalarValue widget OpenTextNode
            let values = Helpers.getNodesFromWidgetCollection<Pattern> widget Parameters
            let closeTextNode = Helpers.getScalarValue widget CloseTextNode

            Pattern.ArrayOrList(PatArrayOrListNode(openTextNode, values, closeTextNode, Range.Zero)))

[<AutoOpen>]
module ArrayOrListPatBuilders =
    type Ast with

        static member ListPat() =
            CollectionBuilder<Pattern, Pattern>(
                ArrayOrListPat.WidgetKey,
                ArrayOrListPat.Parameters,
                ArrayOrListPat.OpenTextNode.WithValue(SingleTextNode.leftBracket),
                ArrayOrListPat.CloseTextNode.WithValue(SingleTextNode.rightBracket)
            )

        static member ArrayPat() =
            CollectionBuilder<Pattern, Pattern>(
                ArrayOrListPat.WidgetKey,
                ArrayOrListPat.Parameters,
                ArrayOrListPat.OpenTextNode.WithValue(SingleTextNode.leftArray),
                ArrayOrListPat.CloseTextNode.WithValue(SingleTextNode.rightArray)
            )
