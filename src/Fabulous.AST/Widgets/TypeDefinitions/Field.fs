namespace Fabulous.AST

open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module Field =

    let Name = Attributes.defineWidget "SingleTextNode"

    let FieldType = Attributes.defineScalar<Type> "SingleTextNode"

    let WidgetKey =
        Widgets.register "Field" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name
            let fieldType = Helpers.getScalarValue widget FieldType
            FieldNode(None, None, None, false, None, Some name, fieldType, Range.Zero))

[<AutoOpen>]
module FieldBuilders =
    type Fabulous.AST.Ast with

        static member inline Field(name: WidgetBuilder<#SingleTextNode>, filedType: Type) =
            WidgetBuilder<FieldNode>(
                Field.WidgetKey,
                AttributesBundle(
                    StackList.one(Field.FieldType.WithValue(filedType)),
                    ValueSome [| Field.Name.WithValue(name.Compile()) |],
                    ValueNone
                )
            )

        static member inline Field(name: SingleTextNode, fieldType: SingleTextNode) =
            Ast.Field(Ast.EscapeHatch(name), Type.Var(fieldType))

        static member inline Field(name: string, fieldType: string) =
            Ast.Field(SingleTextNode(name, Range.Zero), SingleTextNode(fieldType, Range.Zero))
