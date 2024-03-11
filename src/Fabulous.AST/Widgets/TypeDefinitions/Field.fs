namespace Fabulous.AST

open System
open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module Field =
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let Name = Attributes.defineScalar<string> "Name"

    let FieldType = Attributes.defineWidget "FieldType"

    let MultipleAttributes = Attributes.defineWidgetCollection "MultipleAttributes"

    let WidgetKey =
        Widgets.register "Field" (fun widget ->
            let name = Helpers.tryGetScalarValue widget Name
            let lines = Helpers.tryGetScalarValue widget XmlDocs

            let name =
                match name with
                | ValueSome name -> name |> Helpers.normalizeIdentifierBackticks |> SingleTextNode.Create |> Some
                | ValueNone -> None

            let fieldType = Helpers.getNodeFromWidget widget FieldType

            let attributes =
                Helpers.tryGetNodesFromWidgetCollection<AttributeNode> widget MultipleAttributes

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

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            FieldNode(xmlDocs, multipleAttributes, None, false, None, name, fieldType, Range.Zero))

[<AutoOpen>]
module FieldBuilders =
    type Ast with

        static member Field(name: string, filedType: WidgetBuilder<Type>) =
            WidgetBuilder<FieldNode>(
                Field.WidgetKey,
                AttributesBundle(
                    StackList.one(Field.Name.WithValue(name)),
                    ValueSome [| Field.FieldType.WithValue(filedType.Compile()) |],
                    ValueNone
                )
            )

        static member Field(filedType: WidgetBuilder<Type>) =
            WidgetBuilder<FieldNode>(
                Field.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| Field.FieldType.WithValue(filedType.Compile()) |],
                    ValueNone
                )
            )

        static member Field(name: string, filedType: string) =
            WidgetBuilder<FieldNode>(
                Field.WidgetKey,
                AttributesBundle(
                    StackList.one(Field.Name.WithValue(name)),
                    ValueSome [| Field.FieldType.WithValue(Ast.LongIdent(filedType).Compile()) |],
                    ValueNone
                )
            )

[<Extension>]
type FieldModifiers =
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<FieldNode>, comments: string list) =
        this.AddScalar(Field.XmlDocs.WithValue(comments))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<FieldNode>) =
        AttributeCollectionBuilder<FieldNode, AttributeNode>(this, Field.MultipleAttributes)

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<FieldNode>, attributes: string list) =
        AttributeCollectionBuilder<FieldNode, AttributeNode>(this, Field.MultipleAttributes) {
            for attribute in attributes do
                Ast.Attribute(attribute)
        }

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<FieldNode>, attribute: WidgetBuilder<AttributeNode>) =
        AttributeCollectionBuilder<FieldNode, AttributeNode>(this, Field.MultipleAttributes) { attribute }

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<FieldNode>, attribute: string) =
        AttributeCollectionBuilder<FieldNode, AttributeNode>(this, Field.MultipleAttributes) {
            Ast.Attribute(attribute)
        }
