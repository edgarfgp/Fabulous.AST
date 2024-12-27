namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module Augmentation =
    let XmlDocs = Attributes.defineWidget "XmlDocs"

    let Name = Attributes.defineScalar<string> "Name"
    let Members = Attributes.defineWidgetCollection "Members"

    let TypeParams = Attributes.defineWidget "TypeParams"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let WidgetKey =
        Widgets.register "Augmentation" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

            let members =
                Widgets.tryGetNodesFromWidgetCollection<MemberDefn> widget Members
                |> ValueOption.defaultValue []

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let typeParams =
                Widgets.tryGetNodeFromWidget widget TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget XmlDocs
                |> ValueOption.map(fun x -> Some(x))
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

            TypeDefnAugmentationNode(
                TypeNameNode(
                    xmlDocs,
                    attributes,
                    SingleTextNode.``type``,
                    accessControl,
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero),
                    typeParams,
                    [],
                    None,
                    None,
                    Some SingleTextNode.``with``,
                    Range.Zero
                ),
                members,
                Range.Zero
            ))

[<AutoOpen>]
module AugmentBuilders =
    type Ast with
        /// <summary>Create a type augmentation with the given name.</summary>
        /// <param name="name">The name of the type augmentation.</param>
        ///<code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Augmentation("IEnumerable") {
        ///            Member(
        ///                "xs.RepeatElements",
        ///                ParenPat(ParameterPat("n", Int())),
        ///                SeqExpr(ForEachDoExpr("x", "xs", ForEachArrowExpr("_", "1 .. n", "x")))
        ///            )
        ///        }
        /// }
        /// </code>
        static member Augmentation(name: string) =
            CollectionBuilder<TypeDefnAugmentationNode, MemberDefn>(
                Augmentation.WidgetKey,
                Augmentation.Members,
                Augmentation.Name.WithValue(name)
            )

type AugmentationModifiers =
    /// <summary>Sets the type parameters for the current widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="typeParams">The type parameters to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Augmentation("IEnumerable") {
    ///             Member(
    ///                 "xs.RepeatElements",
    ///                 ParenPat(ParameterPat("n", Int())),
    ///                 SeqExpr(ForEachDoExpr("x", "xs", ForEachArrowExpr("_", "1 .. n", "x")))
    ///             )
    ///         }
    ///         |> _.typeParams(PostfixList(TyparDecl("'T")))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline typeParams
        (this: WidgetBuilder<TypeDefnAugmentationNode>, typeParams: WidgetBuilder<TyparDecls>)
        =
        this.AddWidget(Augmentation.TypeParams.WithValue(typeParams.Compile()))

    /// <summary>Sets the XML documentation for the current widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XML documentation to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Augmentation("IEnumerable") {
    ///             Member(
    ///                 "xs.RepeatElements",
    ///                 ParenPat(ParameterPat("n", Int())),
    ///                 SeqExpr(ForEachDoExpr("x", "xs", ForEachArrowExpr("_", "1 .. n", "x")))
    ///             )
    ///         }
    ///         |> _.xmlDocs(Summary("Repeat each element of the sequence n times"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnAugmentationNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(Augmentation.XmlDocs.WithValue(xmlDocs.Compile()))

    /// <summary>Sets the XML documentation for the current widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XML documentation to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Augmentation("IEnumerable") {
    ///             Member(
    ///                 "xs.RepeatElements",
    ///                 ParenPat(ParameterPat("n", Int())),
    ///                 SeqExpr(ForEachDoExpr("x", "xs", ForEachArrowExpr("_", "1 .. n", "x")))
    ///             )
    ///         }
    ///         |> _.xmlDocs([ "Repeat each element of the sequence n times" ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnAugmentationNode>, xmlDocs: string list) =
        AugmentationModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    /// <summary>Sets the accessibility for the current widget to private.</summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Augmentation("IEnumerable") {
    ///             Member(
    ///                 "xs.RepeatElements",
    ///                 ParenPat(ParameterPat("n", Int())),
    ///                 SeqExpr(ForEachDoExpr("x", "xs", ForEachArrowExpr("_", "1 .. n", "x")))
    ///             )
    ///         }
    ///         |> _.toPrivate()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<TypeDefnAugmentationNode>) =
        this.AddScalar(Augmentation.Accessibility.WithValue(AccessControl.Private))

    /// <summary>Sets the accessibility for the current widget to public.</summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Augmentation("IEnumerable") {
    ///             Member(
    ///                 "xs.RepeatElements",
    ///                 ParenPat(ParameterPat("n", Int())),
    ///                 SeqExpr(ForEachDoExpr("x", "xs", ForEachArrowExpr("_", "1 .. n", "x")))
    ///             )
    ///         }
    ///         |> _.toPublic()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<TypeDefnAugmentationNode>) =
        this.AddScalar(Augmentation.Accessibility.WithValue(AccessControl.Public))

    /// <summary>Sets the accessibility for the current widget to internal.</summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Augmentation("IEnumerable") {
    ///             Member(
    ///                 "xs.RepeatElements",
    ///                 ParenPat(ParameterPat("n", Int())),
    ///                 SeqExpr(ForEachDoExpr("x", "xs", ForEachArrowExpr("_", "1 .. n", "x")))
    ///             )
    ///         }
    ///         |> _.toInternal()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<TypeDefnAugmentationNode>) =
        this.AddScalar(Augmentation.Accessibility.WithValue(AccessControl.Internal))

    /// <summary>Sets the attributes for the current widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attributes">The attributes to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Augmentation("IEnumerable") {
    ///             Member(
    ///                 "xs.RepeatElements",
    ///                 ParenPat(ParameterPat("n", Int())),
    ///                 SeqExpr(ForEachDoExpr("x", "xs", ForEachArrowExpr("_", "1 .. n", "x")))
    ///             )
    ///         }
    ///         |> _.attributes([ Attribute("Obsolete") ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<TypeDefnAugmentationNode>, attributes: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(Augmentation.MultipleAttributes.WithValue(attributes |> List.map Gen.mkOak))

    /// <summary>Sets the attribute for the current widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attribute">The attribute to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Augmentation("IEnumerable") {
    ///             Member(
    ///                 "xs.RepeatElements",
    ///                 ParenPat(ParameterPat("n", Int())),
    ///                 SeqExpr(ForEachDoExpr("x", "xs", ForEachArrowExpr("_", "1 .. n", "x")))
    ///             )
    ///         }
    ///         |> _.attribute(Attribute("Obsolete"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attribute
        (this: WidgetBuilder<TypeDefnAugmentationNode>, attribute: WidgetBuilder<AttributeNode>)
        =
        AugmentationModifiers.attributes(this, [ attribute ])

type AugmentYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: TypeDefnAugmentationNode)
        : CollectionContent =
        let typeDefn = TypeDefn.Augmentation(x)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefnAugmentationNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        AugmentYieldExtensions.Yield(this, node)
