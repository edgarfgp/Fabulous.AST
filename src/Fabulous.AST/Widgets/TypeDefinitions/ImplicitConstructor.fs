namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections

module ImplicitConstructor =

    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let MultipleAttributes = Attributes.defineWidget "MultipleAttributes"

    let TypeParams = Attributes.defineScalar<string list> "TypeParams"

    let Parameters = Attributes.defineScalar<SimplePatNode list> "Parameters"

    let SimplePats = Attributes.defineWidgetCollection "SimplePats"

    let WidgetKey =
        Widgets.register "ImplicitConstructor" (fun widget ->
            let simplePatNodes =
                Helpers.getNodesFromWidgetCollection<SimplePatNode> widget SimplePats

            let simplePats =
                match simplePatNodes with
                | [] -> []
                | head :: tail ->
                    [ yield Choice1Of2 head
                      for p in tail do
                          yield Choice2Of2 SingleTextNode.comma
                          yield Choice1Of2 p ]

            let lines = Helpers.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            let attributes =
                Helpers.tryGetNodeFromWidget<AttributeListNode> widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | ValueSome values -> Some(MultipleAttributeListNode([ values ], Range.Zero))
                | ValueNone -> None

            ImplicitConstructorNode(
                xmlDocs,
                multipleAttributes,
                None,
                SingleTextNode.leftParenthesis,
                simplePats,
                SingleTextNode.rightParenthesis,
                None,
                Range.Zero
            ))

[<AutoOpen>]
module ImplicitConstructorBuilders =
    type Ast with
        static member ImplicitConstructor() =
            CollectionBuilder<ImplicitConstructorNode, SimplePatNode>(
                ImplicitConstructor.WidgetKey,
                ImplicitConstructor.SimplePats,
                AttributesBundle(StackList.empty(), ValueNone, ValueNone)
            )

[<Extension>]
type ImplicitConstructorModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<ImplicitConstructorNode>, xmlDocs: string list) =
        this.AddScalar(ImplicitConstructor.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes
        (
            this: WidgetBuilder<ImplicitConstructorNode>,
            attributes: WidgetBuilder<AttributeListNode>
        ) =
        this.AddWidget(ImplicitConstructor.MultipleAttributes.WithValue(attributes.Compile()))

    [<Extension>]
    static member inline attributes
        (
            this: WidgetBuilder<ImplicitConstructorNode>,
            attribute: WidgetBuilder<AttributeNode>
        ) =
        this.AddWidget(ImplicitConstructor.MultipleAttributes.WithValue((Ast.AttributeNodes() { attribute }).Compile()))


[<Extension>]
type ImplicitConstructorYieldExtensions =

    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<ImplicitConstructorNode, SimplePatNode>,
            x: SimplePatNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(x).Compile()
        { Widgets = MutStackArray1.One(widget) }
