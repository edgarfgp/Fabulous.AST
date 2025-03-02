namespace Fabulous.AST

open System
open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module AutoPropertyMember =
    let XmlDocs = Attributes.defineWidget "XmlDocs"
    let Identifier = Attributes.defineScalar<string> "Identifier"
    let ReturnType = Attributes.defineWidget "Type"
    let Parameters = Attributes.defineScalar<MethodParamsType> "Parameters"
    let HasGetter = Attributes.defineScalar<bool> "HasGetter"
    let HasSetter = Attributes.defineScalar<bool> "HasSetter"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let IsStatic = Attributes.defineScalar<bool> "IsStatic"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"
    let BodyExpr = Attributes.defineWidget "BodyExpr"

    let WidgetKey =
        Widgets.register "AutoPropertyMember" (fun widget ->
            let identifier = Widgets.getScalarValue widget Identifier
            let hasGetter = Widgets.getScalarValue widget HasGetter
            let hasSetter = Widgets.getScalarValue widget HasSetter

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let isStatic =
                Widgets.tryGetScalarValue widget IsStatic |> ValueOption.defaultValue false

            let returnType = Widgets.tryGetNodeFromWidget widget ReturnType

            let bodyExpr = Widgets.getNodeFromWidget widget BodyExpr

            let returnType =
                match returnType with
                | ValueSome tp -> Some tp
                | ValueNone -> None

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let multipleTextsNode =
                MultipleTextsNode(
                    [ if isStatic then
                          SingleTextNode.``static``
                          SingleTextNode.``member``
                          SingleTextNode.``val``
                      else
                          SingleTextNode.``member``
                          SingleTextNode.``val`` ],
                    Range.Zero
                )

            let withGetSetText =
                match hasGetter, hasSetter with
                | true, true ->
                    Some(
                        MultipleTextsNode.Create(
                            [ SingleTextNode.``with``; SingleTextNode.Create("get,"); SingleTextNode.set ]
                        )
                    )
                | true, false -> Some(MultipleTextsNode.Create([ SingleTextNode.``with``; SingleTextNode.get ]))
                | false, true -> Some(MultipleTextsNode.Create([ SingleTextNode.``with``; SingleTextNode.set ]))
                | false, false -> None

            MemberDefnAutoPropertyNode(
                xmlDocs,
                attributes,
                multipleTextsNode,
                accessControl,
                SingleTextNode.Create(identifier),
                returnType,
                SingleTextNode.equals,
                bodyExpr,
                withGetSetText,
                Range.Zero
            ))

[<AutoOpen>]
module AutoPropertyMemberBuilders =
    type Ast with
        static member private BaseMemberVal
            (
                identifier: string,
                expr: WidgetBuilder<Expr>,
                ?hasGetter: bool,
                ?hasSetter: bool,
                ?returnType: WidgetBuilder<Type>
            ) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false

            WidgetBuilder<MemberDefnAutoPropertyNode>(
                AutoPropertyMember.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        AutoPropertyMember.Identifier.WithValue(identifier),
                        AutoPropertyMember.HasGetter.WithValue(hasGetter),
                        AutoPropertyMember.HasSetter.WithValue(hasSetter)
                    ),
                    [| AutoPropertyMember.BodyExpr.WithValue(expr.Compile())
                       match returnType with
                       | None -> ()
                       | Some returnType -> AutoPropertyMember.ReturnType.WithValue(returnType.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>
        /// Create an auto property member definition.
        /// </summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="expr">The expression of the member.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             MemberVal("Name", ConstantExpr(Constant("name")), true, true)
        ///             MemberVal("Age", ConstantExpr(Constant("age")), true, true)
        ///         }
        ///     }
        /// }
        /// </code>
        static member MemberVal(identifier: string, expr: WidgetBuilder<Expr>, ?hasGetter: bool, ?hasSetter: bool) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            Ast.BaseMemberVal(identifier, expr, hasGetter, hasSetter)

        static member MemberVal
            (
                identifier: string,
                expr: WidgetBuilder<Expr>,
                returnType: WidgetBuilder<Type>,
                ?hasGetter: bool,
                ?hasSetter: bool
            ) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            Ast.BaseMemberVal(identifier, expr, hasGetter, hasSetter, returnType)

        static member MemberVal
            (identifier: string, expr: WidgetBuilder<Expr>, returnType: string, ?hasGetter: bool, ?hasSetter: bool) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            Ast.BaseMemberVal(identifier, expr, hasGetter, hasSetter, Ast.LongIdent(returnType))

        /// <summary>Create an auto property member definition.</summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="expr">The expression of the member.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             MemberVal("Name", Constant("name"), true, true)
        ///             MemberVal("Age", Constant("age"), true, true)
        ///         }
        ///     }
        /// }
        /// </code>
        static member MemberVal(identifier: string, expr: WidgetBuilder<Constant>, ?hasGetter: bool, ?hasSetter: bool) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            Ast.MemberVal(identifier, Ast.ConstantExpr(expr), hasGetter, hasSetter)

        static member MemberVal
            (
                identifier: string,
                expr: WidgetBuilder<Constant>,
                returnType: WidgetBuilder<Type>,
                ?hasGetter: bool,
                ?hasSetter: bool
            ) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            Ast.BaseMemberVal(identifier, Ast.ConstantExpr(expr), hasGetter, hasSetter, returnType)

        static member MemberVal
            (identifier: string, expr: WidgetBuilder<Constant>, returnType: string, ?hasGetter: bool, ?hasSetter: bool) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            Ast.BaseMemberVal(identifier, Ast.ConstantExpr(expr), hasGetter, hasSetter, Ast.LongIdent(returnType))

        /// <summary>Create an auto property member definition.</summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="expr">The expression of the member.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("Person", UnitPat()) {
        ///             MemberVal("Name", "name", true, true)
        ///             MemberVal("Age", "age", true, true)
        ///        }
        ///     }
        /// }
        /// </code>
        static member MemberVal(identifier: string, expr: string, ?hasGetter: bool, ?hasSetter: bool) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            Ast.MemberVal(identifier, Ast.Constant(expr), hasGetter, hasSetter)

        static member MemberVal
            (identifier: string, expr: string, returnType: WidgetBuilder<Type>, ?hasGetter: bool, ?hasSetter: bool) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            Ast.BaseMemberVal(identifier, Ast.ConstantExpr(expr), hasGetter, hasSetter, returnType)

        static member MemberVal
            (identifier: string, expr: string, returnType: string, ?hasGetter: bool, ?hasSetter: bool)
            =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            Ast.BaseMemberVal(identifier, Ast.ConstantExpr(expr), hasGetter, hasSetter, Ast.LongIdent(returnType))

type AutoPropertyMemberModifiers =
    /// <summary>Sets the XmlDocs for the current widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("Person", UnitPat()) {
    ///             MemberVal("Name", "name", true, true)
    ///                 .xmlDocs(Summary("The name of the person"))
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<MemberDefnAutoPropertyNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(AutoPropertyMember.XmlDocs.WithValue(xmlDocs.Compile()))

    /// <summary>Sets the XmlDocs for the current widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("Person", UnitPat()) {
    ///             MemberVal("Name", "name", true, true)
    ///                 .xmlDocs([ "The name of the person" ])
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<MemberDefnAutoPropertyNode>, xmlDocs: string list) =
        AutoPropertyMemberModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    /// <summary>Sets the attributes for the current widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attributes">The attributes to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("Person", UnitPat()) {
    ///             MemberVal("Name", "name", true, true)
    ///                 .attributes([ Attribute("Obsolete") ])
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<MemberDefnAutoPropertyNode>, attributes: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(AutoPropertyMember.MultipleAttributes.WithValue(attributes |> List.map Gen.mkOak))

    /// <summary>Sets the attribute for the current widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="value">The attribute to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("Person", UnitPat()) {
    ///             MemberVal("Name", "name", true, true)
    ///                 .attribute(Attribute("Obsolete"))
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attribute
        (this: WidgetBuilder<MemberDefnAutoPropertyNode>, value: WidgetBuilder<AttributeNode>)
        =
        AutoPropertyMemberModifiers.attributes(this, [ value ])

    /// <summary>Sets the member to be static.</summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("Person", UnitPat()) {
    ///             MemberVal("Name", "name", true, true)
    ///                 .toStatic()
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toStatic(this: WidgetBuilder<MemberDefnAutoPropertyNode>) =
        this.AddScalar(AutoPropertyMember.IsStatic.WithValue(true))

    /// <summary>Sets the member to be private.</summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("Person", UnitPat()) {
    ///             MemberVal("Name", "name", true, true)
    ///                 .toPrivate()
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<MemberDefnAutoPropertyNode>) =
        this.AddScalar(AutoPropertyMember.Accessibility.WithValue(AccessControl.Private))

    /// <summary>Sets the member to be public.</summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("Person", UnitPat()) {
    ///             MemberVal("Name", "name", true, true)
    ///                 .toPublic()
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<MemberDefnAutoPropertyNode>) =
        this.AddScalar(AutoPropertyMember.Accessibility.WithValue(AccessControl.Public))

    /// <summary>Sets the member to be internal.</summary>
    /// <param name="this">Current widget.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("Person", UnitPat()) {
    ///             MemberVal("Name", "name", true, true)
    ///                 .toInternal()
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<MemberDefnAutoPropertyNode>) =
        this.AddScalar(AutoPropertyMember.Accessibility.WithValue(AccessControl.Internal))

    /// <summary>Sets the return type for the member.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="value">The return type to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("Person", UnitPat()) {
    ///             MemberVal("Name", "name", true, true)
    ///                 .returnType(String())
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    [<Obsolete("Use the overload that takes a widget in the constructor instead.")>]
    static member inline returnType(this: WidgetBuilder<MemberDefnAutoPropertyNode>, value: WidgetBuilder<Type>) =
        this.AddWidget(AutoPropertyMember.ReturnType.WithValue(value.Compile()))

    /// <summary>Sets the return type for the member.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="value">The return type to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("Person", UnitPat()) {
    ///             MemberVal("Name", "name", true, true)
    ///                 .returnType("string")
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    [<Obsolete("Use the overload that takes a widget in the constructor instead.")>]
    static member inline returnType(this: WidgetBuilder<MemberDefnAutoPropertyNode>, value: string) =
        AutoPropertyMemberModifiers.returnType(this, Ast.LongIdent(value))
