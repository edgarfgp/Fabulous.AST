namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module Field =

    let Name = Attributes.defineScalar<string> "Name"

    let FieldType = Attributes.defineScalar<Type> "FieldType"

    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"

    let WidgetKey =
        Widgets.register "Field" (fun widget ->
            let name = Helpers.tryGetScalarValue widget Name

            let name =
                match name with
                | ValueSome name -> Some(SingleTextNode.Create(name))
                | ValueNone -> None

            let fieldType = Helpers.getScalarValue widget FieldType
            let attributes = Helpers.tryGetScalarValue widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | ValueSome values -> MultipleAttributeListNode.Create values |> Some
                | ValueNone -> None

            FieldNode(None, multipleAttributes, None, false, None, name, fieldType, Range.Zero))

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
    static member inline attributes(this: WidgetBuilder<FieldNode>, attributes: string list) =
        this.AddScalar(Field.MultipleAttributes.WithValue(attributes))
