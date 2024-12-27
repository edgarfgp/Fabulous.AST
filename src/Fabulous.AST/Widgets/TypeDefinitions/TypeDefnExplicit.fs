namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module TypeDefnExplicit =
    let Name = Attributes.defineScalar<string> "Name"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let TypeParams = Attributes.defineWidget "TypeParams"

    let Constructor = Attributes.defineWidget "Constructor"
    let XmlDocs = Attributes.defineWidget "XmlDocs"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let Kind = Attributes.defineScalar<SingleTextNode> "Kind"

    let Members = Attributes.defineWidgetCollection "Members"

    let WidgetKey =
        Widgets.register "TypeDefnExplicit" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget XmlDocs
                |> ValueOption.map(fun x -> Some(x))
                |> ValueOption.defaultValue None

            let typeParams =
                Widgets.tryGetNodeFromWidget widget TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let implicitConstructor =
                Widgets.tryGetNodeFromWidget<ImplicitConstructorNode> widget Constructor

            let implicitConstructor =
                match implicitConstructor with
                | ValueNone -> None
                | ValueSome implicitConstructor -> Some implicitConstructor

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let kind = Widgets.getScalarValue widget Kind

            let memDefns =
                Widgets.tryGetNodesFromWidgetCollection widget Members
                |> ValueOption.defaultValue []

            TypeDefnExplicitNode(
                TypeNameNode(
                    xmlDocs,
                    attributes,
                    SingleTextNode.``type``,
                    accessControl,
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero),
                    typeParams,
                    [],
                    implicitConstructor,
                    Some(SingleTextNode.equals),
                    None,
                    Range.Zero
                ),
                TypeDefnExplicitBodyNode(kind, memDefns, SingleTextNode.``end``, Range.Zero),
                [],
                Range.Zero
            ))

[<AutoOpen>]
module TypeDefnExplicitBuilders =
    type Ast with
        static member private BaseClassEnd
            (name: string, constructor: WidgetBuilder<ImplicitConstructorNode> voption, kind: SingleTextNode)
            =
            CollectionBuilder<TypeDefnExplicitNode, MemberDefn>(
                TypeDefnExplicit.WidgetKey,
                TypeDefnExplicit.Members,
                AttributesBundle(
                    StackList.two(TypeDefnExplicit.Name.WithValue(name), TypeDefnExplicit.Kind.WithValue(kind)),
                    [| match constructor with
                       | ValueSome constructor -> TypeDefnExplicit.Constructor.WithValue(constructor.Compile())
                       | ValueNone -> () |],
                    Array.empty
                )
            )

        /// <summary>Create a class end with the given name.</summary>
        /// <param name="name">The name of the class.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ClassEnd("EmptyClass") { }
        ///
        ///         ClassEnd("Class") {
        ///             Member("this.Name", UnitPat(), UnitExpr())
        ///         }
        ///     }
        /// }
        /// </code>
        static member ClassEnd(name: string) =
            Ast.BaseClassEnd(name, ValueNone, SingleTextNode.``class``)

        /// <summary>Create a class end with the given name and constructor.</summary>
        /// <param name="name">The name of the class.</param>
        /// <param name="constructor">The constructor of the class.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///    AnonymousModule() {
        ///         ClassEnd("Class", Constructor(ParameterPat("name", String()))) {
        ///            Member("this.Name", UnitPat(), String("name"))
        ///        }
        ///    }
        /// }
        /// </code>
        static member ClassEnd(name: string, constructor: WidgetBuilder<ImplicitConstructorNode>) =
            Ast.BaseClassEnd(name, ValueSome constructor, SingleTextNode.``class``)

        /// <summary>Create a class end with the given name and constructor.</summary>
        /// <param name="name">The name of the class.</param>
        /// <param name="constructor">The constructor of the class.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         ClassEnd("Class", Constructor(ParameterPat("name", String()))) {
        ///             Member("this.Name", UnitPat(), String("name"))
        ///         }
        ///     }
        /// }
        /// </code>
        static member ClassEnd(name: string, constructor: WidgetBuilder<Pattern>) =
            Ast.BaseClassEnd(name, ValueSome(Ast.Constructor(constructor)), SingleTextNode.``class``)

        /// <summary>Create a struct end with the given name.</summary>
        /// <param name="name">The name of the struct.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         StructEnd("EmptyStruct") { }
        ///     }
        /// }
        /// </code>
        static member StructEnd(name: string) =
            Ast.BaseClassEnd(name, ValueNone, SingleTextNode.``struct``)

        /// <summary>Create a struct end with the given name and constructor.</summary>
        /// <param name="name">The name of the struct.</param>
        /// <param name="constructor">The constructor of the struct.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         StructEnd("Struct", Constructor(ParameterPat("x", Int()))) {
        ///            ValField("x", Int()).toMutable()
        ///            Constructor("x", RecordExpr([ RecordFieldExpr("x", "x") ]))
        ///        }
        ///    }
        /// }
        /// </code>
        static member StructEnd(name: string, constructor: WidgetBuilder<ImplicitConstructorNode>) =
            Ast.BaseClassEnd(name, ValueSome constructor, SingleTextNode.``struct``)

        /// <summary>Create a struct end with the given name and constructor.</summary>
        /// <param name="name">The name of the struct.</param>
        /// <param name="constructor">The constructor of the struct.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         StructEnd("Struct", UnitPat()) {
        ///            ValField("x", Int()).toMutable()
        ///            Constructor("x", RecordExpr([ RecordFieldExpr("x", "x") ]))
        ///        }
        ///    }
        /// }
        /// </code>
        static member StructEnd(name: string, constructor: WidgetBuilder<Pattern>) =
            Ast.BaseClassEnd(name, ValueSome(Ast.Constructor(constructor)), SingleTextNode.``struct``)

        /// <summary>Create an interface end with the given name.</summary>
        /// <param name="name">The name of the interface.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         InterfaceEnd("IMarker") { }
        ///         InterfaceEnd("IMarker") {
        ///             AbstractMember("Name", String())
        ///         }
        ///     }
        /// }
        /// </code>
        static member InterfaceEnd(name: string) =
            Ast.BaseClassEnd(name, ValueNone, SingleTextNode.``interface``)

type TypeDefnExplicitModifiers =
    /// <summary>Sets the XmlDocs for the current widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         StructEnd("Struct", Constructor(ParameterPat("x", Int()))) {
    ///            ValField("x", Int()).toMutable()
    ///            Constructor("x", RecordExpr([ RecordFieldExpr("x", "x") ]))
    ///        }
    ///        |> _.xmlDocs(Summary("This is a struct end"))
    ///    }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnExplicitNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(TypeDefnExplicit.XmlDocs.WithValue(xmlDocs.Compile()))

    /// <summary>Sets the XmlDocs for the current widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         StructEnd("Struct", Constructor(ParameterPat("x", Int()))) {
    ///             ValField("x", Int()).toMutable()
    ///             Constructor("x", RecordExpr([ RecordFieldExpr("x", "x") ]))
    ///         }
    ///         |> _.xmlDocs([ "This is a struct end" ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnExplicitNode>, xmlDocs: string list) =
        TypeDefnExplicitModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    /// <summary>Sets the attributes for the current widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="values">The attributes to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         StructEnd("Struct", Constructor(ParameterPat("x", Int()))) {
    ///             ValField("x", Int()).toMutable()
    ///             Constructor("x", RecordExpr([ RecordFieldExpr("x", "x") ]))
    ///         }
    ///         |> _.attributes([ Attribute("Obsolete") ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<TypeDefnExplicitNode>, values: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            TypeDefnExplicit.MultipleAttributes.WithValue(
                [ for vals in values do
                      Gen.mkOak vals ]
            )
        )

    /// <summary>Sets the attribute for the current widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attribute">The attribute to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         StructEnd("Struct", Constructor(ParameterPat("x", Int()))) {
    ///             ValField("x", Int()).toMutable()
    ///             Constructor("x", RecordExpr([ RecordFieldExpr("x", "x") ]))
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefnExplicitNode>, attribute: WidgetBuilder<AttributeNode>) =
        TypeDefnExplicitModifiers.attributes(this, [ attribute ])

    /// <summary>Sets the type parameters for the current widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="typeParams">The type parameters to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         StructEnd("Struct", Constructor(ParameterPat("x", Int()))) {
    ///             ValField("x", Int()).toMutable()
    ///             Constructor("x", RecordExpr([ RecordFieldExpr("x", "x") ]))
    ///         }
    ///         |> _.typeParams(PostfixList([ "T" ]))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline typeParams(this: WidgetBuilder<TypeDefnExplicitNode>, typeParams: WidgetBuilder<TyparDecls>) =
        this.AddWidget(TypeDefnExplicit.TypeParams.WithValue(typeParams.Compile()))

    /// <summary>Sets the accessibility for the current widget to private.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         StructEnd("Struct", Constructor(ParameterPat("x", Int()))) {
    ///             ValField("x", Int()).toMutable()
    ///             Constructor("x", RecordExpr([ RecordFieldExpr("x", "x") ]))
    ///         }
    ///         |> _.toPrivate()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<TypeDefnExplicitNode>) =
        this.AddScalar(TypeDefnExplicit.Accessibility.WithValue(AccessControl.Private))

    /// <summary>Sets the accessibility for the current widget to public.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         StructEnd("Struct", Constructor(ParameterPat("x", Int()))) {
    ///             ValField("x", Int()).toMutable()
    ///             Constructor("x", RecordExpr([ RecordFieldExpr("x", "x") ]))
    ///         }
    ///         |> _.toPublic()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<TypeDefnExplicitNode>) =
        this.AddScalar(TypeDefnExplicit.Accessibility.WithValue(AccessControl.Public))

    /// <summary>Sets the accessibility for the current widget to internal.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         StructEnd("Struct", Constructor(ParameterPat("x", Int()))) {
    ///             ValField("x", Int()).toMutable()
    ///             Constructor("x", RecordExpr([ RecordFieldExpr("x", "x") ]))
    ///         }
    ///         |> _.toInternal()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<TypeDefnExplicitNode>) =
        this.AddScalar(TypeDefnExplicit.Accessibility.WithValue(AccessControl.Internal))

type TypeDefnExplicitYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: TypeDefnExplicitNode) : CollectionContent =
        let typeDefn = TypeDefn.Explicit(x)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefnExplicitNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        TypeDefnExplicitYieldExtensions.Yield(this, node)
