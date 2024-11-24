namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module PropertyGetSetMember =
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"
    let Identifier = Attributes.defineScalar<string> "Identifier"
    let FirstBindingWidget = Attributes.defineWidget "GetterWidget"
    let LastBindingWidget = Attributes.defineWidget "SetterWidget"
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

            let firstBinding =
                Widgets.tryGetNodeFromWidget<PropertyGetSetBindingNode> widget FirstBindingWidget

            let firstBinding =
                match firstBinding with
                | ValueSome value -> value
                | ValueNone -> failwith "Getter is required"

            let lastBindingWidget =
                Widgets.tryGetNodeFromWidget<PropertyGetSetBindingNode> widget LastBindingWidget
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let andKeyword =
                if lastBindingWidget.IsSome then
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
                firstBinding,
                andKeyword,
                lastBindingWidget,
                Range.Zero
            ))

[<AutoOpen>]
module PropertyGetSetMemberMemberBuilders =
    type Ast with

        static member Member(identifier: string, first: WidgetBuilder<PropertyGetSetBindingNode>) =
            WidgetBuilder<MemberDefnPropertyGetSetNode>(
                PropertyGetSetMember.WidgetKey,
                AttributesBundle(
                    StackList.one(PropertyGetSetMember.Identifier.WithValue(identifier)),
                    [| PropertyGetSetMember.FirstBindingWidget.WithValue(first.Compile()) |],
                    Array.empty
                )
            )

        static member Member
            (
                identifier: string,
                getter: WidgetBuilder<PropertyGetSetBindingNode>,
                setter: WidgetBuilder<PropertyGetSetBindingNode>
            ) =
            WidgetBuilder<MemberDefnPropertyGetSetNode>(
                PropertyGetSetMember.WidgetKey,
                AttributesBundle(
                    StackList.one(PropertyGetSetMember.Identifier.WithValue(identifier)),
                    [| PropertyGetSetMember.FirstBindingWidget.WithValue(getter.Compile())
                       PropertyGetSetMember.LastBindingWidget.WithValue(setter.Compile()) |],
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

    [<Extension>]
    static member inline getter
        (this: WidgetBuilder<MemberDefnPropertyGetSetNode>, getter: WidgetBuilder<PropertyGetSetBindingNode>)
        =
        this.AddWidget(PropertyGetSetMember.FirstBindingWidget.WithValue(getter.Compile()))

    [<Extension>]
    static member inline getter(this: WidgetBuilder<MemberDefnPropertyGetSetNode>, getter: WidgetBuilder<Expr>) =
        PropertyGetSetMemberModifiers.getter(this, Ast.Getter(getter))

    [<Extension>]
    static member inline getter(this: WidgetBuilder<MemberDefnPropertyGetSetNode>, getter: string) =
        PropertyGetSetMemberModifiers.getter(this, Ast.Getter(getter))

    [<Extension>]
    static member inline setter
        (this: WidgetBuilder<MemberDefnPropertyGetSetNode>, setter: WidgetBuilder<PropertyGetSetBindingNode>)
        =
        this.AddWidget(PropertyGetSetMember.LastBindingWidget.WithValue(setter.Compile()))

    [<Extension>]
    static member inline setter(this: WidgetBuilder<MemberDefnPropertyGetSetNode>, setter: WidgetBuilder<Expr>) =
        PropertyGetSetMemberModifiers.setter(this, Ast.Setter(setter))

    [<Extension>]
    static member inline setter(this: WidgetBuilder<MemberDefnPropertyGetSetNode>, setter: string) =
        PropertyGetSetMemberModifiers.setter(this, Ast.Setter(setter))
