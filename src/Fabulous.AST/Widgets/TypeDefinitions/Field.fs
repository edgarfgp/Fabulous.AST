namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module Field =
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let Name = Attributes.defineScalar<string> "Name"

    let FieldType = Attributes.defineScalar<Type> "FieldType"

    let MultipleAttributes = Attributes.defineWidget "MultipleAttributes"

    let WidgetKey =
        Widgets.register "Field" (fun widget ->
            let name = Helpers.tryGetScalarValue widget Name
            let lines = Helpers.tryGetScalarValue widget XmlDocs

            let name =
                match name with
                | ValueSome name -> Some(SingleTextNode.Create(name))
                | ValueNone -> None

            let fieldType = Helpers.getScalarValue widget FieldType

            let attributes =
                Helpers.tryGetNodeFromWidget<AttributeListNode> widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | ValueSome values -> Some(MultipleAttributeListNode([ values ], Range.Zero))
                | ValueNone -> None

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

        static member Field(name: string, filedType: Type) =
            WidgetBuilder<FieldNode>(
                Field.WidgetKey,
                AttributesBundle(
                    StackList.two(Field.Name.WithValue(name), Field.FieldType.WithValue(filedType)),
                    ValueNone,
                    ValueNone
                )
            )

        static member Field(filedType: Type) =
            WidgetBuilder<FieldNode>(
                Field.WidgetKey,
                AttributesBundle(StackList.one(Field.FieldType.WithValue(filedType)), ValueNone, ValueNone)
            )

        static member Field(name: string, filedType: string) =
            WidgetBuilder<FieldNode>(
                Field.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        Field.Name.WithValue(name),
                        Field.FieldType.WithValue(CommonType.mkLongIdent filedType)
                    ),
                    ValueNone,
                    ValueNone
                )
            )

[<Extension>]
type FieldModifiers =
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<FieldNode>, comments: string list) =
        this.AddScalar(Field.XmlDocs.WithValue(comments))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<FieldNode>, attributes: WidgetBuilder<AttributeListNode>) =
        this.AddWidget(Field.MultipleAttributes.WithValue(attributes.Compile()))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<FieldNode>, attribute: WidgetBuilder<AttributeNode>) =
        this.AddWidget(Field.MultipleAttributes.WithValue((Ast.AttributeNodes() { attribute }).Compile()))
