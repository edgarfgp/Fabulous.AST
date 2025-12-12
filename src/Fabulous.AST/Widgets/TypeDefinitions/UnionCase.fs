namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module UnionCase =

    let Name = Attributes.defineScalar<string> "Name"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode seq> "MultipleAttributes"

    let Fields = Attributes.defineScalar<FieldNode seq> "Fields"

    let XmlDocs = Attributes.defineWidget "XmlDocs"

    let WidgetKey =
        Widgets.register "UnionCase" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name
                |> PrettyNaming.NormalizeIdentifierBackticks
                |> SingleTextNode.Create

            let fields =
                Widgets.tryGetScalarValue widget Fields
                |> ValueOption.map id
                |> ValueOption.defaultValue []
                |> List.ofSeq

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            UnionCaseNode(xmlDocs, attributes, None, name, fields, Range.Zero))

[<AutoOpen>]
module UnionCaseBuilders =
    type Ast with
        /// <summary>Create a union case with the specified name.</summary>
        /// <param name="name">The name of the union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Union("Color") {
        ///             UnionCase("Red")
        ///             UnionCase("Green")
        ///             UnionCase("Blue")
        ///         }
        ///     }
        /// }
        /// </code>
        static member UnionCase(name: string) =
            WidgetBuilder<UnionCaseNode>(UnionCase.WidgetKey, UnionCase.Name.WithValue(name))

        /// <summary>Create a union case with the specified name and fields.</summary>
        /// <param name="name">The name of the union case.</param>
        /// <param name="fields">The fields of the union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Union("Shape") {
        ///             UnionCase("Rectangle", [ Field(Float()); Field(Float()) ])
        ///             UnionCase("Rectangle", [ Field("width", Float()); Field("height", Float()) ])
        ///         }
        ///     }
        /// }
        /// </code>
        static member UnionCase(name: string, fields: WidgetBuilder<FieldNode> seq) =
            WidgetBuilder<UnionCaseNode>(
                UnionCase.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        UnionCase.Name.WithValue(name),
                        UnionCase.Fields.WithValue(fields |> Seq.map Gen.mkOak)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        /// <summary>Create a union case with the specified name and fields.</summary>
        /// <param name="name">The name of the union case.</param>
        /// <param name="fields">The fields of the union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Union("Shape") {
        ///             UnionCase("Rectangle", [ Float(); Float() ])
        ///         }
        ///     }
        /// }
        /// </code>
        static member UnionCase(name: string, fields: WidgetBuilder<Type> seq) =
            let fields = fields |> Seq.map Ast.Field
            Ast.UnionCase(name, fields)

        /// <summary>Create a union case with the specified name and fields.</summary>
        /// <param name="name">The name of the union case.</param>
        /// <param name="field">The fields of the union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Union("Shape") {
        ///             UnionCase("Rectangle", Float())
        ///         }
        ///     }
        /// }
        /// </code>
        static member UnionCase(name: string, field: WidgetBuilder<Type>) =
            Ast.UnionCase(name, [ Ast.Field field ])

        /// <summary>Create a union case with the specified name and fields.</summary>
        /// <param name="name">The name of the union case.</param>
        /// <param name="fields">The fields of the union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Union("Shape") {
        ///             UnionCase("Rectangle", [ "float"; "float" ])
        ///         }
        ///     }
        /// }
        /// </code>
        static member UnionCase(name: string, fields: string seq) =
            Ast.UnionCase(name, fields |> Seq.map Ast.Field)

        /// <summary>Create a union case with the specified name and fields.</summary>
        /// <param name="name">The name of the union case.</param>
        /// <param name="field">The fields of the union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Union("Shape") {
        ///             UnionCase("Rectangle", Field(Float()))
        ///             UnionCase("Rectangle", Field("width", Float()))
        ///         }
        /// }
        /// </code>
        static member UnionCase(name: string, field: WidgetBuilder<FieldNode>) = Ast.UnionCase(name, [ field ])

        /// <summary>Create a union case with the specified name and fields.</summary>
        /// <param name="name">The name of the union case.</param>
        /// <param name="field">The fields of the union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Union("Shape") {
        ///             UnionCase("Rectangle", "float")
        ///         }
        ///     }
        /// }
        /// </code>
        static member UnionCase(name: string, field: string) = Ast.UnionCase(name, [ field ])

        /// <summary>Create a union case with the specified name and fields.</summary>
        /// <param name="name">The name of the union case.</param>
        /// <param name="fields">The fields of the union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Union("Shape") {
        ///             UnionCase("Rectangle", [ ("width", "float"); ("height", "float") ])
        ///         }
        ///     }
        /// }
        /// </code>
        static member UnionCase(name: string, fields: (string * string) seq) =
            Ast.UnionCase(name, fields |> Seq.map(Ast.Field))

        /// <summary>Create a union case with the specified name and fields.</summary>
        /// <param name="name">The name of the union case.</param>
        /// <param name="fields">The fields of the union case.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Union("Shape") {
        ///             UnionCase("Rectangle", [ ("width", Float()); ("height", Float()) ])
        ///         }
        ///     }
        /// }
        /// </code>
        static member UnionCase(name: string, fields: (string * WidgetBuilder<Type>) seq) =
            Ast.UnionCase(name, fields |> Seq.map(Ast.Field))

type UnionCaseModifiers =
    /// <summary>Sets the XmlDocs for the current UnionCase definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Union("Shape") {
    ///             UnionCase("Rectangle", [ ("width", Float()); ("height", Float()) ])
    ///                 .xmlDocs(Summary("This is a rectangle"))
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<UnionCaseNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(UnionCase.XmlDocs.WithValue(xmlDocs.Compile()))

    /// <summary>Sets the XmlDocs for the current UnionCase definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Union("Shape") {
    ///             UnionCase("Rectangle", [ ("width", Float()); ("height", Float()) ])
    ///                 .xmlDocs([ "This is a rectangle" ])
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<UnionCaseNode>, xmlDocs: string seq) =
        UnionCaseModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    /// <summary>Sets the attributes for the current UnionCase definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attributes">The attributes to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Union("Shape") {
    ///             UnionCase("Rectangle", [ ("width", Float()); ("height", Float()) ])
    ///                 .attributes([ Attribute("MyCustomAttribute") ])
    ///         }
    ///    }
    /// }
    /// </code>
    [<Extension>]
    static member inline attributes(this: WidgetBuilder<UnionCaseNode>, attributes: WidgetBuilder<AttributeNode> seq) =
        this.AddScalar(
            UnionCase.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    /// <summary>Sets the attributes for the current UnionCase definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attribute">The attribute to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Union("Shape") {
    ///             UnionCase("Rectangle", [ ("width", Float()); ("height", Float()) ])
    ///                 .attribute(Attribute("MyCustomAttribute"))
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attribute(this: WidgetBuilder<UnionCaseNode>, attribute: WidgetBuilder<AttributeNode>) =
        UnionCaseModifiers.attributes(this, [ attribute ])

type UnionCaseYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<TypeDefn, UnionCaseNode>, x: UnionCaseNode) : CollectionContent =
        let widget = Ast.EscapeHatch(x).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<TypeDefn, UnionCaseNode>, x: WidgetBuilder<UnionCaseNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        UnionCaseYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline YieldFrom
        (_: CollectionBuilder<TypeDefnUnionNode, UnionCaseNode>, x: UnionCaseNode seq)
        : CollectionContent =
        let widgets =
            x
            |> Seq.map(fun node -> Ast.EscapeHatch(node).Compile())
            |> Seq.toArray
            |> MutStackArray1.fromArray

        { Widgets = widgets }

    [<Extension>]
    static member inline YieldFrom
        (this: CollectionBuilder<TypeDefnUnionNode, UnionCaseNode>, x: WidgetBuilder<UnionCaseNode> seq)
        : CollectionContent =
        let nodes = x |> Seq.map Gen.mkOak
        UnionCaseYieldExtensions.YieldFrom(this, nodes)
