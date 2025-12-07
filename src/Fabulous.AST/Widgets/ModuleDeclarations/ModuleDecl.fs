namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak

/// Shared attribute definitions used across module declaration widget types
module ModuleDecl =
    let XmlDocs = Attributes.defineWidget "ModuleDeclXmlDocs"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode seq> "ModuleDeclMultipleAttributes"

    let Accessibility = Attributes.defineScalar<AccessControl> "ModuleDeclAccessibility"
    let TypeParams = Attributes.defineWidget "ModuleDeclTypeParams"
    let IsMutable = Attributes.defineScalar<bool> "ModuleDeclIsMutable"
    let IsInlined = Attributes.defineScalar<bool> "ModuleDeclIsInlined"
    let IsRecursive = Attributes.defineScalar<bool> "ModuleDeclIsRecursive"

type ModuleDeclModifiers =
    /// <summary>Sets the XmlDocs for the current module declaration widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<ModuleDecl>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(ModuleDecl.XmlDocs.WithValue(xmlDocs.Compile()))

    /// <summary>Sets the XmlDocs for the current module declaration widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="comments">The comments to set.</param>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<ModuleDecl>, comments: string seq) =
        ModuleDeclModifiers.xmlDocs(this, Ast.XmlDocs(comments))

    /// <summary>Sets the XmlDocs for the current module declaration widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="comment">The comment to set.</param>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<ModuleDecl>, comment: string) =
        ModuleDeclModifiers.xmlDocs(this, [ comment ])

    /// <summary>Sets the attributes for the current module declaration widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attributes">The attributes to set.</param>
    [<Extension>]
    static member inline attributes(this: WidgetBuilder<ModuleDecl>, attributes: WidgetBuilder<AttributeNode> seq) =
        this.AddScalar(ModuleDecl.MultipleAttributes.WithValue(attributes |> Seq.map Gen.mkOak))

    /// <summary>Sets the attribute for the current module declaration widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attribute">The attribute to set.</param>
    [<Extension>]
    static member inline attribute(this: WidgetBuilder<ModuleDecl>, attribute: WidgetBuilder<AttributeNode>) =
        ModuleDeclModifiers.attributes(this, [ attribute ])

    /// <summary>Sets the accessibility for the current module declaration widget to private.</summary>
    /// <param name="this">Current widget.</param>
    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<ModuleDecl>) =
        this.AddScalar(ModuleDecl.Accessibility.WithValue(AccessControl.Private))

    /// <summary>Sets the accessibility for the current module declaration widget to public.</summary>
    /// <param name="this">Current widget.</param>
    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<ModuleDecl>) =
        this.AddScalar(ModuleDecl.Accessibility.WithValue(AccessControl.Public))

    /// <summary>Sets the accessibility for the current module declaration widget to internal.</summary>
    /// <param name="this">Current widget.</param>
    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<ModuleDecl>) =
        this.AddScalar(ModuleDecl.Accessibility.WithValue(AccessControl.Internal))

    /// <summary>Sets the current module declaration widget to be mutable.</summary>
    /// <param name="this">Current widget.</param>
    [<Extension>]
    static member inline toMutable(this: WidgetBuilder<ModuleDecl>) =
        this.AddScalar(ModuleDecl.IsMutable.WithValue(true))

    /// <summary>Sets the current module declaration widget to be inlined.</summary>
    /// <param name="this">Current widget.</param>
    [<Extension>]
    static member inline toInlined(this: WidgetBuilder<ModuleDecl>) =
        this.AddScalar(ModuleDecl.IsInlined.WithValue(true))

    /// <summary>Sets the current module declaration widget to be recursive.</summary>
    /// <param name="this">Current widget.</param>
    [<Extension>]
    static member inline toRecursive(this: WidgetBuilder<ModuleDecl>) =
        this.AddScalar(ModuleDecl.IsRecursive.WithValue(true))

    /// <summary>Sets the type parameters for the current module declaration widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="typeParams">The type parameters to set.</param>
    [<Extension>]
    static member inline typeParams(this: WidgetBuilder<ModuleDecl>, typeParams: WidgetBuilder<TyparDecls>) =
        this.AddWidget(ModuleDecl.TypeParams.WithValue(typeParams.Compile()))

type ModuleDeclCollectionBuilderExtensions =
    // Keep extensions for BindingNode since Value()/Function() return WidgetBuilder<BindingNode> for module-level
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: BindingNode) : CollectionContent =
        let widget = Ast.EscapeHatch(ModuleDecl.TopLevelBinding(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<BindingNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ModuleDeclCollectionBuilderExtensions.Yield(this, node)

    [<Extension>]
    static member inline YieldFrom(_: CollectionBuilder<'parent, ModuleDecl>, x: BindingNode seq) : CollectionContent =
        let widgets =
            x
            |> Seq.map(fun node -> Ast.EscapeHatch(ModuleDecl.TopLevelBinding(node)).Compile())
            |> Seq.toArray
            |> MutStackArray1.fromArray

        { Widgets = widgets }

    [<Extension>]
    static member inline YieldFrom
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<BindingNode> seq)
        : CollectionContent =
        let nodes = x |> Seq.map Gen.mkOak
        ModuleDeclCollectionBuilderExtensions.YieldFrom(this, nodes)

    // Keep extensions for ExternBindingNode since ExternBinding() returns WidgetBuilder<ExternBindingNode>
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: ExternBindingNode) : CollectionContent =
        let widget = Ast.EscapeHatch(ModuleDecl.ExternBinding(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ExternBindingNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ModuleDeclCollectionBuilderExtensions.Yield(this, node)

    [<Extension>]
    static member inline YieldFrom
        (_: CollectionBuilder<'parent, ModuleDecl>, x: ExternBindingNode seq)
        : CollectionContent =
        let widgets =
            x
            |> Seq.map(fun node -> Ast.EscapeHatch(ModuleDecl.ExternBinding(node)).Compile())
            |> Seq.toArray
            |> MutStackArray1.fromArray

        { Widgets = widgets }

    [<Extension>]
    static member inline YieldFrom
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ExternBindingNode> seq)
        : CollectionContent =
        let nodes = x |> Seq.map Gen.mkOak
        ModuleDeclCollectionBuilderExtensions.YieldFrom(this, nodes)
