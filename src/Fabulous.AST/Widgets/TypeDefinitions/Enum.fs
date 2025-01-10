namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module Enum =

    let EnumCaseNode = Attributes.defineWidgetCollection "UnionCaseNode"

    let Name = Attributes.defineScalar<string> "Name"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let XmlDocs = Attributes.defineWidget "XmlDocs"

    let WidgetKey =
        Widgets.register "Enum" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

            let enumCaseNodes =
                Widgets.getNodesFromWidgetCollection<EnumCaseNode> widget EnumCaseNode

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            TypeDefnEnumNode(
                TypeNameNode(
                    xmlDocs,
                    attributes,
                    SingleTextNode.``type``,
                    Some(SingleTextNode.Create(name)),
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.equals) ], Range.Zero),
                    None,
                    [],
                    None,
                    None,
                    None,
                    Range.Zero
                ),
                enumCaseNodes,
                [],
                Range.Zero
            ))

[<AutoOpen>]
module EnumBuilders =
    type Ast with

        /// <summary>Create an enum with the given name.</summary>
        /// <param name="name">The name of the enum.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Enum("Color") {
        ///             EnumCase("Red", Int 0)
        ///             EnumCase("Green", Int 1)
        ///             EnumCase("Blue", Int 2)
        ///         }
        ///     }
        /// }
        /// </code>
        static member Enum(name: string) =
            CollectionBuilder<TypeDefnEnumNode, EnumCaseNode>(
                Enum.WidgetKey,
                Enum.EnumCaseNode,
                Enum.Name.WithValue(name)
            )

type EnumModifiers =
    /// <summary>Sets the XmlDocs for the current Enum definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Enum("Color") {
    ///            EnumCase("Red", Int(0))
    ///            EnumCase("Green", Int(1))
    ///            EnumCase("Blue", Int(2))
    ///        }
    ///        |> _.xmlDocs(Summary("This is a color enum"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnEnumNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(Enum.XmlDocs.WithValue(xmlDocs.Compile()))

    /// <summary>Sets the XmlDocs for the current Enum definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Enum("Color") {
    ///             EnumCase("Red", Int(0))
    ///             EnumCase("Green", Int(1))
    ///             EnumCase("Blue", Int(2))
    ///         }
    ///         |> _.xmlDocs([ "This is a color enum" ])§
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnEnumNode>, xmlDocs: string list) =
        EnumModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    /// <summary>Sets the attributes for the current Enum definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attributes">The attributes to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Enum("Color") {
    ///             EnumCase("Red", Int(0))
    ///             EnumCase("Green", Int(1))
    ///             EnumCase("Blue", Int(2))
    ///         }
    ///         |> _.attributes([ Attribute("Serializable") ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<TypeDefnEnumNode>, attributes: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            Enum.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    /// <summary>Adds an attribute to the current Enum definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attribute">The attribute to add.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Enum("Color") {
    ///             EnumCase("Red", Int(0))
    ///             EnumCase("Green", Int(1))
    ///             EnumCase("Blue", Int(2))
    ///         }
    ///         |> _.attribute(Attribute("Serializable"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefnEnumNode>, attribute: WidgetBuilder<AttributeNode>) =
        EnumModifiers.attributes(this, [ attribute ])

type EnumYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: TypeDefnEnumNode) : CollectionContent =
        let typeDefn = TypeDefn.Enum(x)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefnEnumNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        EnumYieldExtensions.Yield(this, node)
