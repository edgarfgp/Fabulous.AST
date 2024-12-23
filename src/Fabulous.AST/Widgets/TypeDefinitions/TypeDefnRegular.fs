namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module TypeDefnRegular =
    let Name = Attributes.defineScalar<string> "Name"
    let ImplicitConstructor = Attributes.defineWidget "SimplePats"
    let Members = Attributes.defineWidgetCollection "Members"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let XmlDocs = Attributes.defineWidget "XmlDocs"
    let TypeParams = Attributes.defineWidget "TypeParams"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let IsRecursive = Attributes.defineScalar<bool> "IsRecursive"

    let WidgetKey =
        Widgets.register "TypeDefnRegular" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

            let constructor =
                Widgets.tryGetNodeFromWidget<ImplicitConstructorNode> widget ImplicitConstructor

            let members =
                Widgets.tryGetNodesFromWidgetCollection<MemberDefn> widget Members
                |> ValueOption.defaultValue []

            let typeParams =
                Widgets.tryGetNodeFromWidget widget TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget XmlDocs
                |> ValueOption.map(fun x -> Some(x))
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let constructor =
                match constructor with
                | ValueNone -> None
                | ValueSome constructor -> Some constructor

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

            TypeDefnRegularNode(
                TypeNameNode(
                    xmlDocs,
                    attributes,
                    leadingKeyword,
                    accessControl,
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero),
                    typeParams,
                    [],
                    constructor,
                    Some(SingleTextNode.equals),
                    None,
                    Range.Zero
                ),
                members,
                Range.Zero
            ))

[<AutoOpen>]
module TypeDefnRegularBuilders =
    type Ast with
        static member BaseTypeDefn(name: string, constructor: WidgetBuilder<ImplicitConstructorNode> voption) =
            CollectionBuilder<TypeDefnRegularNode, MemberDefn>(
                TypeDefnRegular.WidgetKey,
                TypeDefnRegular.Members,
                AttributesBundle(
                    StackList.one(TypeDefnRegular.Name.WithValue(name)),
                    [| match constructor with
                       | ValueSome constructor -> TypeDefnRegular.ImplicitConstructor.WithValue(constructor.Compile())
                       | ValueNone -> () |],
                    Array.empty
                )
            )

        /// <summary>Create a regular type definition with the given name.</summary>
        /// <param name="name">The name of the type definition.</param>
        /// <param name="parameters">The parameters of the type definition.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///    AnonymousModule() {
        ///        TypeDefn("Person", ParenPat()) {
        ///            Member(
        ///                "this.Name",
        ///                ParenPat(ParameterPat(ConstantPat(Constant("p")), String())),
        ///                ConstantExpr(Int 23)
        ///            )
        ///        }
        ///
        ///        TypeDefn("IFoo") {
        ///             AbstractMember("Name", String())
        ///        }
        ///    }
        ///}
        /// </code>
        static member TypeDefn(name: string, parameters: WidgetBuilder<Pattern>) =
            Ast.BaseTypeDefn(name, ValueSome(Ast.Constructor parameters))

        /// <summary>Create a regular type definition with the given name.</summary>
        /// <param name="name">The name of the type definition.</param>
        /// <param name="constructor">The constructor of the type definition.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn(
        ///             "Person",
        ///              Constructor(
        ///                  ParenPat(
        ///                     TuplePat(
        ///                         [ ParameterPat(ConstantPat(Constant("name")), String())
        ///                           ParameterPat(ConstantPat(Constant("age")), Int()) ])
        ///                     )
        ///                 )
        ///             ) {
        ///                   MemberVal("Name", ConstantExpr(Constant("name")), true, true)
        ///                  MemberVal("Age", ConstantExpr(Constant("age")), true, true)
        ///              }
        ///     }
        /// }
        /// </code>
        static member TypeDefn(name: string, constructor: WidgetBuilder<ImplicitConstructorNode>) =
            Ast.BaseTypeDefn(name, ValueSome constructor)

        /// <summary>Create a regular type definition with the given name.</summary>
        /// <param name="name">The name of the type definition.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("ITriangle") {
        ///            AbstractMember("Area", Float(), true)
        ///         }
        ///    }
        /// }
        /// </code>
        static member TypeDefn(name: string) = Ast.BaseTypeDefn(name, ValueNone)

type TypeDefnRegularModifiers =
    /// <summary>Sets the XmlDocs for the current type definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("ITriangle") {
    ///             AbstractMember("Area", Float(), true)
    ///         }
    ///         |> _.xmlDocs(Summary("This is a triangle"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnRegularNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(TypeDefnRegular.XmlDocs.WithValue(xmlDocs.Compile()))

    /// <summary>Sets the XmlDocs for the current type definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("ITriangle") {
    ///             AbstractMember("Area", Float(), true)
    ///         }
    ///         |> _.xmlDocs([ "This is a triangle" ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnRegularNode>, xmlDocs: string list) =
        TypeDefnRegularModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    /// <summary>Sets the type parameters for the current type definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="typeParams">The type parameters to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("ITriangle") {
    ///             AbstractMember("Area", Float(), true)
    ///         }
    ///         |> _.typeParams(PostfixList([ "'other"; "'another" ]))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline typeParams(this: WidgetBuilder<TypeDefnRegularNode>, typeParams: WidgetBuilder<TyparDecls>) =
        this.AddWidget(TypeDefnRegular.TypeParams.WithValue(typeParams.Compile()))

    /// <summary>Sets the attributes for the current type definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attributes">The attributes to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("ITriangle") {
    ///             AbstractMember("Area", Float(), true)
    ///         }
    ///         |> _.attributes([ Attribute("Obsolete") ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<TypeDefnRegularNode>, attributes: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            TypeDefnRegular.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    /// <summary>Sets the attributes for the current type definition.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attribute">The attributes to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("ITriangle") {
    ///             AbstractMember("Area", Float(), true)
    ///         }
    ///         |> _.attribute(Attribute("Obsolete"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefnRegularNode>, attribute: WidgetBuilder<AttributeNode>) =
        TypeDefnRegularModifiers.attributes(this, [ attribute ])

    /// <summary>Sets the type definition to be private.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("ITriangle") {
    ///             AbstractMember("Area", Float(), true)
    ///         }
    ///         |> _.toPrivate()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<TypeDefnRegularNode>) =
        this.AddScalar(TypeDefnRegular.Accessibility.WithValue(AccessControl.Private))

    /// <summary>Sets the type definition to be public.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("ITriangle") {
    ///             AbstractMember("Area", Float(), true)
    ///         }
    ///         |> _.toPublic()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<TypeDefnRegularNode>) =
        this.AddScalar(TypeDefnRegular.Accessibility.WithValue(AccessControl.Public))

    /// <summary>Sets the type definition to be internal.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("ITriangle") {
    ///             AbstractMember("Area", Float(), true)
    ///         }
    ///         |> _.toInternal()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<TypeDefnRegularNode>) =
        this.AddScalar(TypeDefnRegular.Accessibility.WithValue(AccessControl.Internal))

    /// <summary>Sets the type definition to be recursive.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("ITriangle") {
    ///             AbstractMember("Area", Float(), true)
    ///         }
    ///         |> _.toRecursive()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toRecursive(this: WidgetBuilder<TypeDefnRegularNode>) =
        this.AddScalar(TypeDefnRegular.IsRecursive.WithValue(true))

type TypeDefnRegularYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: TypeDefnRegularNode) : CollectionContent =
        let typeDefn = TypeDefn.Regular(x)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)

        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefnRegularNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        TypeDefnRegularYieldExtensions.Yield(this, node)
