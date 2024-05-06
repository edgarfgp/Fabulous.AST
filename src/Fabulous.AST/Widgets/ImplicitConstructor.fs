namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections

module ImplicitConstructor =

    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let TypeParams = Attributes.defineScalar<string list> "TypeParams"

    let Patterns = Attributes.defineWidget "Pattern"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let Alias = Attributes.defineScalar<string> "Alias"

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

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let alias =
                match Widgets.tryGetScalarValue widget Alias with
                | ValueNone -> None
                | ValueSome value ->
                    let value = PrettyNaming.NormalizeIdentifierBackticks value
                    Some(AsSelfIdentifierNode(SingleTextNode.``as``, SingleTextNode.Create(value), Range.Zero))

            ImplicitConstructorNode(xmlDocs, multipleAttributes, accessControl, pattern, alias, Range.Zero))

[<AutoOpen>]
module ImplicitConstructorBuilders =
    type Ast with
        static member ImplicitConstructor(pattern: WidgetBuilder<Pattern>) =
            WidgetBuilder<ImplicitConstructorNode>(
                ImplicitConstructor.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| ImplicitConstructor.Patterns.WithValue(pattern.Compile()) |],
                    Array.empty
                )
            )

        static member ImplicitConstructor(pattern: WidgetBuilder<Pattern>, alias: string) =
            WidgetBuilder<ImplicitConstructorNode>(
                ImplicitConstructor.WidgetKey,
                AttributesBundle(
                    StackList.one(ImplicitConstructor.Alias.WithValue(alias)),
                    [| ImplicitConstructor.Patterns.WithValue(pattern.Compile()) |],
                    Array.empty
                )
            )

        static member ImplicitConstructor() =
            WidgetBuilder<ImplicitConstructorNode>(
                ImplicitConstructor.WidgetKey,
                AttributesBundle(StackList.empty(), Array.empty, Array.empty)
            )

        static member ImplicitConstructor(alias: string) =
            WidgetBuilder<ImplicitConstructorNode>(
                ImplicitConstructor.WidgetKey,
                AttributesBundle(StackList.one(ImplicitConstructor.Alias.WithValue(alias)), Array.empty, Array.empty)
            )

type ImplicitConstructorModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<ImplicitConstructorNode>, xmlDocs: string list) =
        this.AddScalar(ImplicitConstructor.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<ImplicitConstructorNode>, attributes: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            ImplicitConstructor.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    [<Extension>]
    static member inline attribute
        (this: WidgetBuilder<ImplicitConstructorNode>, attribute: WidgetBuilder<AttributeNode>)
        =
        ImplicitConstructorModifiers.attributes(this, [ attribute ])

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<ImplicitConstructorNode>) =
        this.AddScalar(ImplicitConstructor.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<ImplicitConstructorNode>) =
        this.AddScalar(ImplicitConstructor.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<ImplicitConstructorNode>) =
        this.AddScalar(ImplicitConstructor.Accessibility.WithValue(AccessControl.Internal))
