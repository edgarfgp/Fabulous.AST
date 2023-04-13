namespace Fabulous.AST

open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module Field =

    let Name = Attributes.defineWidget "Name"

    let FieldType = Attributes.defineScalar<Type> "FieldType"

    let WidgetKey =
        Widgets.register "Field" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name
            let fieldType = Helpers.getScalarValue widget FieldType
            FieldNode(None, None, None, false, None, Some name, fieldType, Range.Zero))

[<AutoOpen>]
module FieldBuilders =
    type Ast with

        static member inline Field(name: WidgetBuilder<#SingleTextNode>, filedType: Type) =
            WidgetBuilder<FieldNode>(
                Field.WidgetKey,
                AttributesBundle(
                    StackList.one(Field.FieldType.WithValue(filedType)),
                    ValueSome [| Field.Name.WithValue(name.Compile()) |],
                    ValueNone
                )
            )

        static member inline Field(name: SingleTextNode, fieldType: Type) =
            Ast.Field(Ast.EscapeHatch(name), fieldType)

        static member inline Field(name: string, fieldType: Type) =
            Ast.Field(SingleTextNode(name, Range.Zero), fieldType)
