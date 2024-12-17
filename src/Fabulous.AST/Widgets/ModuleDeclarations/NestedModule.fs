namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

open type Fabulous.AST.Ast
open type Fantomas.Core.SyntaxOak.Oak

module NestedModule =
    let Name = Attributes.defineScalar<string> "Name"
    let IsRecursive = Attributes.defineScalar<bool> "IsRecursive"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"
    let Decls = Attributes.defineWidgetCollection "Decls"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let XmlDocs = Attributes.defineWidget "XmlDoc"

    let IsTopLevel = Attributes.defineScalar<bool> "IsTopLevel"

    let WidgetKey =
        Widgets.register "NestedModule" (fun widget ->
            let name = Widgets.getScalarValue widget Name

            let name =
                name |> PrettyNaming.NormalizeIdentifierBackticks |> SingleTextNode.Create

            let moduleDecls = Widgets.getNodesFromWidgetCollection<ModuleDecl> widget Decls

            let isRecursive =
                Widgets.tryGetScalarValue widget IsRecursive |> ValueOption.defaultValue false

            let isTopLevel =
                Widgets.tryGetScalarValue widget IsTopLevel |> ValueOption.defaultValue false

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
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            NestedModuleNode(
                xmlDocs,
                attributes,
                SingleTextNode.``module``,
                accessControl,
                isRecursive,
                IdentListNode([ IdentifierOrDot.Ident(name) ], Range.Zero),
                SingleTextNode.equals,
                moduleDecls,
                Range.Zero
            ))

[<AutoOpen>]
module NestedModuleBuilders =
    type Ast with
        /// <summary>Creates a module widget with the specified name.</summary>
        /// <param name="name">The name of the module.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Module("MyModule") {
        ///             Value("x", "1")
        ///         }
        ///     }
        /// }
        /// </code>
        static member Module(name: string) =
            CollectionBuilder<NestedModuleNode, ModuleDecl>(
                NestedModule.WidgetKey,
                NestedModule.Decls,
                NestedModule.Name.WithValue(name)
            )

type NestedModuleModifiers =
    /// <summary>Sets the module to be recursive.</summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Module("MyModule") {
    ///             Value("x", "1")
    ///         }
    ///         |> _.toRecursive()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toRecursive(this: WidgetBuilder<NestedModuleNode>) =
        this.AddScalar(NestedModule.IsRecursive.WithValue(true))

    /// <summary>Sets the module to be private.</summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Module("MyModule") {
    ///             Value("x", "1")
    ///         }
    ///         |> _.toPrivate()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<NestedModuleNode>) =
        this.AddScalar(NestedModule.Accessibility.WithValue(AccessControl.Private))

    /// <summary>Sets the module to be public.</summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Module("MyModule") {
    ///             Value("x", "1")
    ///         }
    ///         |> _.toPublic()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<NestedModuleNode>) =
        this.AddScalar(NestedModule.Accessibility.WithValue(AccessControl.Public))

    /// <summary>Sets the module to be internal.</summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Module("MyModule") {
    ///             Value("x", "1")
    ///         }
    ///         |> _.toInternal()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<NestedModuleNode>) =
        this.AddScalar(NestedModule.Accessibility.WithValue(AccessControl.Internal))

    /// <summary>Sets the XmlDocs for the current module.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Module("Module1") {
    ///             Value("x", "1")
    ///         }
    ///         |> _.xmlDocs(Summary("This is a module"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<NestedModuleNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(NestedModule.XmlDocs.WithValue(xmlDocs.Compile()))

    /// <summary>Sets the XmlDocs for the current module.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Module("Module1") {
    ///             Value("x", "1")
    ///         }
    ///         |> _.xmlDocs([ "This is a module" ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<NestedModuleNode>, xmlDocs: string list) =
        NestedModuleModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    /// <summary>Sets the XmlDocs for the current module.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Module("Module1") {
    ///             Value("x", "1")
    ///         }
    ///         |> _.xmlDocs("This is a module")
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<NestedModuleNode>, xmlDocs: string) =
        NestedModuleModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    /// <summary>Sets the attributes for the current module.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attributes">The attributes to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Module("Module1") {
    ///             Value("x", "1")
    ///         }
    ///         |> _.attributes([ Attribute("AutoOpen") ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<NestedModuleNode>, attributes: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            NestedModule.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    /// <summary>Sets the attributes for the current module.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attribute">The attribute to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Module("Module1") {
    ///             Value("x", "1")
    ///         }
    ///         |> _.attribute(Attribute("AutoOpen"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attribute(this: WidgetBuilder<NestedModuleNode>, attribute: WidgetBuilder<AttributeNode>) =
        NestedModuleModifiers.attributes(this, [ attribute ])

type NestedModuleYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: NestedModuleNode) : CollectionContent =
        let moduleDecl = ModuleDecl.NestedModule x
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<NestedModuleNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        NestedModuleYieldExtensions.Yield(this, node)
