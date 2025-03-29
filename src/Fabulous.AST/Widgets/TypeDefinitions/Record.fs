namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module Record =

    let RecordCaseNode = Attributes.defineWidgetCollection "RecordCaseNode"

    let Name = Attributes.defineScalar<string> "Name"

    let Members = Attributes.defineWidgetCollection "Members"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let XmlDocs = Attributes.defineWidget "XmlDocs"

    let TypeParams = Attributes.defineWidget "TypeParams"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let IsRecursive = Attributes.defineScalar<bool> "IsRecursive"

    let WidgetKey =
        Widgets.register "Record" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

            let fields = Widgets.getNodesFromWidgetCollection<FieldNode> widget RecordCaseNode

            let members =
                Widgets.tryGetNodesFromWidgetCollection widget Members
                |> ValueOption.defaultValue []

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let typeParams =
                Widgets.tryGetNodeFromWidget widget TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let isRecursive =
                Widgets.tryGetScalarValue widget IsRecursive
                |> ValueOption.map(fun x ->
                    if x then
                        SingleTextNode.``and``
                    else
                        SingleTextNode.``type``)
                |> ValueOption.defaultValue SingleTextNode.``type``

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            TypeDefnRecordNode(
                TypeNameNode(
                    xmlDocs,
                    attributes,
                    isRecursive,
                    None,
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero),
                    typeParams,
                    [],
                    None,
                    Some(SingleTextNode.equals),
                    None,
                    Range.Zero
                ),
                accessControl,
                SingleTextNode.leftCurlyBrace,
                fields,
                SingleTextNode.rightCurlyBrace,
                members,
                Range.Zero
            ))

[<AutoOpen>]
module RecordBuilders =
    type Ast with
        /// <summary>Create a record type with the given name.</summary>
        /// <param name="name">The name of the record type.</param>
        /// <param name="isRecursive">Whether the record type is recursive.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Record("Point") {
        ///             Field("X", Float())
        ///             Field("Y", Float())
        ///             Field("Z", Float())
        ///         }
        ///     }
        /// }
        /// </code>
        static member Record(name: string) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name

            CollectionBuilder<TypeDefnRecordNode, FieldNode>(
                Record.WidgetKey,
                Record.RecordCaseNode,
                Record.Name.WithValue(name)
            )

type RecordModifiers =
    /// <summary>Sets the members for the current record definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         (Record("Point") {
    ///             Field("X", Float())
    ///             Field("Y", Float())
    ///             Field("Z", Float())
    ///         })
    ///             .members() {
    ///                 Member("X", Float(12)).toStatic()
    ///             }
    ///    }
    /// }
    /// </code>
    [<Extension>]
    static member inline members(this: WidgetBuilder<TypeDefnRecordNode>) =
        AttributeCollectionBuilder<TypeDefnRecordNode, MemberDefn>(this, Record.Members)

    /// <summary>Sets the XmlDocs for the current record definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Record("Point") {
    ///             Field("X", Float())
    ///             Field("Y", Float())
    ///             Field("Z", Float())
    ///         }
    ///         |> _.xmlDocs(Summary("This is a record type"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnRecordNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(Record.XmlDocs.WithValue(xmlDocs.Compile()))

    /// <summary>Sets the XmlDocs for the current record definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Record("Point") {
    ///             Field("X", Float())
    ///             Field("Y", Float())
    ///             Field("Z", Float())
    ///         }
    ///         |> _.xmlDocs([ "This is a record type" ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnRecordNode>, xmlDocs: string list) =
        RecordModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    /// <summary>Sets the attributes for the current record definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attributes">The attributes to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Record("Point") {
    ///             Field("X", Float())
    ///             Field("Y", Float())
    ///             Field("Z", Float())
    ///         }
    ///         |> _.attributes([ Attribute("Obsolete") ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<TypeDefnRecordNode>, attributes: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            Record.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    /// <summary>Sets the attributes for the current record definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attribute">The attributes to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Record("Point") {
    ///             Field("X", Float())
    ///             Field("Y", Float())
    ///             Field("Z", Float())
    ///         }
    ///         |> _.attribute(Attribute("Obsolete"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefnRecordNode>, attribute: WidgetBuilder<AttributeNode>) =
        RecordModifiers.attributes(this, [ attribute ])

    /// <summary>Sets the type parameters for the current record definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="typeParams">The type parameters to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Record("Point") {
    ///             Field("X", Float())
    ///             Field("Y", Float())
    ///             Field("Z", Float())
    ///         }
    ///         |> _.typeParams(PostfixList("'a"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline typeParams(this: WidgetBuilder<TypeDefnRecordNode>, typeParams: WidgetBuilder<TyparDecls>) =
        this.AddWidget(Record.TypeParams.WithValue(typeParams.Compile()))

    /// <summary>Sets the Record to be private.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Record("Point") {
    ///             Field("X", Float())
    ///             Field("Y", Float())
    ///             Field("Z", Float())
    ///         }
    ///         |> _.toPrivate()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<TypeDefnRecordNode>) =
        this.AddScalar(Record.Accessibility.WithValue(AccessControl.Private))

    /// <summary>Sets the Record to be public.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Record("Point") {
    ///             Field("X", Float())
    ///             Field("Y", Float())
    ///             Field("Z", Float())
    ///         }
    ///         |> _.toPublic()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<TypeDefnRecordNode>) =
        this.AddScalar(Record.Accessibility.WithValue(AccessControl.Public))

    /// <summary>Sets the Record to be internal.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Record("Point") {
    ///             Field("X", Float())
    ///             Field("Y", Float())
    ///             Field("Z", Float())
    ///         }
    ///         |> _.toInternal()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<TypeDefnRecordNode>) =
        this.AddScalar(Record.Accessibility.WithValue(AccessControl.Internal))

    /// <summary>Sets the Record to be recursive.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Record("Point") {
    ///             Field("X", Float())
    ///             Field("Y", Float())
    ///             Field("Z", Float())
    ///         }
    ///         |> _.toRecursive()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toRecursive(this: WidgetBuilder<TypeDefnRecordNode>) =
        this.AddScalar(Record.IsRecursive.WithValue(true))

type RecordYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: TypeDefnRecordNode) : CollectionContent =
        let typeDefn = TypeDefn.Record(x)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefnRecordNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        RecordYieldExtensions.Yield(this, node)
