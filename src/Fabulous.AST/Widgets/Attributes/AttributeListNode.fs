namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module AttributeListNode =
    let AttributeNodes = Attributes.defineWidgetCollection "AttributeNodes"

    let WidgetKey =
        Widgets.register "AttributeNode" (fun widget ->
            let attributeNodes =
                Helpers.getNodesFromWidgetCollection<AttributeNode> widget AttributeNodes

            AttributeListNode(SingleTextNode.leftAttribute, attributeNodes, SingleTextNode.rightAttribute, Range.Zero))


[<AutoOpen>]
module AttributeListNodeBuilders =
    type Ast with
        static member AttributeNodes() =
            CollectionBuilder<AttributeListNode, AttributeNode>(
                AttributeListNode.WidgetKey,
                AttributeListNode.AttributeNodes
            )

        static member AttributeNodes(node: WidgetBuilder<AttributeNode>) = Ast.AttributeNodes() { node }

[<Extension>]
type AttributeListYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<AttributeListNode, AttributeNode>,
            x: WidgetBuilder<AttributeNode>
        ) : CollectionContent =
        { Widgets = MutStackArray1.One(x.Compile()) }
