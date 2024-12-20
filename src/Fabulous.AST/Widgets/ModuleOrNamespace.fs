namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ModuleOrNamespace =
    let Decls = Attributes.defineWidgetCollection "Decls"
    let IsRecursive = Attributes.defineScalar<bool> "IsRecursive"
    let HeaderName = Attributes.defineScalar<string> "HeaderName"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"
    let IsImplicit = Attributes.defineScalar<bool> "IsImplicit"
    let XmlDocs = Attributes.defineWidget "XmlDocs"
    let IsGlobal = Attributes.defineScalar<bool> "IsGlobal"

    let IsAnonymousModule = Attributes.defineScalar<bool> "IsAnonymousModule"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let WidgetKey =
        Widgets.register "Namespace" (fun widget ->
            let decls = Widgets.getNodesFromWidgetCollection<ModuleDecl> widget Decls

            let isAnonymousModule =
                Widgets.tryGetScalarValue widget IsAnonymousModule
                |> ValueOption.defaultValue false

            if isAnonymousModule then
                ModuleOrNamespaceNode(None, decls, Range.Zero)
            else
                let isGlobal =
                    Widgets.tryGetScalarValue widget IsGlobal |> ValueOption.defaultValue false

                let isImplicit =
                    Widgets.tryGetScalarValue widget IsImplicit |> ValueOption.defaultValue false

                let headerName =
                    if isGlobal then
                        SingleTextNode.``global``
                    else
                        SingleTextNode.Create(Widgets.getScalarValue widget HeaderName)

                let isRecursive =
                    Widgets.tryGetScalarValue widget IsRecursive |> ValueOption.defaultValue false

                let leadingKeyword =
                    if isImplicit then
                        SingleTextNode.``module``
                    else
                        SingleTextNode.``namespace``

                let accessControl =
                    Widgets.tryGetScalarValue widget Accessibility
                    |> ValueOption.defaultValue AccessControl.Unknown

                let accessControl =
                    match accessControl with
                    | Public -> Some(SingleTextNode.``public``)
                    | Private -> Some(SingleTextNode.``private``)
                    | Internal -> Some(SingleTextNode.``internal``)
                    | Unknown -> None

                let xmlDocs =
                    Widgets.tryGetNodeFromWidget widget XmlDocs
                    |> ValueOption.map(fun x -> Some(x))
                    |> ValueOption.defaultValue None

                let attributes =
                    Widgets.tryGetScalarValue widget MultipleAttributes
                    |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                    |> ValueOption.defaultValue None

                let header =
                    Some(
                        ModuleOrNamespaceHeaderNode(
                            xmlDocs,
                            attributes,
                            MultipleTextsNode([ leadingKeyword ], Range.Zero),
                            accessControl,
                            isRecursive,
                            Some(IdentListNode([ IdentifierOrDot.Ident(headerName) ], Range.Zero)),
                            Range.Zero
                        )
                    )

                ModuleOrNamespaceNode(header, decls, Range.Zero))

[<AutoOpen>]
module NamespaceBuilders =
    type Ast with
        /// <summary>Create a namespace with the specified name.</summary>
        /// <remarks>Namespaces are used to group declarations together e.g. modules, types.</remarks>
        /// <param name="name">The name of the namespace.</param>
        /// <code lang="fsharp">
        /// Oak() {
        ///     Namespace("Fabulous.AST") {
        ///         Namespace("MyNamespace") {
        ///            Module("MyModule") {
        ///                Value("x", "1")
        ///            }
        ///         }
        ///     }
        /// }
        /// </code>
        static member Namespace(name: string) =
            CollectionBuilder<ModuleOrNamespaceNode, ModuleDecl>(
                ModuleOrNamespace.WidgetKey,
                ModuleOrNamespace.Decls,
                AttributesBundle(
                    StackList.two(
                        ModuleOrNamespace.HeaderName.WithValue(name),
                        ModuleOrNamespace.IsAnonymousModule.WithValue(false)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        /// <summary>Create a global namespace.</summary>
        /// <remarks>You use the predefined namespace global to put names in the .NET top-level namespace..</remarks>
        /// <code lang="fsharp">
        /// Oak() {
        ///     GlobalNamespace() {
        ///         Module("Module1") {
        ///             Value("x", "1")
        ///         }
        ///     }
        /// }
        /// </code>
        static member GlobalNamespace() =
            CollectionBuilder<ModuleOrNamespaceNode, ModuleDecl>(
                ModuleOrNamespace.WidgetKey,
                ModuleOrNamespace.Decls,
                AttributesBundle(
                    StackList.two(
                        ModuleOrNamespace.IsGlobal.WithValue(true),
                        ModuleOrNamespace.IsAnonymousModule.WithValue(false)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        /// <summary>Create an anonymous module.</summary>
        /// <remarks>Anonymous modules are used to group declarations together without creating a new namespace.</remarks>
        /// <code lang="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Value("x", "1")
        ///     }
        /// }
        /// </code>
        static member AnonymousModule() =
            CollectionBuilder<ModuleOrNamespaceNode, ModuleDecl>(
                ModuleOrNamespace.WidgetKey,
                ModuleOrNamespace.Decls,
                ModuleOrNamespace.IsAnonymousModule.WithValue(true)
            )

type NamespaceModifiers =
    /// <summary>Sets the namespace to be recursive.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     Namespace("Fabulous.AST") {
    ///         Module("Module1") {
    ///             Value("x", "1")
    ///         }
    ///     }
    ///     |> _.toRecursive()
    /// }
    /// </code>
    [<Extension>]
    static member inline toRecursive(this: WidgetBuilder<ModuleOrNamespaceNode>) =
        this.AddScalar(ModuleOrNamespace.IsRecursive.WithValue(true))

    /// <summary>Sets the namespace to be private.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     Namespace("Fabulous.AST") {
    ///         Module("Module1") {
    ///             Value("x", "1")
    ///         }
    ///     }
    ///     |> _.toPrivate()
    /// }
    /// </code>
    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<ModuleOrNamespaceNode>) =
        this.AddScalar(ModuleOrNamespace.Accessibility.WithValue(AccessControl.Private))

    /// <summary>Sets the namespace to be public.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     Namespace("Fabulous.AST") {
    ///         Module("Module1") {
    ///             Value("x", "1")
    ///         }
    ///     }
    ///     |> _.toPublic()
    /// }
    /// </code>
    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<ModuleOrNamespaceNode>) =
        this.AddScalar(ModuleOrNamespace.Accessibility.WithValue(AccessControl.Public))

    /// <summary>Sets the namespace to be internal.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     Namespace("Fabulous.AST") {
    ///         Module("Module1") {
    ///             Value("x", "1")
    ///     }
    ///     |> _.toInternal()
    /// }
    /// </code>
    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<ModuleOrNamespaceNode>) =
        this.AddScalar(ModuleOrNamespace.Accessibility.WithValue(AccessControl.Internal))

    /// <summary>Sets the XmlDocs for the current namespace.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     Namespace("Fabulous.AST") {
    ///         Module("Module1") {
    ///             Value("x", "1")
    ///         }
    ///     }
    ///     |> _.xmlDocs(Summary("This is a namespace"))
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<ModuleOrNamespaceNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(ModuleOrNamespace.XmlDocs.WithValue(xmlDocs.Compile()))

    /// <summary>Sets the XmlDocs for the current namespace.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     Namespace("Fabulous.AST") {
    ///         Module("Module1") {
    ///             Value("x", "1")
    ///         }
    ///     }
    ///     |> _.xmlDocs([ "This is a namespace" ])
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<ModuleOrNamespaceNode>, xmlDocs: string list) =
        NamespaceModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    /// <summary>Sets the XmlDocs for the current namespace.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     Namespace("Fabulous.AST") {
    ///         Module("Module1") {
    ///             Value("x", "1")
    ///         }
    ///     }
    ///     |> _.xmlDocs("This is a namespace")
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<ModuleOrNamespaceNode>, xmlDocs: string) =
        NamespaceModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    /// <summary>Sets the attributes for the current namespace.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attributes">The attributes to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     Namespace("Fabulous.AST") {
    ///         Module("Module1") {
    ///             Value("x", "1")
    ///         }
    ///     }
    ///     |> _.attributes([ Attribute("MyAttribute") ])
    /// }
    /// </code>
    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<ModuleOrNamespaceNode>, attributes: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(ModuleOrNamespace.MultipleAttributes.WithValue(attributes |> List.map Gen.mkOak))

    /// <summary>Sets the attributes for the current namespace.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attribute">The attribute to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     Namespace("Fabulous.AST") {
    ///         Module("Module1") {
    ///             Value("x", "1")
    ///         }
    ///     }
    ///     |> _.attribute(Attribute("MyAttribute"))
    /// }
    /// </code>
    [<Extension>]
    static member inline attribute
        (this: WidgetBuilder<ModuleOrNamespaceNode>, attribute: WidgetBuilder<AttributeNode>)
        =
        NamespaceModifiers.attributes(this, [ attribute ])

    /// <summary>Sets the namespace to be implicit.</summary>
    /// <remarks>If the entire contents of the file are in one module, you can also declare namespaces implicitly by using the module keyword and providing the new namespace name in the fully qualified module name.</remarks>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     Namespace("Fabulous.AST") {
    ///         Module("Module1") {
    ///             Value("x", "1")
    ///         }
    ///     }
    ///     |> _.toImplicit()
    /// }
    /// </code>
    [<Extension>]
    static member inline toImplicit(this: WidgetBuilder<ModuleOrNamespaceNode>) =
        this.AddScalar(ModuleOrNamespace.IsImplicit.WithValue(true))
