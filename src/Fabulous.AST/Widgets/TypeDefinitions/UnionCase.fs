namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module UnionCase =

    let Name = Attributes.defineScalar<string> "Name"

    let MultipleAttributes = Attributes.defineWidgetCollection "MultipleAttributes"

    let Fields = Attributes.defineWidgetCollection "Fields"
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let WidgetKey =
        Widgets.register "UnionCase" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name
                |> StringParsing.normalizeIdentifierBackticks
                |> SingleTextNode.Create

            let fields = Widgets.tryGetNodesFromWidgetCollection<FieldNode> widget Fields

            let fields =
                match fields with
                | Some fields -> fields
                | None -> []

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

            let lines = Widgets.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            UnionCaseNode(xmlDocs, multipleAttributes, None, name, fields, Range.Zero))

[<AutoOpen>]
module UnionCaseBuilders =
    type Ast with
        static member UnionCase(name: string) =
            WidgetBuilder<UnionCaseNode>(
                UnionCase.WidgetKey,
                AttributesBundle(StackList.one(UnionCase.Name.WithValue(name)), ValueNone, ValueNone)
            )

        static member UnionParamsCase(name: string) =
            CollectionBuilder<UnionCaseNode, FieldNode>(
                UnionCase.WidgetKey,
                UnionCase.Fields,
                AttributesBundle(StackList.one(UnionCase.Name.WithValue(name)), ValueNone, ValueNone)
            )

[<Extension>]
type UnionCaseModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<UnionCaseNode>, xmlDocs: string list) =
        this.AddScalar(UnionCase.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<UnionCaseNode>) =
        AttributeCollectionBuilder<UnionCaseNode, AttributeNode>(this, UnionCase.MultipleAttributes)

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<UnionCaseNode>, attributes: string list) =
        AttributeCollectionBuilder<UnionCaseNode, AttributeNode>(this, UnionCase.MultipleAttributes) {
            for attribute in attributes do
                Ast.Attribute(attribute)
        }

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<UnionCaseNode>, attribute: WidgetBuilder<AttributeNode>) =
        AttributeCollectionBuilder<UnionCaseNode, AttributeNode>(this, UnionCase.MultipleAttributes) { attribute }

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<UnionCaseNode>, attribute: string) =
        AttributeCollectionBuilder<UnionCaseNode, AttributeNode>(this, UnionCase.MultipleAttributes) {
            Ast.Attribute(attribute)
        }
