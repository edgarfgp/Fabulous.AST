namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Val =
    let LeadingKeyword = Attributes.defineScalar<SingleTextNode seq> "LeadingKeyword"

    let Identifier = Attributes.defineScalar<string> "Identifier"

    let ReturnType = Attributes.defineWidget "Identifier"

    let XmlDocs = Attributes.defineWidget "ValXmlDocs"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode seq> "ValMultipleAttributes"

    let Accessibility = Attributes.defineScalar<AccessControl> "ValAccessibility"

    let TypeParams = Attributes.defineWidget "ValTypeParams"

    let IsMutable = Attributes.defineScalar<bool> "ValIsMutable"

    let IsInlined = Attributes.defineScalar<bool> "ValIsInlined"

    let WidgetKey =
        Widgets.register "ValNode" (fun widget ->
            let xmlDocs =
                Widgets.tryGetNodeFromWidget<XmlDocNode> widget XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let inlined =
                Widgets.tryGetScalarValue widget IsInlined
                |> ValueOption.map(fun _ -> Some(SingleTextNode.``inline``))
                |> ValueOption.defaultValue None

            let isMutable =
                Widgets.tryGetScalarValue widget IsMutable |> ValueOption.defaultValue(false)

            let identifier = Widgets.getScalarValue widget Identifier
            let identifier = SingleTextNode.Create(identifier)

            let returnType = Widgets.getNodeFromWidget widget ReturnType

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let typeParams =
                Widgets.tryGetNodeFromWidget widget TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let leadingKeyword =
                Widgets.tryGetScalarValue widget LeadingKeyword
                |> ValueOption.map(List.ofSeq)
                |> ValueOption.filter(_.IsEmpty >> not)
                |> ValueOption.map(fun nodes -> Some(MultipleTextsNode(nodes, Range.Zero)))
                |> ValueOption.defaultValue None

            ValNode(
                xmlDocs,
                attributes,
                leadingKeyword,
                inlined,
                isMutable,
                accessControl,
                identifier,
                typeParams,
                returnType,
                None,
                None,
                Range.Zero
            ))

[<AutoOpen>]
module ValBuilders =
    type Ast with
        static member private BaseVal
            (leadingKeyword: SingleTextNode seq, identifier: string, returnType: WidgetBuilder<Type>)
            =
            WidgetBuilder<ValNode>(
                Val.WidgetKey,
                AttributesBundle(
                    StackList.two(Val.Identifier.WithValue(identifier), Val.LeadingKeyword.WithValue(leadingKeyword)),
                    [| Val.ReturnType.WithValue(returnType.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>Creates a Val widget with an identifier and a return type.</summary>
        /// <param name="identifier">The identifier of the Val.</param>
        /// <param name="returnType">The return type of the Val.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Val("x", "int")
        ///     }
        /// }
        /// </code>
        static member Val(identifier: string, returnType: WidgetBuilder<Type>) =
            Ast.BaseVal([ SingleTextNode.``val`` ], identifier, returnType)

        /// <summary>Creates a Val widget with an identifier and a return type.</summary>
        /// <param name="identifier">The identifier of the Val.</param>
        /// <param name="returnType">The return type of the Val.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Val("x", "int")
        ///     }
        /// }
        /// </code>
        static member Val(identifier: string, returnType: string) =
            Ast.Val(identifier, Ast.LongIdent(returnType))

        /// <summary>Creates a Val widget with a leading keyword, an identifier, and a return type.</summary>
        /// <param name="leadingKeyword">The leading keyword of the Val.</param>
        /// <param name="identifier">The identifier of the Val.</param>
        /// <param name="returnType">The return type of the Val.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Val([ "static"; "member" ], "x", Int())
        ///     }
        /// }
        /// </code>
        static member Val(leadingKeyword: string seq, identifier: string, returnType: WidgetBuilder<Type>) =
            Ast.BaseVal([ for kw in leadingKeyword -> SingleTextNode.Create(kw) ], identifier, returnType)

        /// <summary>Creates a Val widget with a leading keyword, an identifier, and a return type.</summary>
        /// <param name="leadingKeyword">The leading keyword of the Val.</param>
        /// <param name="identifier">The identifier of the Val.</param>
        /// <param name="returnType">The return type of the Val.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Val([ "static"; "member" ], "x", "int")
        ///     }
        /// }
        /// </code>
        static member Val(leadingKeyword: string seq, identifier: string, returnType: string) =
            Ast.BaseVal(
                [ for kw in leadingKeyword -> SingleTextNode.Create(kw) ],
                identifier,
                Ast.LongIdent(returnType)
            )

        /// <summary>Creates a Val widget with a leading keyword, an identifier, and a return type.</summary>
        /// <param name="leadingKeyword">The leading keyword of the Val.</param>
        /// <param name="identifier">The identifier of the Val.</param>
        /// <param name="returnType">The return type of the Val.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Val("static", "x", Int())
        ///     }
        /// }
        /// </code>
        static member Val(leadingKeyword: string, identifier: string, returnType: WidgetBuilder<Type>) =
            Ast.BaseVal([ SingleTextNode.Create(leadingKeyword) ], identifier, returnType)

        /// <summary>Creates a Val widget with a leading keyword, an identifier, and a return type.</summary>
        /// <param name="leadingKeyword">The leading keyword of the Val.</param>
        /// <param name="identifier">The identifier of the Val.</param>
        /// <param name="returnType">The return type of the Val.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Val("static", "x", "int")
        ///     }
        /// }
        /// </code>
        static member Val(leadingKeyword: string, identifier: string, returnType: string) =
            Ast.BaseVal([ SingleTextNode.Create(leadingKeyword) ], identifier, Ast.LongIdent(returnType))

type ValNodeModifiers =
    /// <summary>Sets the XmlDocs for the current Val.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Val("x", "int")
    ///             .xmlDocs(Summary("This is a Val"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<ValNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(Val.XmlDocs.WithValue(xmlDocs.Compile()))

    /// <summary>Sets the XmlDocs for the current Val.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Val("x", "int")
    ///             .xmlDocs([ "This is a Val" ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<ValNode>, xmlDocs: string seq) =
        ValNodeModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    /// <summary>Sets the Val to be mutable.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Val("x", "int")
    ///             .toMutable()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toMutable(this: WidgetBuilder<ValNode>) =
        this.AddScalar(Val.IsMutable.WithValue(true))

    /// <summary>Sets the Val to be inlined.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Val("x", "int")
    ///             .toInlined()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toInlined(this: WidgetBuilder<ValNode>) =
        this.AddScalar(Val.IsInlined.WithValue(true))

    /// <summary>Sets the attributes for the current Val.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attributes">The attributes to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///        Value("x", "1")
    ///             .attributes([ Attribute("DefaultValue") ])
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attributes(this: WidgetBuilder<ValNode>, attributes: WidgetBuilder<AttributeNode> seq) =
        this.AddScalar(Val.MultipleAttributes.WithValue(attributes |> Seq.map Gen.mkOak))

    /// <summary>Sets the attribute for the current Val.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attribute">The attribute to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Val("x", "int")
    ///             .attribute(Attribute("DefaultValue"))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attribute(this: WidgetBuilder<ValNode>, attribute: WidgetBuilder<AttributeNode>) =
        ValNodeModifiers.attributes(this, [ attribute ])

    /// <summary>Sets the Val to be private.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Val("x", "int")
    ///             .toPrivate()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<ValNode>) =
        this.AddScalar(Val.Accessibility.WithValue(AccessControl.Private))

    /// <summary>Sets the Val to be public.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Val("x", "int")
    ///             .toPublic()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<ValNode>) =
        this.AddScalar(Val.Accessibility.WithValue(AccessControl.Public))

    /// <summary>Sets the Val to be internal.</summary>
    /// <param name="this">Current widget.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Val("x", "int")
    ///             .toInternal()
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<ValNode>) =
        this.AddScalar(Val.Accessibility.WithValue(AccessControl.Internal))

    /// <summary>Sets the type parameters for the current Val.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="typeParams">The type parameters to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Val("x", "int")
    ///             .typeParams(PostfixList([ "'a"; "'b" ]))
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline typeParams(this: WidgetBuilder<ValNode>, typeParams: WidgetBuilder<TyparDecls>) =
        this.AddWidget(Val.TypeParams.WithValue(typeParams.Compile()))

type ValYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: ValNode) : CollectionContent =
        let moduleDecl = ModuleDecl.Val x
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ValNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ValYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline YieldFrom(_: CollectionBuilder<'parent, ModuleDecl>, x: ValNode seq) : CollectionContent =
        let widgets =
            x
            |> Seq.map(fun node ->
                let moduleDecl = ModuleDecl.Val node
                Ast.EscapeHatch(moduleDecl).Compile())
            |> Seq.toArray
            |> MutStackArray1.fromArray

        { Widgets = widgets }

    [<Extension>]
    static member inline YieldFrom
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ValNode> seq)
        : CollectionContent =
        let nodes = x |> Seq.map Gen.mkOak
        ValYieldExtensions.YieldFrom(this, nodes)
