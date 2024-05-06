namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module UnionCase =

    let Name = Attributes.defineScalar<string> "Name"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let Fields = Attributes.defineScalar<FieldNode list> "Fields"

    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let WidgetKey =
        Widgets.register "UnionCase" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name
                |> PrettyNaming.NormalizeIdentifierBackticks
                |> SingleTextNode.Create

            let fields = Widgets.tryGetScalarValue widget Fields

            let fields =
                match fields with
                | ValueSome fields -> fields
                | ValueNone -> []

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
                AttributesBundle(StackList.one(UnionCase.Name.WithValue(name)), Array.empty, Array.empty)
            )

        static member UnionCase(name: string, parameters: WidgetBuilder<FieldNode> list) =
            WidgetBuilder<UnionCaseNode>(
                UnionCase.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        UnionCase.Name.WithValue(name),
                        UnionCase.Fields.WithValue(parameters |> List.map Gen.mkOak)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member UnionCase(name: string, parameters: (string * string) list) =
            Ast.UnionCase(name, parameters |> List.map(Ast.Field))

        static member UnionCase(name: string, parameters: (string * WidgetBuilder<Type>) list) =
            Ast.UnionCase(name, parameters |> List.map(Ast.Field))

type UnionCaseModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<UnionCaseNode>, xmlDocs: string list) =
        this.AddScalar(UnionCase.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<UnionCaseNode>, attributes: WidgetBuilder<AttributeNode> list) =
        this.AddScalar(
            UnionCase.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<UnionCaseNode>, attribute: WidgetBuilder<AttributeNode>) =
        UnionCaseModifiers.attributes(this, [ attribute ])
