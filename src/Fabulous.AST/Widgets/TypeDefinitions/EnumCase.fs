namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module EnumCase =

    let Name = Attributes.defineScalar<string> "Name"

    let Value = Attributes.defineWidget "Value"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let XmlDocs = Attributes.defineWidget "XmlDocs"

    let WidgetKey =
        Widgets.register "EnumCase" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

            let value = Widgets.getNodeFromWidget widget Value

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            EnumCaseNode(
                xmlDocs,
                None,
                attributes,
                SingleTextNode.Create(name),
                SingleTextNode.equals,
                value,
                Range.Zero
            ))

[<AutoOpen>]
module EnumCaseBuilders =
    type Ast with

        /// <summary>Create an enum case with the given name and field.</summary>
        /// <param name="name">The name of the enum case.</param>
        /// <param name="field">The field of the enum case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Enum("Color") {
        ///             EnumCase("Red", ConstantExpr(Int 0))
        ///             EnumCase("Green", ConstantExpr(Int 1))
        ///             EnumCase("Blue", ConstantExpr(Int 2))
        ///         }
        ///     }
        /// }
        /// </code>
        static member EnumCase(name: string, field: WidgetBuilder<Expr>) =
            WidgetBuilder<EnumCaseNode>(
                EnumCase.WidgetKey,
                AttributesBundle(
                    StackList.one(EnumCase.Name.WithValue(name)),
                    [| EnumCase.Value.WithValue(field.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>Create an enum case with the given name and field.</summary>
        /// <param name="name">The name of the enum case.</param>
        /// <param name="field">The field of the enum case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Enum("Color") {
        ///             EnumCase("Red", Int(0))
        ///             EnumCase("Green", Int(1))
        ///             EnumCase("Blue", Int(2))
        ///         }
        ///     }
        /// }
        /// </code>
        static member EnumCase(name: string, field: WidgetBuilder<Constant>) =
            Ast.EnumCase(name, Ast.ConstantExpr(field))

        /// <summary>Create an enum case with the given name and field.</summary>
        /// <param name="name">The name of the enum case.</param>
        /// <param name="field">The field of the enum case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Enum("Color") {
        ///             EnumCase("Red", "0")
        ///             EnumCase("Green", "1")
        ///             EnumCase("Blue", "2")
        ///         }
        ///     }
        /// }
        /// </code>
        static member EnumCase(name: string, field: string) = Ast.EnumCase(name, Ast.Constant(field))

type EnumCaseModifiers =
    /// <summary>Sets the XmlDocs for the current EnumCase definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Enum("Color") {
    ///            EnumCase("Red", Int(0))
    ///                .xmlDocs(Summary("This is the Red color."))
    ///            EnumCase("Green", Int(1))
    ///            EnumCase("Blue", Int(2))
    ///        }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<EnumCaseNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(EnumCase.XmlDocs.WithValue(xmlDocs.Compile()))

    /// <summary>Sets the XmlDocs for the current EnumCase definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Enum("Color") {
    ///             EnumCase("Red", Int(0))
    ///                 .xmlDocs([ "This is the Red color." ])
    ///             EnumCase("Green", Int(1))
    ///             EnumCase("Blue", Int(2))
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<EnumCaseNode>, xmlDocs: string list) =
        EnumCaseModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    /// <summary>Sets the attributes for the current EnumCase definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attributes">The attributes to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Enum("Color") {
    ///             EnumCase("Red", Int(0))
    ///                 .attributes([ Attribute("Serializable") ])
    ///             EnumCase("Green", Int(1))
    ///             EnumCase("Blue", Int(2))
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attributes(this: WidgetBuilder<EnumCaseNode>, attributes: WidgetBuilder<AttributeNode> list) =
        this.AddScalar(
            EnumCase.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    /// <summary>Sets the attribute for the current EnumCase definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attribute">The attribute to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Enum("Color") {
    ///             EnumCase("Red", Int(0))
    ///                 .attribute(Attribute("Serializable"))
    ///             EnumCase("Green", Int(1))
    ///             EnumCase("Blue", Int(2))
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attribute(this: WidgetBuilder<EnumCaseNode>, attribute: WidgetBuilder<AttributeNode>) =
        EnumCaseModifiers.attributes(this, [ attribute ])

type EnumCaseNodeYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<TypeDefnEnumNode, EnumCaseNode>, x: EnumCaseNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(x).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<TypeDefnEnumNode, EnumCaseNode>, x: WidgetBuilder<EnumCaseNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        EnumCaseNodeYieldExtensions.Yield(this, node)
