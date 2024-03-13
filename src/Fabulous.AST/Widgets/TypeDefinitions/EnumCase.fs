namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module EnumCase =

    let Name = Attributes.defineScalar<string> "Name"

    let Value = Attributes.defineScalar<StringOrWidget<Expr>> "Value"

    let MultipleAttributes = Attributes.defineWidgetCollection "MultipleAttributes"

    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let HasQuotes = Attributes.defineScalar<bool> "HasQuotes"

    let WidgetKey =
        Widgets.register "EnumCase" (fun widget ->
            let name = Widgets.getScalarValue widget Name
            let value = Widgets.getScalarValue widget Value
            let lines = Widgets.tryGetScalarValue widget XmlDocs

            let hasQuotes =
                Widgets.tryGetScalarValue widget HasQuotes |> ValueOption.defaultValue true

            let value =
                match value with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(
                            SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value, hasQuotes))
                        )
                    )
                | StringOrWidget.WidgetExpr value -> value

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

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
                        EnumCase.Value.WithValue(StringOrWidget.StringExpr(value))
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
    static member inline hasQuotes(this: WidgetBuilder<EnumCaseNode>, value: bool) =
        this.AddScalar(EnumCase.HasQuotes.WithValue(value))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<EnumCaseNode>) =
        AttributeCollectionBuilder<EnumCaseNode, AttributeNode>(this, EnumCase.MultipleAttributes)

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<EnumCaseNode>, attributes: string list) =
        AttributeCollectionBuilder<EnumCaseNode, AttributeNode>(this, EnumCase.MultipleAttributes) {
            for attribute in attributes do
                Ast.Attribute(attribute)
        }

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<EnumCaseNode>, attribute: WidgetBuilder<AttributeNode>) =
        AttributeCollectionBuilder<EnumCaseNode, AttributeNode>(this, EnumCase.MultipleAttributes) { attribute }

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<EnumCaseNode>, attribute: string) =
        AttributeCollectionBuilder<EnumCaseNode, AttributeNode>(this, EnumCase.MultipleAttributes) {
            Ast.Attribute(attribute)
        }
