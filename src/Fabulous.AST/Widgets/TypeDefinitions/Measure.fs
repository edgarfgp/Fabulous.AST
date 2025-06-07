namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module TypeNameNode =

    let Name = Attributes.defineScalar<string> "Name"

    let PowerType = Attributes.defineWidget "PowerType"

    let XmlDocs = Attributes.defineWidget "XmlDocs"

    let MeasureAttribute = Attributes.defineScalar<AttributeNode seq> "MeasureAttribute"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode seq> "MultipleAttributes"

    let WidgetKey =
        Widgets.register "Measure" (fun widget ->
            let name = Widgets.getScalarValue widget Name

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let measureAttribute = Widgets.getScalarValue widget MeasureAttribute |> List.ofSeq

            let multipleAttributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let attributes =
                match multipleAttributes with
                | Some(multipleAttributes) -> measureAttribute @ List.ofSeq multipleAttributes
                | None -> measureAttribute

            TypeNameNode(
                xmlDocs,
                Some(MultipleAttributeListNode.Create(attributes)),
                SingleTextNode.``type``,
                Some(SingleTextNode.Create(name)),
                IdentListNode([], Range.Zero),
                None,
                [],
                None,
                None,
                None,
                Range.Zero
            ))

[<AutoOpen>]
module TypeNameNodeBuilders =
    type Ast with
        /// <summary>Create a measure type with the given name.</summary>
        /// <param name="name">The name of the measure type.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Measure("cm")
        ///     }
        /// }
        /// </code>
        static member Measure(name: string) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name

            WidgetBuilder<TypeNameNode>(
                TypeNameNode.WidgetKey,
                TypeNameNode.Name.WithValue(name),
                TypeNameNode.MeasureAttribute.WithValue([ Gen.mkOak(Ast.Attribute("Measure")) ])
            )

        /// <summary>Create a measure type with the given name and power type.</summary>
        /// <param name="name">The name of the measure type.</param>
        /// <param name="powerType">The power type of the measure type.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Measure("ml", MeasurePower(LongIdent "cm", Integer "3"))
        ///     }
        /// }
        /// </code>
        static member Measure(name: string, powerType: WidgetBuilder<Type>) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name
            Ast.Abbrev(name, powerType).attribute(Ast.Attribute("Measure"))

        /// <summary>Create a measure type with the given name and power type.</summary>
        /// <param name="name">The name of the measure type.</param>
        /// <param name="powerType">The power type of the measure type.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Measure("ml", "cm^3")
        ///     }
        /// }
        /// </code>
        static member Measure(name: string, powerType: string) =
            Ast.Measure(name, Ast.LongIdent(powerType))

type TypeNameNodeModifiers =
    /// <summary>Sets the XmlDocs for the current measure type definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Measure("ml", "cm^3")
    ///             .xmlDocs(Summary("This is a measure type"))
    /// }
    /// </code>
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<TypeNameNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(TypeNameNode.XmlDocs.WithValue(xmlDocs.Compile()))

    /// <summary>Sets the XmlDocs for the current measure type definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Measure("ml", "cm^3")
    ///             .xmlDocs([ "This is a measure type" ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<TypeNameNode>, xmlDocs: string seq) =
        TypeNameNodeModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    /// <summary>Sets the attributes for the current measure type definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attributes">The attributes to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Measure("ml", "cm^3")
    ///             .attributes([ Attribute("Obsolete") ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attributes(this: WidgetBuilder<TypeNameNode>, attributes: WidgetBuilder<AttributeNode> seq) =
        this.AddScalar(TypeNameNode.MultipleAttributes.WithValue(attributes |> Seq.map Gen.mkOak))

    /// <summary>Sets the attributes for the current measure type definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attribute">The attributes to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Measure("ml", "cm^3")
    ///             .attribute(Attribute("Obsolete"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeNameNode>, attribute: WidgetBuilder<AttributeNode>) =
        TypeNameNodeModifiers.attributes(this, [ attribute ])

type TypeDefnAbbrevNodeYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: TypeNameNode) : CollectionContent =
        let typeDefn = TypeDefn.None(x)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeNameNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        TypeDefnAbbrevNodeYieldExtensions.Yield(this, node)
