namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module Field =
    let XmlDocs = Attributes.defineWidget "XmlDocs"

    let Name = Attributes.defineScalar<string> "Name"

    let FieldType = Attributes.defineWidget "FieldType"

    let Mutable = Attributes.defineScalar<bool> "Mutable"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let LeadingKeyword = Attributes.defineScalar<SingleTextNode> "LeadingKeyword"

    let WidgetKey =
        Widgets.register "Field" (fun widget ->
            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let name =
                Widgets.tryGetScalarValue widget Name
                |> ValueOption.map(fun x -> Some(SingleTextNode.Create(PrettyNaming.NormalizeIdentifierBackticks x)))
                |> ValueOption.defaultValue None

            let fieldType = Widgets.getNodeFromWidget widget FieldType

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let mutableKeyword =
                Widgets.tryGetScalarValue widget Mutable |> ValueOption.defaultValue false

            let mutableKeyword =
                if mutableKeyword then
                    Some(SingleTextNode.``mutable``)
                else
                    None

            let leadingKeyword =
                Widgets.tryGetScalarValue widget LeadingKeyword
                |> ValueOption.map(fun x -> Some(MultipleTextsNode.Create([ x ])))
                |> ValueOption.defaultValue None

            FieldNode(xmlDocs, attributes, leadingKeyword, mutableKeyword, None, name, fieldType, Range.Zero))

[<AutoOpen>]
module FieldBuilders =
    type Ast with
        static member Field(fieldType: WidgetBuilder<Type>) =
            WidgetBuilder<FieldNode>(Field.WidgetKey, Field.FieldType.WithValue(fieldType.Compile()))

        static member Field(fieldType: string) = Ast.Field(Ast.LongIdent(fieldType))

        static member Field(name: string, fieldType: WidgetBuilder<Type>) =
            WidgetBuilder<FieldNode>(
                Field.WidgetKey,
                AttributesBundle(
                    StackList.one(Field.Name.WithValue(name)),
                    [| Field.FieldType.WithValue(fieldType.Compile()) |],
                    Array.empty
                )
            )

        static member ValField(name: string, fieldType: WidgetBuilder<Type>) =
            WidgetBuilder<FieldNode>(
                Field.WidgetKey,
                AttributesBundle(
                    StackList.two(Field.Name.WithValue(name), Field.LeadingKeyword.WithValue(SingleTextNode.``val``)),
                    [| Field.FieldType.WithValue(fieldType.Compile()) |],
                    Array.empty
                )
            )

        static member ValField(name: string, fieldType: string) =
            Ast.ValField(name, Ast.LongIdent(fieldType))

        static member Field(name: string, fieldType: string) =
            Ast.Field(name, Ast.LongIdent(fieldType))

type FieldModifiers =
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<FieldNode>, comments: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(Field.XmlDocs.WithValue(comments.Compile()))

    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<FieldNode>, comments: string list) =
        FieldModifiers.xmlDocs(this, Ast.XmlDocs(comments))

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
    static member inline attribute(this: WidgetBuilder<FieldNode>, attribute: WidgetBuilder<AttributeNode>) =
        FieldModifiers.attributes(this, [ attribute ])
