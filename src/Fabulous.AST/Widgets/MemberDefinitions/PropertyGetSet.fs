namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module PropertyGetSetMember =
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"
    let Identifier = Attributes.defineScalar<string> "Identifier"
    let GetterWidget = Attributes.defineWidget "GetterWidget"
    let SetterWidget = Attributes.defineWidget "SetterWidget"
    let IsInlined = Attributes.defineScalar<bool> "IsInlined"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let IsStatic = Attributes.defineScalar<bool> "IsStatic"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let WidgetKey =
        Widgets.register "PropertyGetSetMember" (fun widget ->
            let identifier = Widgets.getScalarValue widget Identifier

            let isInlined =
                Widgets.tryGetScalarValue widget IsInlined |> ValueOption.defaultValue false

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let isStatic =
                Widgets.tryGetScalarValue widget IsStatic |> ValueOption.defaultValue false

            let lines = Widgets.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            let multipleTextsNode =
                MultipleTextsNode(
                    (if isStatic then
                         [ SingleTextNode.``static``; SingleTextNode.``member`` ]
                     else
                         [ SingleTextNode.``member`` ]),
                    Range.Zero
                )

            let inlinedNode = if isInlined then Some SingleTextNode.``inline`` else None

            let memberName =
                IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(identifier)) ], Range.Zero)

            let getterWidget =
                Widgets.getNodeFromWidget<PropertyGetSetBindingNode> widget GetterWidget

            let setterWidget =
                Widgets.tryGetNodeFromWidget<PropertyGetSetBindingNode> widget SetterWidget
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let andKeyword =
                if setterWidget.IsSome then
                    Some SingleTextNode.``and``
                else
                    None

            MemberDefnPropertyGetSetNode(
                xmlDocs,
                attributes,
                multipleTextsNode,
                inlinedNode,
                accessControl,
                memberName,
                SingleTextNode.``with``,
                getterWidget,
                andKeyword,
                setterWidget,
                Range.Zero
            ))

[<AutoOpen>]
module PropertyGetSetMemberMemberBuilders =
    type Ast with

        static member Property(identifier: string, getter: WidgetBuilder<PropertyGetSetBindingNode>) =
            WidgetBuilder<MemberDefnPropertyGetSetNode>(
                PropertyGetSetMember.WidgetKey,
                AttributesBundle(
                    StackList.one(PropertyGetSetMember.Identifier.WithValue(identifier)),
                    [| PropertyGetSetMember.GetterWidget.WithValue(getter.Compile()) |],
                    Array.empty
                )
            )

        static member Property
            (
                identifier: string,
                getter: WidgetBuilder<PropertyGetSetBindingNode>,
                setter: WidgetBuilder<PropertyGetSetBindingNode>
            ) =
            WidgetBuilder<MemberDefnPropertyGetSetNode>(
                PropertyGetSetMember.WidgetKey,
                AttributesBundle(
                    StackList.one(PropertyGetSetMember.Identifier.WithValue(identifier)),
                    [| PropertyGetSetMember.GetterWidget.WithValue(getter.Compile())
                       PropertyGetSetMember.SetterWidget.WithValue(setter.Compile()) |],
                    Array.empty
                )
            )

type PropertyGetSetMemberModifiers =
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<MemberDefnPropertyGetSetNode>, values: string list) =
        this.AddScalar(PropertyGetSetMember.XmlDocs.WithValue(values))

    [<Extension>]
    static member attributes
        (this: WidgetBuilder<MemberDefnPropertyGetSetNode>, values: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            PropertyGetSetMember.MultipleAttributes.WithValue(
                [ for vals in values do
                      Gen.mkOak vals ]
            )
        )

    [<Extension>]
    static member attribute(this: WidgetBuilder<MemberDefnPropertyGetSetNode>, value: WidgetBuilder<AttributeNode>) =
        PropertyGetSetMemberModifiers.attributes(this, [ value ])

    [<Extension>]
    static member inline toStatic(this: WidgetBuilder<MemberDefnPropertyGetSetNode>) =
        this.AddScalar(PropertyGetSetMember.IsStatic.WithValue(true))

    [<Extension>]
    static member inline toInlined(this: WidgetBuilder<MemberDefnPropertyGetSetNode>) =
        this.AddScalar(PropertyGetSetMember.IsInlined.WithValue(true))

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<MemberDefnPropertyGetSetNode>) =
        this.AddScalar(PropertyGetSetMember.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<MemberDefnPropertyGetSetNode>) =
        this.AddScalar(PropertyGetSetMember.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<MemberDefnPropertyGetSetNode>) =
        this.AddScalar(PropertyGetSetMember.Accessibility.WithValue(AccessControl.Internal))
