namespace Fabulous.AST

open System
open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak

module BindingNode =
    let BodyExpr = Attributes.defineWidget "BindingBodyExpr"
    let IsMutable = Attributes.defineScalar<bool> "IsMutable"
    let XmlDocs = Attributes.defineWidget "XmlDocs"
    let IsInlined = Attributes.defineScalar<bool> "IsInlined"
    let IsStatic = Attributes.defineScalar<bool> "IsStatic"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode seq> "MultipleAttributes"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"
    let Return = Attributes.defineWidget "Return"
    let TypeParams = Attributes.defineWidget "TypeParams"

type BindingNodeModifiers =
    /// <summary>
    /// Sets the XmlDocs for the current widget.
    /// </summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Function("add", ["a"; "b"], "a + b")
    ///             .xmlDocs(Summary("This is a summary"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<BindingNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(BindingNode.XmlDocs.WithValue(xmlDocs.Compile()))

    /// <summary>
    /// Sets the XmlDocs for the current widget.
    /// </summary>
    /// <param name="this">Current widget.</param>
    /// <param name="comments">The comments to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Function("add", ["a"; "b"], "a + b")
    ///             .xmlDocs(["This is a summary"])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<BindingNode>, comments: string seq) =
        BindingNodeModifiers.xmlDocs(this, Ast.XmlDocs(comments))

    /// <summary>
    /// Sets the attributes for the current widget.
    /// </summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attributes">The attributes to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Function("add", ["a"; "b"], "a + b")
    ///             .attributes([ Attribute "Obsolete" ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attributes(this: WidgetBuilder<BindingNode>, attributes: WidgetBuilder<AttributeNode> seq) =
        this.AddScalar(BindingNode.MultipleAttributes.WithValue(attributes |> Seq.map Gen.mkOak))

    /// <summary>
    /// Sets the attributes for the current widget.
    /// </summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attribute">The attribute to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Function("add", ["a"; "b"], "a + b")
    ///             .attribute(Attribute "Obsolete")
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attribute(this: WidgetBuilder<BindingNode>, attribute: WidgetBuilder<AttributeNode>) =
        BindingNodeModifiers.attributes(this, [ attribute ])

    /// <summary>
    /// Sets the accessibility for the current widget to private.
    /// </summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Function("add", ["a"; "b"], "a + b")
    ///             .toPrivate()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<BindingNode>) =
        this.AddScalar(BindingNode.Accessibility.WithValue(AccessControl.Private))

    /// <summary>
    /// Sets the accessibility for the current widget to public.
    /// </summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Function("add", ["a"; "b"], "a + b")
    ///             .toPublic()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<BindingNode>) =
        this.AddScalar(BindingNode.Accessibility.WithValue(AccessControl.Public))

    /// <summary>
    /// Sets the accessibility for the current widget to internal.
    /// </summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Function("add", ["a"; "b"], "a + b")
    ///             .toInternal()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<BindingNode>) =
        this.AddScalar(BindingNode.Accessibility.WithValue(AccessControl.Internal))

    /// <summary>
    /// Sets the return type for the current widget.
    /// </summary>
    /// <param name="this">Current widget.</param>
    /// <param name="returnType">The return type to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Function("add", ["a"; "b"], "a + b")
    ///             .returnType(Int())
    ///     }
    /// }
    /// </code>
    [<Extension>]
    [<Obsolete("Use the overload that takes a widget in the constructor instead.")>]
    static member inline returnType(this: WidgetBuilder<BindingNode>, returnType: WidgetBuilder<Type>) =
        this.AddWidget(BindingNode.Return.WithValue(returnType.Compile()))

    /// <summary>
    /// Sets the return type for the current widget.
    /// </summary>
    /// <param name="this">Current widget.</param>
    /// <param name="returnType">The return type to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Function("add", ["a"; "b"], "a + b")
    ///             .returnType("int")
    ///     }
    /// }
    /// </code>
    [<Extension>]
    [<Obsolete("Use the overload that takes a widget in the constructor instead.")>]
    static member inline returnType(this: WidgetBuilder<BindingNode>, returnType: string) =
        this.AddWidget(BindingNode.Return.WithValue(Ast.LongIdent(returnType).Compile()))

    /// <summary>
    /// Sets the current widget to be mutable.
    /// </summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Value("x", Int(12))
    ///             .toMutable()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toMutable(this: WidgetBuilder<BindingNode>) =
        this.AddScalar(BindingNode.IsMutable.WithValue(true))

    /// <summary>
    /// Sets the current widget to be inlined.
    /// </summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Function("add", ["a"; "b"], "a + b")
    ///             .toInlined()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toInlined(this: WidgetBuilder<BindingNode>) =
        this.AddScalar(BindingNode.IsInlined.WithValue(true))

    /// <summary>
    /// Sets the current widget to be static.
    /// </summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Value("x", Int(12))
    ///             .toStatic()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toStatic(this: WidgetBuilder<BindingNode>) =
        this.AddScalar(BindingNode.IsStatic.WithValue(true))

    /// <summary>
    /// Sets the type parameters for the current widget.
    /// </summary>
    /// <param name="this">Current widget.</param>
    /// <param name="typeParams">The type parameters to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Function("add", ["a"; "b"], "a + b")
    ///             .typeParams([ TyparDecl("'a") ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline typeParams(this: WidgetBuilder<#BindingNode>, typeParams: WidgetBuilder<TyparDecls>) =
        this.AddWidget(BindingNode.TypeParams.WithValue(typeParams.Compile()))

type ValueYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: BindingNode) : CollectionContent =
        let moduleDecl = ModuleDecl.TopLevelBinding x
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<BindingNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ValueYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline YieldFrom(_: CollectionBuilder<'parent, ModuleDecl>, x: BindingNode seq) : CollectionContent =
        let widgets =
            x
            |> Seq.map(fun node ->
                let moduleDecl = ModuleDecl.TopLevelBinding node
                Ast.EscapeHatch(moduleDecl).Compile())
            |> Seq.toArray
            |> MutStackArray1.fromArray

        { Widgets = widgets }

    [<Extension>]
    static member inline YieldFrom
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<BindingNode> seq)
        : CollectionContent =
        let nodes = x |> Seq.map Gen.mkOak
        ValueYieldExtensions.YieldFrom(this, nodes)
