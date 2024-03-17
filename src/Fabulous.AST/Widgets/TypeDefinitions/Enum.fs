namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module Enum =

    let EnumCaseNode = Attributes.defineWidgetCollection "UnionCaseNode"

    let Name = Attributes.defineScalar<string> "Name"

    let MultipleAttributes = Attributes.defineWidgetCollection "MultipleAttributes"

    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let WidgetKey =
        Widgets.register "Enum" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name
                |> Unquoted
                |> StringParsing.normalizeIdentifierBackticks

            let enumCaseNodes =
                Widgets.getNodesFromWidgetCollection<EnumCaseNode> widget EnumCaseNode

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

            TypeDefnEnumNode(
                TypeNameNode(
                    xmlDocs,
                    multipleAttributes,
                    SingleTextNode.``type``,
                    Some(SingleTextNode.Create(name)),
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.equals) ], Range.Zero),
                    None,
                    [],
                    None,
                    None,
                    None,
                    Range.Zero
                ),
                enumCaseNodes,
                [],
                Range.Zero
            ))

[<AutoOpen>]
module EnumBuilders =
    type Ast with

        static member Enum(name: string) =
            CollectionBuilder<TypeDefnEnumNode, EnumCaseNode>(
                Enum.WidgetKey,
                Enum.EnumCaseNode,
                AttributesBundle(StackList.one(Enum.Name.WithValue(name)), Array.empty, Array.empty)
            )

[<Extension>]
type EnumModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnEnumNode>, xmlDocs: string list) =
        this.AddScalar(Enum.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<TypeDefnEnumNode>) =
        AttributeCollectionBuilder<TypeDefnEnumNode, AttributeNode>(this, Enum.MultipleAttributes)

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<TypeDefnEnumNode>, attributes: string list) =
        AttributeCollectionBuilder<TypeDefnEnumNode, AttributeNode>(this, Enum.MultipleAttributes) {
            for attribute in attributes do
                Ast.Attribute(attribute)
        }

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefnEnumNode>, attribute: WidgetBuilder<AttributeNode>) =
        AttributeCollectionBuilder<TypeDefnEnumNode, AttributeNode>(this, Enum.MultipleAttributes) { attribute }

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefnEnumNode>, attribute: string) =
        AttributeCollectionBuilder<TypeDefnEnumNode, AttributeNode>(this, Enum.MultipleAttributes) {
            Ast.Attribute(attribute)
        }

[<Extension>]
type EnumYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefnEnumNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let typeDefn = TypeDefn.Enum(node)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<TypeDefnEnumNode, EnumCaseNode>, x: EnumCaseNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(x).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<TypeDefnEnumNode, EnumCaseNode>, x: WidgetBuilder<EnumCaseNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        EnumYieldExtensions.Yield(this, node)
