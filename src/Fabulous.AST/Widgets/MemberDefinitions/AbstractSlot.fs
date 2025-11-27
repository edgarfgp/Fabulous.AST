namespace Fabulous.AST

open System.Linq
open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module AbstractSlot =
    let XmlDocs = Attributes.defineWidget "XmlDocs"
    let Identifier = Attributes.defineScalar<string> "Identifier"
    let ReturnType = Attributes.defineWidget "Type"
    let Parameters = Attributes.defineScalar<MethodParamsType> "Parameters"

    let HasGetterSetter =
        Attributes.defineScalar<(bool * AccessControl) * (bool * AccessControl)> "HasGetter"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode seq> "MultipleAttributes"

    let TypeParams = Attributes.defineWidget "TypeParams"

    let WidgetKey =
        Widgets.register "AbstractMember" (fun widget ->
            let identifier = Widgets.getScalarValue widget Identifier
            let returnType = Widgets.getNodeFromWidget widget ReturnType
            let parameters = Widgets.tryGetScalarValue widget Parameters
            let hasGetter, hasSetter = Widgets.getScalarValue widget HasGetterSetter

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let returnType =
                match parameters with
                | ValueNone -> [], returnType
                | ValueSome(UnNamed(parameters, isTupled)) ->
                    let parameters =
                        parameters
                        |> Seq.mapi(fun index value ->
                            let separator =
                                if index < Seq.length parameters - 1 && isTupled then
                                    SingleTextNode.star
                                else
                                    SingleTextNode.rightArrow

                            (Gen.mkOak value, separator))

                    List.ofSeq parameters, returnType
                | ValueSome(Named(parameters, isTupled)) ->
                    let parameters =
                        parameters
                        |> Seq.mapi(fun index (name, value) ->
                            let separator =
                                if index < Seq.length parameters - 1 && isTupled then
                                    SingleTextNode.star
                                else
                                    SingleTextNode.rightArrow

                            if System.String.IsNullOrEmpty(name) then
                                failwith "Named parameters must have a name"

                            let value =
                                Type.SignatureParameter(
                                    TypeSignatureParameterNode(
                                        None,
                                        Some(SingleTextNode.Create(name)),
                                        Gen.mkOak(value),
                                        Range.Zero
                                    )
                                )

                            (value, separator))

                    List.ofSeq parameters, returnType

            let withGetSetText =
                match hasGetter, hasSetter with
                | (true, getterAccessibility), (true, setterAccessibility) ->
                    Some(
                        MultipleTextsNode.Create(
                            [ SingleTextNode.``with``
                              // Getter
                              match getterAccessibility with
                              | Public -> SingleTextNode.``public``
                              | Private -> SingleTextNode.``private``
                              | Internal -> SingleTextNode.``internal``
                              | Unknown -> ()
                              SingleTextNode.Create("get,")
                              // Setter
                              match setterAccessibility with
                              | Public -> SingleTextNode.``public``
                              | Private -> SingleTextNode.``private``
                              | Internal -> SingleTextNode.``internal``
                              | Unknown -> ()
                              SingleTextNode.set ]
                        )
                    )
                | (true, getterAccessibility), (false, _) ->
                    Some(
                        MultipleTextsNode.Create(
                            [ SingleTextNode.``with``
                              match getterAccessibility with
                              | Public -> SingleTextNode.``public``
                              | Private -> SingleTextNode.``private``
                              | Internal -> SingleTextNode.``internal``
                              | Unknown -> ()
                              SingleTextNode.get ]
                        )
                    )
                | (false, _), (true, setterAccessibility) ->
                    Some(
                        MultipleTextsNode.Create(
                            [ SingleTextNode.``with``
                              match setterAccessibility with
                              | Public -> SingleTextNode.``public``
                              | Private -> SingleTextNode.``private``
                              | Internal -> SingleTextNode.``internal``
                              | Unknown -> ()
                              SingleTextNode.set ]
                        )
                    )
                | (false, _), (false, _) -> None

            let returnType =
                match returnType with
                | [], returnType -> returnType
                | parameters, returnType -> Type.Funs(TypeFunsNode(parameters, returnType, Range.Zero))

            let leadingKeywords = MultipleTextsNode.Create([ SingleTextNode.``abstract`` ])

            let typeParams =
                Widgets.tryGetNodeFromWidget widget TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            MemberDefnAbstractSlotNode(
                xmlDocs,
                attributes,
                leadingKeywords,
                SingleTextNode.Create(identifier),
                typeParams,
                returnType,
                withGetSetText,
                Range.Zero
            ))

[<AutoOpen>]
module AbstractMemberBuilders =
    type Ast with
        /// <summary>Creates an abstract member.</summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="returnType">The return type of the member.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
        /// <param name="getterAccessibility">The accessibility of the getter.</param>
        /// <param name="setterAccessibility">The accessibility of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("ICircle") {
        ///            AbstractMember("Area", Float(), true)
        ///         }
        ///     }
        /// }
        /// </code>
        static member AbstractMember
            (
                identifier: string,
                returnType: WidgetBuilder<Type>,
                ?hasGetter: bool,
                ?hasSetter: bool,
                ?getterAccessibility: AccessControl,
                ?setterAccessibility: AccessControl
            ) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let getterAccessibility = defaultArg getterAccessibility AccessControl.Unknown
            let setterAccessibility = defaultArg setterAccessibility AccessControl.Unknown

            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractSlot.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        AbstractSlot.Identifier.WithValue(identifier),
                        AbstractSlot.HasGetterSetter.WithValue(
                            (hasGetter, getterAccessibility),
                            (hasSetter, setterAccessibility)
                        )
                    ),
                    [| AbstractSlot.ReturnType.WithValue(returnType.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>Creates an abstract member.</summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="returnType">The return type of the member.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
        /// <param name="getterAccessibility">The accessibility of the getter.</param>
        /// <param name="setterAccessibility">The accessibility of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("ICircle") {
        ///             AbstractMember("Area", "float", true)
        ///         }
        ///     }
        /// }
        /// </code>
        static member AbstractMember
            (
                identifier: string,
                returnType: string,
                ?hasGetter: bool,
                ?hasSetter: bool,
                ?getterAccessibility: AccessControl,
                ?setterAccessibility: AccessControl
            ) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let getterAccessibility = defaultArg getterAccessibility AccessControl.Unknown
            let setterAccessibility = defaultArg setterAccessibility AccessControl.Unknown

            Ast.AbstractMember(
                identifier,
                Ast.LongIdent(returnType),
                hasGetter,
                hasSetter,
                getterAccessibility,
                setterAccessibility
            )

        /// <summary>Creates an abstract member with parameters.</summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="parameters">The parameters of the member.</param>
        /// <param name="returnType">The return type of the member.</param>
        /// <param name="isTupled">Whether the parameters are tupled.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
        /// <param name="getterAccessibility">The accessibility of the getter.</param>
        /// <param name="setterAccessibility">The accessibility of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("ICircle") {
        ///             AbstractMember("Add", [ LongIdent "int"; LongIdent "int" ], LongIdent "int", true)
        ///         }
        ///     }
        /// }
        /// </code>
        static member AbstractMember
            (
                identifier: string,
                parameters: WidgetBuilder<Type> seq,
                returnType: WidgetBuilder<Type>,
                ?isTupled: bool,
                ?hasGetter: bool,
                ?hasSetter: bool,
                ?getterAccessibility: AccessControl,
                ?setterAccessibility: AccessControl
            ) =
            let isTupled = defaultArg isTupled false
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let getterAccessibility = defaultArg getterAccessibility AccessControl.Unknown
            let setterAccessibility = defaultArg setterAccessibility AccessControl.Unknown

            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractSlot.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        AbstractSlot.Identifier.WithValue(identifier),
                        AbstractSlot.HasGetterSetter.WithValue(
                            (hasGetter, getterAccessibility),
                            (hasSetter, setterAccessibility)
                        ),
                        AbstractSlot.Parameters.WithValue(UnNamed(parameters, isTupled))
                    ),
                    [| AbstractSlot.ReturnType.WithValue(returnType.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>Creates an abstract member with parameters.</summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="parameters">The parameters of the member.</param>
        /// <param name="returnType">The return type of the member.</param>
        /// <param name="isTupled">Whether the parameters are tupled.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
        /// <param name="getterAccessibility">The accessibility of the getter.</param>
        /// <param name="setterAccessibility">The accessibility of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("ICircle") {
        ///             AbstractMember("Add", [ "int"; "int" ], "int", true)
        ///         }
        ///     }
        /// }
        /// </code>
        static member AbstractMember
            (
                identifier: string,
                parameters: string seq,
                returnType: WidgetBuilder<Type>,
                ?isTupled: bool,
                ?hasGetter: bool,
                ?hasSetter: bool,
                ?getterAccessibility: AccessControl,
                ?setterAccessibility: AccessControl
            ) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let getterAccessibility = defaultArg getterAccessibility AccessControl.Unknown
            let setterAccessibility = defaultArg setterAccessibility AccessControl.Unknown
            let isTupled = defaultArg isTupled false
            let parameters = parameters |> Seq.map Ast.LongIdent

            Ast.AbstractMember(
                identifier,
                parameters,
                returnType,
                isTupled,
                hasGetter,
                hasSetter,
                getterAccessibility,
                setterAccessibility
            )

        /// <summary>Creates an abstract member with parameters.</summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="parameters">The parameters of the member.</param>
        /// <param name="returnType">The return type of the member.</param>
        /// <param name="isTupled">Whether the parameters are tupled.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
        /// <param name="getterAccessibility">The accessibility of the getter.</param>
        /// <param name="setterAccessibility">The accessibility of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("ICircle") {
        ///             AbstractMember("Add", [ Int(); Int() ], "int")
        ///         }
        ///     }
        /// }
        /// </code>
        static member AbstractMember
            (
                identifier: string,
                parameters: WidgetBuilder<Type> seq,
                returnType: string,
                ?isTupled: bool,
                ?hasGetter: bool,
                ?hasSetter: bool,
                ?getterAccessibility: AccessControl,
                ?setterAccessibility: AccessControl
            ) =
            let isTupled = defaultArg isTupled false
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let getterAccessibility = defaultArg getterAccessibility AccessControl.Unknown
            let setterAccessibility = defaultArg setterAccessibility AccessControl.Unknown
            let returnType = Ast.LongIdent(returnType)

            Ast.AbstractMember(
                identifier,
                parameters,
                returnType,
                isTupled,
                hasGetter,
                hasSetter,
                getterAccessibility,
                setterAccessibility
            )

        /// <summary>Creates an abstract member with parameters.</summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="parameters">The parameters of the member.</param>
        /// <param name="returnType">The return type of the member.</param>
        /// <param name="isTupled">Whether the parameters are tupled.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
        /// <param name="getterAccessibility">The accessibility of the getter.</param>
        /// <param name="setterAccessibility">The accessibility of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("ICircle") {
        ///             AbstractMember("Add", [ "int"; "int" ], "int")
        ///         }
        ///     }
        /// }
        /// </code>
        static member AbstractMember
            (
                identifier: string,
                parameters: string seq,
                returnType: string,
                ?isTupled: bool,
                ?hasGetter: bool,
                ?hasSetter: bool,
                ?getterAccessibility: AccessControl,
                ?setterAccessibility: AccessControl
            ) =
            let parameters = parameters |> Seq.map Ast.LongIdent
            let isTupled = defaultArg isTupled false
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let getterAccessibility = defaultArg getterAccessibility AccessControl.Unknown
            let setterAccessibility = defaultArg setterAccessibility AccessControl.Unknown
            let returnType = Ast.LongIdent(returnType)

            Ast.AbstractMember(
                identifier,
                parameters,
                returnType,
                isTupled,
                hasGetter,
                hasSetter,
                getterAccessibility,
                setterAccessibility
            )

        /// <summary>Creates an abstract member with parameters.</summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="parameters">The parameters of the member.</param>
        /// <param name="returnType">The return type of the member.</param>
        /// <param name="isTupled">Whether the parameters are tupled.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
        /// <param name="getterAccessibility">The accessibility of the getter.</param>
        /// <param name="setterAccessibility">The accessibility of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("ICircle") {
        ///              AbstractMember("Add", [ ("a", Int()); ("b", Int()) ], Int())
        ///         }
        ///     }
        /// }
        /// </code>
        static member AbstractMember
            (
                identifier: string,
                parameters: (string * WidgetBuilder<Type>) seq,
                returnType: WidgetBuilder<Type>,
                ?isTupled: bool,
                ?hasGetter: bool,
                ?hasSetter: bool,
                ?getterAccessibility: AccessControl,
                ?setterAccessibility: AccessControl
            ) =
            let isTupled = defaultArg isTupled false
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let getterAccessibility = defaultArg getterAccessibility AccessControl.Unknown
            let setterAccessibility = defaultArg setterAccessibility AccessControl.Unknown

            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractSlot.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        AbstractSlot.Identifier.WithValue(identifier),
                        AbstractSlot.HasGetterSetter.WithValue(
                            (hasGetter, getterAccessibility),
                            (hasSetter, setterAccessibility)
                        ),
                        AbstractSlot.Parameters.WithValue(Named(parameters, isTupled))
                    ),
                    [| AbstractSlot.ReturnType.WithValue(returnType.Compile()) |],
                    Array.empty
                )
            )

        /// <summary>Creates an abstract member with parameters.</summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="parameters">The parameters of the member.</param>
        /// <param name="returnType">The return type of the member.</param>
        /// <param name="isTupled">Whether the parameters are tupled.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
        /// <param name="getterAccessibility">The accessibility of the getter.</param>
        /// <param name="setterAccessibility">The accessibility of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("ICircle") {
        ///             AbstractMember("Add", [ ("a", "int"); ("b", "int") ], Int())
        ///         }
        ///     }
        /// }
        /// </code>
        static member AbstractMember
            (
                identifier: string,
                parameters: (string * string) seq,
                returnType: WidgetBuilder<Type>,
                ?isTupled: bool,
                ?hasGetter: bool,
                ?hasSetter: bool,
                ?getterAccessibility: AccessControl,
                ?setterAccessibility: AccessControl
            ) =
            let isTupled = defaultArg isTupled false
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let getterAccessibility = defaultArg getterAccessibility AccessControl.Unknown
            let setterAccessibility = defaultArg setterAccessibility AccessControl.Unknown

            let parameters =
                parameters |> Seq.map(fun (name, tp) -> Ast.LongIdent(tp) |> fun tp -> name, tp)

            Ast.AbstractMember(
                identifier,
                parameters,
                returnType,
                isTupled,
                hasGetter,
                hasSetter,
                getterAccessibility,
                setterAccessibility
            )

        /// <summary>Creates an abstract member with parameters.</summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="parameters">The parameters of the member.</param>
        /// <param name="returnType">The return type of the member.</param>
        /// <param name="isTupled">Whether the parameters are tupled.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
        /// <param name="getterAccessibility">The accessibility of the getter.</param>
        /// <param name="setterAccessibility">The accessibility of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("ICircle") {
        ///             AbstractMember("Add", [ ("a", Int()); ("b", Int()) ], "int")
        ///         }
        ///     }
        /// }
        /// </code>
        static member AbstractMember
            (
                identifier: string,
                parameters: (string * WidgetBuilder<Type>) seq,
                returnType: string,
                ?isTupled: bool,
                ?hasGetter: bool,
                ?hasSetter: bool,
                ?getterAccessibility: AccessControl,
                ?setterAccessibility: AccessControl
            ) =
            let isTupled = defaultArg isTupled false
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let getterAccessibility = defaultArg getterAccessibility AccessControl.Unknown
            let setterAccessibility = defaultArg setterAccessibility AccessControl.Unknown
            let returnType = Ast.LongIdent(returnType)

            Ast.AbstractMember(
                identifier,
                parameters,
                returnType,
                isTupled,
                hasGetter,
                hasSetter,
                getterAccessibility,
                setterAccessibility
            )

        /// <summary>Creates an abstract member with parameters.</summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="parameters">The parameters of the member.</param>
        /// <param name="returnType">The return type of the member.</param>
        /// <param name="isTupled">Whether the parameters are tupled.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
        /// <param name="getterAccessibility">The accessibility of the getter.</param>
        /// <param name="setterAccessibility">The accessibility of the setter.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("ICircle") {
        ///             AbstractMember("Add", [ ("a", "int"); ("b", "int") ], "int")
        ///         }
        ///     }
        /// }
        /// </code>
        static member AbstractMember
            (
                identifier: string,
                parameters: (string * string) seq,
                returnType: string,
                ?isTupled: bool,
                ?hasGetter: bool,
                ?hasSetter: bool,
                ?getterAccessibility: AccessControl,
                ?setterAccessibility: AccessControl
            ) =
            let isTupled = defaultArg isTupled false
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let getterAccessibility = defaultArg getterAccessibility AccessControl.Unknown
            let setterAccessibility = defaultArg setterAccessibility AccessControl.Unknown

            let parameters =
                parameters |> Seq.map(fun (name, tp) -> Ast.LongIdent(tp) |> fun tp -> name, tp)

            let returnType = Ast.LongIdent(returnType)

            Ast.AbstractMember(
                identifier,
                parameters,
                returnType,
                isTupled,
                hasGetter,
                hasSetter,
                getterAccessibility,
                setterAccessibility
            )

type AbstractMemberModifiers =
    /// <summary>Sets the XmlDocs for the current member.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("ICircle") {
    ///             AbstractMember("Area", Float(), true)
    ///                 .xmlDocs(Summary("This is the area"))
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<MemberDefnAbstractSlotNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(AbstractSlot.XmlDocs.WithValue(xmlDocs.Compile()))

    /// <summary>Sets the XmlDocs for the current member.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="xmlDocs">The XmlDocs to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("ICircle") {
    ///             AbstractMember("Area", Float(), true)
    ///                 .xmlDocs([ "This is the area" ])
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<MemberDefnAbstractSlotNode>, xmlDocs: string seq) =
        AbstractMemberModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    /// <summary>Sets the attributes for the current member.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attributes">The attributes to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("ICircle") {
    ///             AbstractMember("Area", Float(), true)
    ///                 .attributes([ Attribute("Obsolete") ])
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<MemberDefnAbstractSlotNode>, attributes: WidgetBuilder<AttributeNode> seq)
        =
        this.AddScalar(AbstractSlot.MultipleAttributes.WithValue(attributes |> Seq.map Gen.mkOak))

    /// <summary>Sets the attributes for the current member.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="attribute">The attribute to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("ICircle") {
    ///             AbstractMember("Area", Float(), true)
    ///                 .attribute(Attribute("Obsolete"))
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline attribute
        (this: WidgetBuilder<MemberDefnAbstractSlotNode>, attribute: WidgetBuilder<AttributeNode>)
        =
        AbstractMemberModifiers.attributes(this, [ attribute ])

    /// <summary>Sets the type parameters for the current member.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="typeParams">The type parameters to set.</param>
    /// <code language="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         TypeDefn("ICircle") {
    ///             AbstractMember("Area", Float(), true)
    ///                 .typeParams(PostfixList([ "'a" ]))
    ///         }
    ///     }
    /// }
    /// </code>
    [<Extension>]
    static member inline typeParams
        (this: WidgetBuilder<MemberDefnAbstractSlotNode>, typeParams: WidgetBuilder<TyparDecls>)
        =
        this.AddWidget(AbstractSlot.TypeParams.WithValue(typeParams.Compile()))
