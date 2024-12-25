namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fabulous.AST.WidgetDefinitionStore
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module PropertyGetSetMember =
    let XmlDocs = Attributes.defineWidget "XmlDocs"
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

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget XmlDocs
                |> ValueOption.map(fun x -> Some(x))
                |> ValueOption.defaultValue None

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

        /// <summary>Create a property with a getter.</summary>
        /// <param name="identifier">The name of the property.</param>
        /// <param name="getter">The getter for the property.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             Member(
        ///                 "this.Name",
        ///                 Getter(ConstantExpr("_name"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
        static member Member(identifier: string, getter: WidgetBuilder<PropertyGetSetBindingNode>) =
            WidgetBuilder<MemberDefnPropertyGetSetNode>(
                PropertyGetSetMember.WidgetKey,
                AttributesBundle(
                    StackList.one(PropertyGetSetMember.Identifier.WithValue(identifier)),
                    [| PropertyGetSetMember.FirstBindingWidget.WithValue(getter.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>Create a property with a getter and setter.</summary>
        /// <param name="identifier">The name of the property.</param>
        /// <param name="getter">The getter for the property.</param>
        /// <param name="setter">The setter for the property.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             Member(
        ///                 "this.Name",
        ///                 Getter(ConstantExpr("_name")),
        ///                 Setter(NamedPat("value"), ConstantExpr("_name &lt;- value"))
        ///             )
        ///         }
        ///     }
        /// }
        /// </code>
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
    /// <summary>Sets the XmlDocs for the current PropertyGetSetMember widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("Person", UnitPat()) {
    ///             Member(
    ///                 "this.Name",
    ///                 Getter(ConstantExpr("_name"))
    ///             )
    ///             .xmlDocs(Summary([ "This is a property" ]))
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<MemberDefnPropertyGetSetNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(PropertyGetSetMember.XmlDocs.WithValue(xmlDocs.Compile()))

    /// <summary>Sets the XmlDocs for the current PropertyGetSetMember widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The comments to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("Person", UnitPat()) {
    ///             Member(
    ///                 "this.Name",
    ///                 Getter(ConstantExpr("_name"))
    ///             )
    ///             .xmlDocs([ "This is a property" ])
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<MemberDefnPropertyGetSetNode>, xmlDocs: string list) =
        PropertyGetSetMemberModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    /// <summary>Sets the attributes for the current PropertyGetSetMember widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="values">The attributes to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("Person", UnitPat()) {
    ///             Member(
    ///                 "this.Name",
    ///                 Getter(ConstantExpr("_name"))
    ///             )
    ///             .attributes([ Attribute("Obsolete") ])
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member attributes
        (this: WidgetBuilder<MemberDefnPropertyGetSetNode>, values: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(PropertyGetSetMember.MultipleAttributes.WithValue(values |> List.map Gen.mkOak))

    /// <summary>Sets the attributes for the current PropertyGetSetMember widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="value">The attribute to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("Person", UnitPat()) {
    ///             Member(
    ///                 "this.Name",
    ///                 Getter(ConstantExpr("_name"))
    ///             )
    ///             .attribute(Attribute("Obsolete"))
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member attribute(this: WidgetBuilder<MemberDefnPropertyGetSetNode>, value: WidgetBuilder<AttributeNode>) =
        PropertyGetSetMemberModifiers.attributes(this, [ value ])

    /// <summary>Sets the static modifier for the current PropertyGetSetMember widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("Person", UnitPat()) {
    ///             Member(
    ///                 "this.Name",
    ///                 Getter(ConstantExpr("_name"))
    ///             )
    ///             .toStatic()
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toStatic(this: WidgetBuilder<MemberDefnPropertyGetSetNode>) =
        this.AddScalar(PropertyGetSetMember.IsStatic.WithValue(true))

    /// <summary>Sets the inlined modifier for the current PropertyGetSetMember widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("Person", UnitPat()) {
    ///             Member(
    ///                 "this.Name",
    ///                 Getter(ConstantExpr("_name"))
    ///             )
    ///             .toInlined()
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toInlined(this: WidgetBuilder<MemberDefnPropertyGetSetNode>) =
        this.AddScalar(PropertyGetSetMember.IsInlined.WithValue(true))

    /// <summary>Sets the private accessibility modifier for the current PropertyGetSetMember widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("Person", UnitPat()) {
    ///             Member(
    ///                 "this.Name",
    ///                 Getter(ConstantExpr("_name"))
    ///             )
    ///             .toPrivate()
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<MemberDefnPropertyGetSetNode>) =
        this.AddScalar(PropertyGetSetMember.Accessibility.WithValue(AccessControl.Private))

    /// <summary>Sets the public accessibility modifier for the current PropertyGetSetMember widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("Person", UnitPat()) {
    ///             Member(
    ///                 "this.Name",
    ///                 Getter(ConstantExpr("_name"))
    ///             )
    ///             .toPublic()
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<MemberDefnPropertyGetSetNode>) =
        this.AddScalar(PropertyGetSetMember.Accessibility.WithValue(AccessControl.Public))

    /// <summary>Sets the internal accessibility modifier for the current PropertyGetSetMember widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("Person", UnitPat()) {
    ///             Member(
    ///                 "this.Name",
    ///                 Getter(ConstantExpr("_name"))
    ///             )
    ///             .toInternal()
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<MemberDefnPropertyGetSetNode>) =
        this.AddScalar(PropertyGetSetMember.Accessibility.WithValue(AccessControl.Internal))
