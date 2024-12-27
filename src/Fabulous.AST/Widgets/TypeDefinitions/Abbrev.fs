namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module TypeDefnAbbrevNode =

    let Name = Attributes.defineScalar<string> "Name"

    let AliasType = Attributes.defineWidget "AliasType"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let XmlDocs = Attributes.defineWidget "XmlDocs"

    let TypeParams = Attributes.defineWidget "TypeParams"

    let WidgetKey =
        Widgets.register "Abbrev" (fun widget ->
            let name = Widgets.getScalarValue widget Name

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let aliasType = Widgets.getNodeFromWidget widget AliasType

            let typeParams =
                Widgets.tryGetNodeFromWidget widget TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            TypeDefnAbbrevNode(
                TypeNameNode(
                    xmlDocs,
                    attributes,
                    SingleTextNode.``type``,
                    None,
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero),
                    typeParams,
                    [],
                    None,
                    Some(SingleTextNode.equals),
                    None,
                    Range.Zero
                ),
                aliasType,
                [],
                Range.Zero
            ))

[<AutoOpen>]
module TypeDefnAbbrevNodeBuilders =
    type Ast with

        /// <summary>Create a type abbreviation with the given name and alias type.</summary>
        /// <param name="name">The name of the type abbreviation.</param>
        /// <param name="alias">The alias type of the type abbreviation.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Abbrev("SizeType", "uint32")
        ///    }
        /// }
        /// </code>
        static member Abbrev(name: string, alias: WidgetBuilder<Type>) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name

            WidgetBuilder<TypeDefnAbbrevNode>(
                TypeDefnAbbrevNode.WidgetKey,
                AttributesBundle(
                    StackList.one(TypeDefnAbbrevNode.Name.WithValue(name)),
                    [| TypeDefnAbbrevNode.AliasType.WithValue(alias.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>Create a type abbreviation with the given name and alias type.</summary>
        /// <param name="name">The name of the type abbreviation.</param>
        /// <param name="alias">The alias type of the type abbreviation.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Abbrev("SizeType", "uint32")
        ///     }
        /// }
        /// </code>
        static member Abbrev(name: string, alias: string) = Ast.Abbrev(name, Ast.LongIdent(alias))

type TypeDefnAbbrevNodeModifiers =
    /// <summary>Sets the XmlDocs for the current type abbreviation definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Abbrev("SizeType", "uint32")
    ///             .xmlDocs(Summary("This is a size type"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<TypeDefnAbbrevNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(TypeDefnAbbrevNode.XmlDocs.WithValue(xmlDocs.Compile()))

    /// <summary>Sets the XmlDocs for the current type abbreviation definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Abbrev("SizeType", "uint32")
    ///             .xmlDocs([ "This is a size type" ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<TypeDefnAbbrevNode>, xmlDocs: string list) =
        TypeDefnAbbrevNodeModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    /// <summary>Sets the attributes for the current type abbreviation definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attributes">The attributes to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Abbrev("SizeType", "uint32")
    ///             .attributes([ Attribute("Serializable") ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<TypeDefnAbbrevNode>, attributes: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            TypeDefnAbbrevNode.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    /// <summary>Sets the attributes for the current type abbreviation definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attribute">The attributes to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Abbrev("SizeType", "uint32")
    ///             .attribute(Attribute("Serializable"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefnAbbrevNode>, attribute: WidgetBuilder<AttributeNode>) =
        TypeDefnAbbrevNodeModifiers.attributes(this, [ attribute ])

    /// <summary>Sets the type parameters for the current type abbreviation definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="typeParams">The type parameters to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Abbrev("Transform", Funs("'a", "'a"))
    ///             .typeParams(PostfixList("'a"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline typeParams(this: WidgetBuilder<TypeDefnAbbrevNode>, typeParams: WidgetBuilder<TyparDecls>) =
        this.AddWidget(TypeDefnAbbrevNode.TypeParams.WithValue(typeParams.Compile()))

type AbbrevYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: TypeDefnAbbrevNode) : CollectionContent =
        let typeDefn = TypeDefn.Abbrev(x)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefnAbbrevNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        AbbrevYieldExtensions.Yield(this, node)
