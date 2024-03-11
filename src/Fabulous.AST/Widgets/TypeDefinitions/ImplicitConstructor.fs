namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections

module ImplicitConstructor =

    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let MultipleAttributes = Attributes.defineWidgetCollection "MultipleAttributes"

    let TypeParams = Attributes.defineScalar<string list> "TypeParams"

    let Parameters = Attributes.defineScalar<SimplePatNode list> "Parameters"

    let SimplePats = Attributes.defineWidgetCollection "SimplePats"

    let WidgetKey =
        Widgets.register "ImplicitConstructor" (fun widget ->
            let simplePatNodes =
                Widgets.getNodesFromWidgetCollection<SimplePatNode> widget SimplePats

            let simplePats =
                match simplePatNodes with
                | [] -> []
                | head :: tail ->
                    [ yield Choice1Of2 head
                      for p in tail do
                          yield Choice2Of2 SingleTextNode.comma
                          yield Choice1Of2 p ]

            let lines = Widgets.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            let attributes =
                Widgets.tryGetNodesFromWidgetCollection<AttributeNode> widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | Some values ->
                    Some(
                        MultipleAttributeListNode(
                            [ AttributeListNode(
                                  SingleTextNode.leftAttribute,
                                  values,
                                  SingleTextNode.rightAttribute,
                                  Range.Zero
                              ) ],
                            Range.Zero
                        )
                    )
                | None -> None


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
        static member Constructor() =
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
    static member inline attributes(this: WidgetBuilder<ImplicitConstructorNode>) =
        AttributeCollectionBuilder<ImplicitConstructorNode, AttributeNode>(this, ImplicitConstructor.MultipleAttributes)

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<ImplicitConstructorNode>, attributes: string list) =
        AttributeCollectionBuilder<ImplicitConstructorNode, AttributeNode>(this, ImplicitConstructor.MultipleAttributes) {
            for attribute in attributes do
                Ast.Attribute(attribute)
        }

    [<Extension>]
    static member inline attribute
        (this: WidgetBuilder<ImplicitConstructorNode>, attribute: WidgetBuilder<AttributeNode>)
        =
        AttributeCollectionBuilder<ImplicitConstructorNode, AttributeNode>(this, ImplicitConstructor.MultipleAttributes) {
            attribute
        }

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<ImplicitConstructorNode>, attribute: string) =
        AttributeCollectionBuilder<ImplicitConstructorNode, AttributeNode>(this, ImplicitConstructor.MultipleAttributes) {
            Ast.Attribute(attribute)
        }

[<Extension>]
type ImplicitConstructorYieldExtensions =

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<ImplicitConstructorNode, SimplePatNode>, x: SimplePatNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(x).Compile()
        { Widgets = MutStackArray1.One(widget) }
