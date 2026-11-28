namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ExternBinding =
    let XmlDocs = Attributes.defineWidget "XmlDocs"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode seq> "MultipleAttributes"

    let AttributesOfType = Attributes.defineScalar<AttributeNode seq> "AttributesOfType"
    let TypeVal = Attributes.defineWidget "Type"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"
    let Identifier = Attributes.defineScalar<string> "Identifier"
    let Parameters = Attributes.defineScalar<ExternBindingPatternNode seq> "Parameters"

    let IsStatic = Attributes.defineScalar<bool> "ExternIsStatic"

    let WidgetKey =
        Widgets.register "ModuleDeclAttributes" (fun widget ->
            let xmlDocs = Widgets.tryGetNodeFromWidget widget XmlDocs |> ValueOption.toOption

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map MultipleAttributeListNode.Create
                |> ValueOption.toOption

            let multipleAttributes =
                Widgets.tryGetScalarValue widget AttributesOfType
                |> ValueOption.map MultipleAttributeListNode.Create
                |> ValueOption.toOption

            let tp = Widgets.getNodeFromWidget widget TypeVal

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let name = Widgets.getScalarValue widget Identifier

            let parameters =
                Widgets.tryGetScalarValue widget Parameters
                |> ValueOption.defaultValue []
                |> List.ofSeq

            let staticNode =
                match Widgets.tryGetScalarValue widget IsStatic with
                | ValueSome true -> Some SingleTextNode.``static``
                | _ -> None

            ExternBindingNode(
                xmlDocs,
                attributes,
                staticNode,
                SingleTextNode.``extern``,
                multipleAttributes,
                tp,
                accessControl,
                IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.range0),
                SingleTextNode.leftParenthesis,
                parameters,
                SingleTextNode.rightParenthesis,
                Range.range0
            ))

[<AutoOpen>]
module ExternBindingNodeBuilders =
    type Ast with
        /// <summary>
        /// Create an extern binding with the given type, name, and parameters.
        /// </summary>
        /// <param name="tp">The type of the extern binding.</param>
        /// <param name="name">The name of the extern binding.</param>
        /// <param name="parameters">The parameters of the extern binding.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///    AnonymousModule() {
        ///        ExternBinding(
        ///            LongIdent "void",
        ///            "HelloWorld",
        ///            [ ExternBindingPat("string", "x")
        ///              ExternBindingPat(Int(), "y") ]
        ///        )
        ///    }
        /// }
        /// </code>
        static member ExternBinding
            (tp: WidgetBuilder<Type>, name: string, parameters: WidgetBuilder<ExternBindingPatternNode> seq)
            =
            let parameters = parameters |> Seq.map Gen.mkOak |> Seq.toArray

            WidgetBuilder<ExternBindingNode>(
                ExternBinding.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        ExternBinding.Identifier.WithValue(name),
                        ExternBinding.Parameters.WithValue(parameters)
                    ),
                    [| ExternBinding.TypeVal.WithValue(tp.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>
        /// Create an extern binding with the given type, name, and parameters.
        /// </summary>
        /// <param name="tp">The type of the extern binding.</param>
        /// <param name="name">The name of the extern binding.</param>
        /// <param name="parameters">The parameters of the extern binding.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ExternBinding(
        ///             "void",
        ///             "HelloWorld",
        ///             [ ExternBindingPat("string", "x")
        ///               ExternBindingPat(Int(), "y") ]
        ///         )
        ///     }
        /// }
        /// </code>
        static member ExternBinding(tp: string, name: string, parameters: WidgetBuilder<ExternBindingPatternNode> seq) =
            Ast.ExternBinding(Ast.LongIdent(tp), name, parameters)

        /// <summary>
        /// Create an extern binding with the given type and name.
        /// </summary>
        /// <param name="tp">The type of the extern binding.</param>
        /// <param name="name">The name of the extern binding.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ExternBinding(LongIdent "void", "HelloWorld")
        ///     }
        /// }
        /// </code>
        static member ExternBinding(tp: WidgetBuilder<Type>, name: string) = Ast.ExternBinding(tp, name, [])

        /// <summary>
        /// Create an extern binding with the given type and name.
        /// </summary>
        /// <param name="tp">The type of the extern binding.</param>
        /// <param name="name">The name of the extern binding.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ExternBinding("void", "HelloWorld")
        ///     }
        /// }
        /// </code>
        static member ExternBinding(tp: string, name: string) =
            Ast.ExternBinding(Ast.LongIdent(tp), name, [])

        /// <summary>
        /// Create an extern binding with the given type, name, and parameter.
        /// </summary>
        /// <param name="tp">The type of the extern binding.</param>
        /// <param name="name">The name of the extern binding.</param>
        /// <param name="parameter">The parameter of the extern binding.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ExternBinding(
        ///             LongIdent "void",
        ///             "HelloWorld",
        ///             ExternBindingPat("string", "x")
        ///         )
        ///     }
        /// }
        /// </code>
        static member ExternBinding
            (tp: WidgetBuilder<Type>, name: string, parameter: WidgetBuilder<ExternBindingPatternNode>)
            =
            Ast.ExternBinding(tp, name, [ parameter ])

        /// <summary>
        /// Create an extern binding with the given type, name, and parameter.
        /// </summary>
        /// <param name="tp">The type of the extern binding.</param>
        /// <param name="name">The name of the extern binding.</param>
        /// <param name="parameter">The parameter of the extern binding.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ExternBinding(
        ///             "void",
        ///             "HelloWorld",
        ///             ExternBindingPat("string", "x")
        ///         )
        ///     }
        /// }
        /// </code>
        static member ExternBinding(tp: string, name: string, parameter: WidgetBuilder<ExternBindingPatternNode>) =
            Ast.ExternBinding(Ast.LongIdent(tp), name, [ parameter ])

type ExternBindingNodeModifiers =
    /// <summary>Sets the XmlDocs for the current widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         ExternBinding(
    ///             "void",
    ///             "HelloWorld",
    ///             ExternBindingPat("string", "x")
    ///         )
    ///             .xmlDocs(Summary("This is an ExternBinding"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<ExternBindingNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(ExternBinding.XmlDocs.WithValue(xmlDocs.Compile()))

    /// <summary>Sets the XmlDocs for the current widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         ExternBinding(
    ///             "void",
    ///             "HelloWorld",
    ///             ExternBindingPat("string", "x")
    ///         )
    ///             .xmlDocs([ "This is an ExternBinding" ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<ExternBindingNode>, xmlDocs: string seq) =
        ExternBindingNodeModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    /// <summary>Sets the attributes for the current widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attributes">The attributes to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         ExternBinding(
    ///             "void",
    ///             "HelloWorld",
    ///             ExternBindingPat("string", "x")
    ///         )
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<ExternBindingNode>, attributes: WidgetBuilder<AttributeNode> seq)
        =
        this.AddScalar(ExternBinding.MultipleAttributes.WithValue(attributes |> Seq.map Gen.mkOak |> Seq.toArray))

    /// <summary>
    /// Sets the attribute for the current widget.
    /// </summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attribute">The attribute to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         ExternBinding(
    ///             "void",
    ///             "HelloWorld",
    ///             ExternBindingPat("string", "x")
    ///         )
    ///             .attribute(Attribute("DllImport"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attribute(this: WidgetBuilder<ExternBindingNode>, attribute: WidgetBuilder<AttributeNode>) =
        ExternBindingNodeModifiers.attributes(this, [ attribute ])

    /// <summary>Sets the attributes of the return type, printed between <c>extern</c> and the return type.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attributes">The attributes to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         ExternBinding("bool", "CloseHandle", ExternBindingPat("nativeint", "handle"))
    ///             .attribute(Attribute("DllImport", ParenExpr(String("kernel32.dll"))))
    ///             .returnAttributes([ Attribute("MarshalAs", ParenExpr(Constant("UnmanagedType.Bool"))) ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline returnAttributes
        (this: WidgetBuilder<ExternBindingNode>, attributes: WidgetBuilder<AttributeNode> seq)
        =
        this.AddScalar(ExternBinding.AttributesOfType.WithValue(attributes |> Seq.map Gen.mkOak |> Seq.toArray))

    /// <summary>Sets the attribute of the return type, printed between <c>extern</c> and the return type.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attribute">The attribute to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         ExternBinding("bool", "CloseHandle", ExternBindingPat("nativeint", "handle"))
    ///             .attribute(Attribute("DllImport", ParenExpr(String("kernel32.dll"))))
    ///             .returnAttribute(Attribute("MarshalAs", ParenExpr(Constant("UnmanagedType.Bool"))))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline returnAttribute
        (this: WidgetBuilder<ExternBindingNode>, attribute: WidgetBuilder<AttributeNode>)
        =
        ExternBindingNodeModifiers.returnAttributes(this, [ attribute ])

    /// <summary>Sets the accessibility to private for the current widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         ExternBinding(
    ///             "void",
    ///             "HelloWorld",
    ///             ExternBindingPat("string", "x")
    ///         )
    ///             .toPrivate()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<ExternBindingNode>) =
        this.AddScalar(ExternBinding.Accessibility.WithValue(AccessControl.Private))

    /// <summary>Sets the accessibility to public for the current widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         ExternBinding(
    ///             "void",
    ///             "HelloWorld",
    ///             ExternBindingPat("string", "x")
    ///         )
    ///             .toPublic()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<ExternBindingNode>) =
        this.AddScalar(ExternBinding.Accessibility.WithValue(AccessControl.Public))

    /// <summary>Sets the accessibility to internal for the current widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         ExternBinding(
    ///             "void",
    ///             "HelloWorld",
    ///             ExternBindingPat("string", "x")
    ///         )
    ///             .toInternal()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<ExternBindingNode>) =
        this.AddScalar(ExternBinding.Accessibility.WithValue(AccessControl.Internal))

    /// <summary>
    /// Makes the extern binding a static member (<c>static extern</c>), for declaring it inside a class.
    /// F# rejects <c>static</c> on an extern declared directly in a module.
    /// </summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("Native", UnitPat()) {
    ///             ExternBinding("int", "getpid").attribute(Attribute("DllImport", ParenExpr(String("libc")))).toStatic()
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toStatic(this: WidgetBuilder<ExternBindingNode>) =
        this.AddScalar(ExternBinding.IsStatic.WithValue(true))

type ExternBindingNodeYieldExtensions =
    /// An extern binding inside a class or other type definition (usually <c>static extern</c>).
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<ExternBindingNode>)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.ExternBinding(Gen.mkOak x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: ExternBindingNode) : CollectionContent =
        let moduleDecl = ModuleDecl.ExternBinding x
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ExternBindingNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ExternBindingNodeYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline YieldFrom
        (_: CollectionBuilder<'parent, ModuleDecl>, x: ExternBindingNode seq)
        : CollectionContent =
        let widgets =
            x
            |> Seq.map(fun node ->
                let moduleDecl = ModuleDecl.ExternBinding node
                Ast.EscapeHatch(moduleDecl).Compile())
            |> Seq.toArray
            |> MutStackArray1.fromArray

        { Widgets = widgets }

    [<Extension>]
    static member inline YieldFrom
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ExternBindingNode> seq)
        : CollectionContent =
        let nodes = x |> Seq.map Gen.mkOak |> Seq.toArray
        ExternBindingNodeYieldExtensions.YieldFrom(this, nodes)
