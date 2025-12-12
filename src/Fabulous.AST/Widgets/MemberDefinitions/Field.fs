namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module Field =
    let XmlDocs = Attributes.defineWidget "XmlDocs"

    let Name = Attributes.defineScalar<string> "Name"

    let FieldType = Attributes.defineWidget "FieldType"

    let Mutable = Attributes.defineScalar<bool> "Mutable"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode seq> "MultipleAttributes"

    let LeadingKeyword = Attributes.defineScalar<SingleTextNode> "LeadingKeyword"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let WidgetKey =
        Widgets.register "Field" (fun widget ->
            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let name =
                Widgets.tryGetScalarValue widget Name
                |> ValueOption.map(fun x -> Some(SingleTextNode.Create(PrettyNaming.NormalizeIdentifierBackticks x)))
                |> ValueOption.defaultValue None

            let fieldType = Widgets.getNodeFromWidget widget FieldType

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let mutableKeyword =
                Widgets.tryGetScalarValue widget Mutable |> ValueOption.defaultValue false

            let mutableKeyword =
                if mutableKeyword then
                    Some(SingleTextNode.``mutable``)
                else
                    None

            let leadingKeyword =
                Widgets.tryGetScalarValue widget LeadingKeyword
                |> ValueOption.map(fun x -> Some(MultipleTextsNode.Create([ x ])))
                |> ValueOption.defaultValue None

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            FieldNode(xmlDocs, attributes, leadingKeyword, mutableKeyword, accessControl, name, fieldType, Range.Zero))

    let ValFieldWidgetKey =
        Widgets.register "ValField" (fun widget ->
            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget MemberDefn.XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let name =
                Widgets.tryGetScalarValue widget Name
                |> ValueOption.map(fun x -> Some(SingleTextNode.Create(PrettyNaming.NormalizeIdentifierBackticks x)))
                |> ValueOption.defaultValue None

            let fieldType = Widgets.getNodeFromWidget widget FieldType

            let attributes =
                Widgets.tryGetScalarValue widget MemberDefn.MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let mutableKeyword =
                Widgets.tryGetScalarValue widget MemberDefn.IsMutable
                |> ValueOption.defaultValue false

            let mutableKeyword =
                if mutableKeyword then
                    Some(SingleTextNode.``mutable``)
                else
                    None

            let leadingKeyword =
                Widgets.tryGetScalarValue widget LeadingKeyword
                |> ValueOption.map(fun x -> Some(MultipleTextsNode.Create([ x ])))
                |> ValueOption.defaultValue None

            let accessControl =
                Widgets.tryGetScalarValue widget MemberDefn.Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let node =
                FieldNode(
                    xmlDocs,
                    attributes,
                    leadingKeyword,
                    mutableKeyword,
                    accessControl,
                    name,
                    fieldType,
                    Range.Zero
                )

            MemberDefn.ValField(node))

[<AutoOpen>]
module FieldBuilders =
    type Ast with
        /// <summary>
        /// Create a field with the give type.
        /// </summary>
        /// <param name="fieldType">The type of the field.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Record("HEX") {
        ///            Field(Int())
        ///            Field(Int())
        ///            Field(Int())
        ///        }
        ///     }
        /// }
        /// </code>
        static member Field(fieldType: WidgetBuilder<Type>) =
            WidgetBuilder<FieldNode>(Field.WidgetKey, Field.FieldType.WithValue(fieldType.Compile()))

        /// <summary>
        /// Create a field with the give type.
        /// </summary>
        /// <param name="fieldType">The type of the field.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Record("HEX") {
        ///             Field("int")
        ///             Field("int")
        ///             Field("int")
        ///         }
        ///     }
        /// }
        /// </code>
        static member Field(fieldType: string) = Ast.Field(Ast.LongIdent(fieldType))

        /// <summary>
        /// Create a field with the give a name and type.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="fieldType">The type of the field.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Record("HEX") {
        ///             Field("R", Int())
        ///             Field("G", Int())
        ///             Field("B", Int())
        ///         }
        ///     }
        /// }
        /// </code>
        static member Field(name: string, fieldType: WidgetBuilder<Type>) =
            WidgetBuilder<FieldNode>(
                Field.WidgetKey,
                AttributesBundle(
                    StackList.one(Field.Name.WithValue(name)),
                    [| Field.FieldType.WithValue(fieldType.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>
        /// Create a field with the give a name and type.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="fieldType">The type of the field.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Record("HEX") {
        ///             Field("R", "int")
        ///             Field("G", "int")
        ///             Field("B", "int")
        ///         }
        ///     }
        /// }
        /// </code>
        static member Field(name: string, fieldType: string) =
            Ast.Field(name, Ast.LongIdent(fieldType))

        /// <summary>
        /// Create a Val field with the give a name and type.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="fieldType">The type of the field.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("X") {
        ///            ValField("x", Int())
        ///        }
        ///     }
        /// }
        /// </code>
        static member ValField(name: string, fieldType: WidgetBuilder<Type>) =
            WidgetBuilder<MemberDefn>(
                Field.ValFieldWidgetKey,
                AttributesBundle(
                    StackList.two(Field.Name.WithValue(name), Field.LeadingKeyword.WithValue(SingleTextNode.``val``)),
                    [| Field.FieldType.WithValue(fieldType.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>
        /// Create a Val field with the give a name and type.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="fieldType">The type of the field.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("X") {
        ///             ValField("x", "int")
        ///         }
        ///     }
        /// }
        /// </code>
        static member ValField(name: string, fieldType: string) =
            Ast.ValField(name, Ast.LongIdent(fieldType))

type FieldModifiers =
    /// <summary>Sets the XmlDocs for the current widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("X") {
    ///             ValField("x", "int")
    ///                 .xmlDocs(Summary("This is a field"))
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<FieldNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(Field.XmlDocs.WithValue(xmlDocs.Compile()))

    /// <summary>
    /// Sets the XmlDocs for the current widget.
    /// </summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("X") {
    ///             ValField("x", "int")
    ///                 .xmlDocs(["This is a field"])
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<FieldNode>, xmlDocs: string seq) =
        FieldModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    /// <summary>Sets the ValField to be mutable.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("X") {
    ///             ValField("x", "int")
    ///                 .toMutable()
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member toMutable(this: WidgetBuilder<FieldNode>) =
        this.AddScalar(Field.Mutable.WithValue(true))

    /// <summary>Sets the attributes for the current measure ValField definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attributes">The attributes to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("X") {
    ///             ValField("x", "int")
    ///                 .attributes([ Attribute "DefaultValue" ])
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attributes(this: WidgetBuilder<FieldNode>, attributes: WidgetBuilder<AttributeNode> seq) =
        this.AddScalar(Field.MultipleAttributes.WithValue(attributes |> Seq.map Gen.mkOak))

    /// <summary>Sets the attributes for the current measure ValField definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attribute">The attributes to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("X") {
    ///             ValField("x", "int")
    ///                 .attributes(Attribute("DefaultValue"))
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attribute(this: WidgetBuilder<FieldNode>, attribute: WidgetBuilder<AttributeNode>) =
        FieldModifiers.attributes(this, [ attribute ])

    /// <summary>Sets the field to be private.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Record("Person") {
    ///             Field("name", String()).toPrivate()
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<FieldNode>) =
        this.AddScalar(Field.Accessibility.WithValue(AccessControl.Private))

    /// <summary>Sets the field to be public.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Record("Person") {
    ///             Field("name", String()).toPublic()
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<FieldNode>) =
        this.AddScalar(Field.Accessibility.WithValue(AccessControl.Public))

    /// <summary>Sets the field to be internal.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Record("Person") {
    ///             Field("name", String()).toInternal()
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<FieldNode>) =
        this.AddScalar(Field.Accessibility.WithValue(AccessControl.Internal))
