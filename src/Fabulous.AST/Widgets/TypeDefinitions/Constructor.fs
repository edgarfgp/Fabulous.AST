namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections

module Constructor =

    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let TypeParams = Attributes.defineScalar<string list> "TypeParams"

    let Patterns = Attributes.defineWidget "Pattern"

    let WidgetKey =
        Widgets.register "ImplicitConstructor" (fun widget ->
            let lines = Widgets.tryGetScalarValue widget XmlDocs

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

            let pattern =
                Pattern.Unit(UnitNode(SingleTextNode.leftParenthesis, SingleTextNode.rightParenthesis, Range.Zero))

            let pattern =
                match Widgets.tryGetNodeFromWidget<Pattern> widget Patterns with
                | ValueSome pattern -> pattern
                | ValueNone -> pattern

            ImplicitConstructorNode(xmlDocs, multipleAttributes, None, pattern, None, Range.Zero))

[<AutoOpen>]
module ImplicitConstructorBuilders =
    type Ast with
        static member Constructor(pattern: WidgetBuilder<Pattern>) =
            WidgetBuilder<ImplicitConstructorNode>(
                Constructor.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| Constructor.Patterns.WithValue(pattern.Compile()) |],
                    Array.empty
                )
            )

        static member Constructor() =
            WidgetBuilder<ImplicitConstructorNode>(
                Constructor.WidgetKey,
                AttributesBundle(StackList.empty(), Array.empty, Array.empty)
            )

type ImplicitConstructorModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<ImplicitConstructorNode>, xmlDocs: string list) =
        this.AddScalar(Constructor.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<ImplicitConstructorNode>, attributes: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            Constructor.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<ImplicitConstructorNode>, attributes: string list) =
        ImplicitConstructorModifiers.attributes(
            this,
            [ for attr in attributes do
                  Ast.Attribute(attr) ]
        )

    [<Extension>]
    static member inline attribute
        (this: WidgetBuilder<ImplicitConstructorNode>, attribute: WidgetBuilder<AttributeNode>)
        =
        ImplicitConstructorModifiers.attributes(this, [ attribute ])

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<ImplicitConstructorNode>, attribute: string) =
        ImplicitConstructorModifiers.attributes(this, [ Ast.Attribute(attribute) ])
