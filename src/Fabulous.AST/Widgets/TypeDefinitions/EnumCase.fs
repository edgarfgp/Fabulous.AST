namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module EnumCase =

    let Name = Attributes.defineScalar<string> "Name"

    let Value = Attributes.defineScalar<StringOrWidget<Expr>> "Value"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let WidgetKey =
        Widgets.register "EnumCase" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name
                |> Unquoted
                |> StringParsing.normalizeIdentifierQuotes

            let value = Widgets.getScalarValue widget Value
            let lines = Widgets.tryGetScalarValue widget XmlDocs

            let value =
                match value with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(Constant.FromText(SingleTextNode.Create(value.Normalize())))
                | StringOrWidget.WidgetExpr value -> value

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

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

            EnumCaseNode(
                xmlDocs,
                None,
                multipleAttributes,
                SingleTextNode.Create(name),
                SingleTextNode.equals,
                value,
                Range.Zero
            ))

[<AutoOpen>]
module EnumCaseBuilders =
    type Ast with

        static member EnumCase(name: string, value: WidgetBuilder<Expr>) =
            WidgetBuilder<EnumCaseNode>(
                EnumCase.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        EnumCase.Name.WithValue(name),
                        EnumCase.Value.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak value))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member EnumCase(name: string, value: string) =
            WidgetBuilder<EnumCaseNode>(
                EnumCase.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        EnumCase.Name.WithValue(name),
                        EnumCase.Value.WithValue(StringOrWidget.StringExpr(Unquoted(value)))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

[<Extension>]
type EnumCaseModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<EnumCaseNode>, xmlDocs: string list) =
        this.AddScalar(EnumCase.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<EnumCaseNode>, values: WidgetBuilder<AttributeNode> list) =
        this.AddScalar(
            EnumCase.MultipleAttributes.WithValue(
                [ for vals in values do
                      Gen.mkOak vals ]
            )
        )

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<EnumCaseNode>, attributes: string list) =
        EnumCaseModifiers.attributes(
            this,
            [ for attribute in attributes do
                  Ast.Attribute(attribute) ]
        )

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<EnumCaseNode>, attribute: WidgetBuilder<AttributeNode>) =
        EnumCaseModifiers.attributes(this, [ attribute ])

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<EnumCaseNode>, attribute: string) =
        EnumCaseModifiers.attributes(this, [ Ast.Attribute(attribute) ])
