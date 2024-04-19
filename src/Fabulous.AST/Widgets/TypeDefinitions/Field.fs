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

    let Mutable = Attributes.defineScalar<bool> "Mutable"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let WidgetKey =
        Widgets.register "Field" (fun widget ->
            let name = Widgets.getScalarValue widget Name
            let lines = Widgets.tryGetScalarValue widget XmlDocs

            let name =
                Unquoted(name)
                |> StringParsing.normalizeIdentifierBackticks
                |> SingleTextNode.Create
                |> Some

            let fieldType = Widgets.getNodeFromWidget widget FieldType

            let attributes = Widgets.tryGetScalarValue widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | ValueSome values ->
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
                | ValueNone -> None

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            let mutableKeyword =
                Widgets.tryGetScalarValue widget Mutable |> ValueOption.defaultValue false

            let mutableKeyword =
                if mutableKeyword then
                    Some(SingleTextNode.``mutable``)
                else
                    None

            FieldNode(xmlDocs, multipleAttributes, None, mutableKeyword, None, name, fieldType, Range.Zero))

[<AutoOpen>]
module FieldBuilders =
    type Ast with

        static member Field(name: string, filedType: WidgetBuilder<Type>) =
            WidgetBuilder<FieldNode>(
                Field.WidgetKey,
                AttributesBundle(
                    StackList.one(Field.Name.WithValue(name)),
                    [| Field.FieldType.WithValue(filedType.Compile()) |],
                    Array.empty
                )
            )

        static member Field(name: string, filedType: string) =
            WidgetBuilder<FieldNode>(
                Field.WidgetKey,
                AttributesBundle(
                    StackList.one(Field.Name.WithValue(name)),
                    [| Field.FieldType.WithValue(Ast.LongIdent(filedType).Compile()) |],
                    Array.empty
                )
            )

type FieldModifiers =
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<FieldNode>, comments: string list) =
        this.AddScalar(Field.XmlDocs.WithValue(comments))

    [<Extension>]
    static member toMutable(this: WidgetBuilder<FieldNode>) =
        this.AddScalar(Field.Mutable.WithValue(true))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<FieldNode>, attributes: WidgetBuilder<AttributeNode> list) =
        this.AddScalar(
            Field.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<FieldNode>, attributes: string list) =
        FieldModifiers.attributes(
            this,
            [ for attr in attributes do
                  Ast.Attribute(attr) ]
        )

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<FieldNode>, attribute: WidgetBuilder<AttributeNode>) =
        FieldModifiers.attributes(this, [ attribute ])

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<FieldNode>, attribute: string) =
        FieldModifiers.attributes(this, [ Ast.Attribute(attribute) ])
