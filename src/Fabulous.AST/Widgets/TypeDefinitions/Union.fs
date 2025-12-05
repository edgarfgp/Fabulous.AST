namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module Union =

    let UnionCaseNode = Attributes.defineWidgetCollection "UnionCaseNode"

    let Name = Attributes.defineScalar<string> "Name"

    let Members = Attributes.defineWidgetCollection "Members"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode seq> "MultipleAttributes"

    let XmlDocs = Attributes.defineWidget "XmlDocs"

    let TypeParams = Attributes.defineWidget "TypeParams"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let IsRecursive = Attributes.defineScalar<bool> "IsRecursive"

    let WidgetKey =
        Widgets.register "Union" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

            let unionCaseNode =
                Widgets.getNodesFromWidgetCollection<UnionCaseNode> widget UnionCaseNode

            let members =
                Widgets.tryGetNodesFromWidgetCollection<MemberDefn> widget Members
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

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let isRecursive =
                Widgets.tryGetScalarValue widget IsRecursive |> ValueOption.defaultValue false

            let leadingKeyword =
                if isRecursive then
                    SingleTextNode.``and``
                else
                    SingleTextNode.``type``

            TypeDefnUnionNode(
                TypeNameNode(
                    xmlDocs,
                    attributes,
                    leadingKeyword,
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
                unionCaseNode,
                members,
                Range.Zero
            ))

[<AutoOpen>]
module UnionBuilders =
    type Ast with
        /// <summary>Creates a new union type with the specified name.</summary>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Union("Shape") {
        ///             UnionCase("Circle")
        ///             UnionCase("Rectangle")
        ///         }
        ///     }
        /// }
        /// </code>
        static member Union(name: string) =
            CollectionBuilder<TypeDefnUnionNode, UnionCaseNode>(
                Union.WidgetKey,
                Union.UnionCaseNode,
                Union.Name.WithValue(PrettyNaming.NormalizeIdentifierBackticks name)
            )

type UnionModifiers =
    /// <summary>Sets the members for the current union definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         (Union("Shape") {
    ///             UnionCase("Rectangle", Field("width", String()))
    ///         })
    ///             .members() {
    ///                 Member("Width", Float(10.)).toStatic()
    ///             }
    ///    }
    /// }
    /// </code>
    [<Extension>]
    static member inline members(this: WidgetBuilder<TypeDefnUnionNode>) =
        AttributeCollectionBuilder<TypeDefnUnionNode, MemberDefn>(this, Union.Members)

    /// <summary>Sets the type params for the current union definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="typeParams">The type params to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Union("Option") {
    ///             UnionCase("Some", Field("value", "'a"))
    ///             UnionCase("None")
    ///         }
    ///         |> |> _.typeParams(PostfixList("'a"))
    ///     }
    /// }
    ///  </code>
    [<Extension>]
    static member inline typeParams(this: WidgetBuilder<TypeDefnUnionNode>, typeParams: WidgetBuilder<TyparDecls>) =
        this.AddWidget(Union.TypeParams.WithValue(typeParams.Compile()))

    /// <summary>Sets the XmlDocs for the current union definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Union("Shape") {
    ///             UnionCase("Rectangle", [ ("width", Float()); ("height", Float()) ])
    ///         }
    ///         |> _.xmlDocs(Summary("This is a shape"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnUnionNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(Union.XmlDocs.WithValue(xmlDocs.Compile()))

    /// <summary>Sets the XmlDocs for the current union definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Union("Shape") {
    ///             UnionCase("Rectangle", [ ("width", Float()); ("height", Float()) ])
    ///         }
    ///         |> _.xmlDocs([ "This is a shape" ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnUnionNode>, xmlDocs: string seq) =
        UnionModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    /// <summary>Sets the attributes for the current union definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attributes">The attributes to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Union("Shape") {
    ///             UnionCase("Rectangle", [ ("width", Float()); ("height", Float()) ])
    ///         }
    ///         |> _.attributes([ Attribute("MyCustomUnionAttribute") ])
    ///    }
    /// }
    /// </code>
    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<TypeDefnUnionNode>, attributes: WidgetBuilder<AttributeNode> seq)
        =
        this.AddScalar(Union.MultipleAttributes.WithValue(attributes |> Seq.map Gen.mkOak))

    /// <summary>Sets the attributes for the current union definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attribute">The attribute to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Union("Shape") {
    ///             UnionCase("Rectangle", [ ("width", Float()); ("height", Float()) ])
    ///         }
    ///         |> _.attribute(Attribute("MyCustomUnionAttribute"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefnUnionNode>, attribute: WidgetBuilder<AttributeNode>) =
        UnionModifiers.attributes(this, [ attribute ])

    /// <summary>Sets the union to be internal.</summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Union("Shape") {
    ///             UnionCase("Rectangle", [ ("width", Float()); ("height", Float()) ])
    ///         }
    ///         |> _.toInternal()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<TypeDefnUnionNode>) =
        this.AddScalar(Union.Accessibility.WithValue(AccessControl.Private))

    /// <summary>Sets the union to be public.</summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Union("Shape") {
    ///             UnionCase("Rectangle", [ ("width", Float()); ("height", Float()) ])
    ///         }
    ///         |> _.toPublic()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<TypeDefnUnionNode>) =
        this.AddScalar(Union.Accessibility.WithValue(AccessControl.Public))

    /// <summary>Sets the union to be internal.</summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Union("Shape") {
    ///             UnionCase("Rectangle", [ ("width", Float()); ("height", Float()) ])
    ///         }
    ///         |> _.toInternal()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<TypeDefnUnionNode>) =
        this.AddScalar(Union.Accessibility.WithValue(AccessControl.Internal))

    /// <summary>Sets the union to be recursive.</summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Union("Shape") {
    ///             UnionCase("Rectangle", [ ("width", Float()); ("height", Float()) ])
    ///         }
    ///         |> _.toRecursive()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toRecursive(this: WidgetBuilder<TypeDefnUnionNode>) =
        this.AddScalar(Union.IsRecursive.WithValue(true))

type UnionYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: TypeDefnUnionNode) : CollectionContent =
        let typeDefn = TypeDefn.Union(x)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefnUnionNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        UnionYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline YieldFrom
        (_: CollectionBuilder<'parent, ModuleDecl>, x: TypeDefnUnionNode seq)
        : CollectionContent =
        let widgets =
            x
            |> Seq.map(fun node ->
                let typeDefn = TypeDefn.Union(node)
                let typeDefn = ModuleDecl.TypeDefn(typeDefn)
                Ast.EscapeHatch(typeDefn).Compile())
            |> Seq.toArray
            |> MutStackArray1.fromArray

        { Widgets = widgets }

    [<Extension>]
    static member inline YieldFrom
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefnUnionNode> seq)
        : CollectionContent =
        let nodes = x |> Seq.map Gen.mkOak
        UnionYieldExtensions.YieldFrom(this, nodes)
