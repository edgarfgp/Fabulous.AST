namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module AbstractSlot =
    let Identifier = Attributes.defineScalar<string> "Identifier"
    let ReturnType = Attributes.defineWidget "ReturnType"
    let Parameters = Attributes.defineScalar<MethodParamsType> "Parameters"

    let HasGetterSetter = Attributes.defineScalar<bool * bool> "HasGetterSetter"

    let WidgetKey =
        Widgets.register "AbstractMember" (fun widget ->
            let identifier = Widgets.getScalarValue widget Identifier
            let returnType = Widgets.getNodeFromWidget widget ReturnType
            let parameters = Widgets.tryGetScalarValue widget Parameters
            let hasGetter, hasSetter = Widgets.getScalarValue widget HasGetterSetter

            let attributes =
                Widgets.tryGetScalarValue widget MemberDefn.MultipleAttributes
                |> ValueOption.map MultipleAttributeListNode.Create
                |> ValueOption.toOption

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget MemberDefn.XmlDocs |> ValueOption.toOption

            let separatorAt index lastIndex isTupled =
                if isTupled && index < lastIndex then
                    SingleTextNode.star
                else
                    SingleTextNode.rightArrow

            let parameterList =
                match parameters with
                | ValueNone -> []
                | ValueSome(UnNamed(parameters, isTupled)) ->
                    let parameters = List.ofSeq parameters
                    let lastIndex = List.length parameters - 1

                    parameters
                    |> List.mapi(fun index value -> Gen.mkOak value, separatorAt index lastIndex isTupled)
                | ValueSome(Named(parameters, isTupled)) ->
                    let parameters = List.ofSeq parameters
                    let lastIndex = List.length parameters - 1

                    parameters
                    |> List.mapi(fun index (name, value) ->
                        let signatureParam =
                            Type.SignatureParameter(
                                TypeSignatureParameterNode(
                                    None,
                                    Some(SingleTextNode.Create(name)),
                                    Gen.mkOak(value),
                                    Range.Zero
                                )
                            )

                        signatureParam, separatorAt index lastIndex isTupled)

            let returnType =
                match parameterList with
                | [] -> returnType
                | parameters -> Type.Funs(TypeFunsNode(parameters, returnType, Range.Zero))

            // Abstract slots always have the visibility of the enclosing type;
            // F# forbids accessibility modifiers on their accessors, so drop them.
            let withGetSetText =
                MultipleTextsNode.CreateGetSet((hasGetter, AccessControl.Unknown), (hasSetter, AccessControl.Unknown))

            let isStatic =
                Widgets.tryGetScalarValue widget BindingNode.IsStatic
                |> ValueOption.defaultValue false

            let leadingKeywords =
                MultipleTextsNode.Create(
                    if isStatic then
                        [ SingleTextNode.``static``; SingleTextNode.``abstract`` ]
                    else
                        [ SingleTextNode.``abstract`` ]
                )

            let typeParams =
                Widgets.tryGetNodeFromWidget widget MemberDefn.TypeParams
                |> ValueOption.toOption

            let node =
                MemberDefnAbstractSlotNode(
                    xmlDocs,
                    attributes,
                    leadingKeywords,
                    SingleTextNode.Create(identifier),
                    typeParams,
                    returnType,
                    withGetSetText,
                    Range.Zero
                )

            MemberDefn.AbstractSlot(node))

[<AutoOpen>]
module AbstractMemberBuilders =
    type Ast with
        /// <summary>Creates an abstract member.</summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="returnType">The return type of the member.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
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
            (identifier: string, returnType: WidgetBuilder<Type>, ?hasGetter: bool, ?hasSetter: bool)
            =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false

            WidgetBuilder<MemberDefn>(
                AbstractSlot.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        AbstractSlot.Identifier.WithValue(identifier),
                        AbstractSlot.HasGetterSetter.WithValue(hasGetter, hasSetter)
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
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         TypeDefn("ICircle") {
        ///             AbstractMember("Area", "float", true)
        ///         }
        ///     }
        /// }
        /// </code>
        static member AbstractMember(identifier: string, returnType: string, ?hasGetter: bool, ?hasSetter: bool) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false

            Ast.AbstractMember(identifier, Ast.LongIdent(returnType), hasGetter, hasSetter)

        /// <summary>Creates an abstract member with parameters.</summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="parameters">The parameters of the member.</param>
        /// <param name="returnType">The return type of the member.</param>
        /// <param name="isTupled">Whether the parameters are tupled.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
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
                ?hasSetter: bool
            ) =
            let isTupled = defaultArg isTupled false
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false

            WidgetBuilder<MemberDefn>(
                AbstractSlot.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        AbstractSlot.Identifier.WithValue(identifier),
                        AbstractSlot.HasGetterSetter.WithValue(hasGetter, hasSetter),
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
                ?hasSetter: bool
            ) =
            let isTupled = defaultArg isTupled false
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let parameters = parameters |> Seq.map Ast.LongIdent

            Ast.AbstractMember(identifier, parameters, returnType, isTupled, hasGetter, hasSetter)

        /// <summary>Creates an abstract member with parameters.</summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="parameters">The parameters of the member.</param>
        /// <param name="returnType">The return type of the member.</param>
        /// <param name="isTupled">Whether the parameters are tupled.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
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
                ?hasSetter: bool
            ) =
            let isTupled = defaultArg isTupled false
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let returnType = Ast.LongIdent(returnType)

            Ast.AbstractMember(identifier, parameters, returnType, isTupled, hasGetter, hasSetter)

        /// <summary>Creates an abstract member with parameters.</summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="parameters">The parameters of the member.</param>
        /// <param name="returnType">The return type of the member.</param>
        /// <param name="isTupled">Whether the parameters are tupled.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
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
                ?hasSetter: bool
            ) =
            let isTupled = defaultArg isTupled false
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let parameters = parameters |> Seq.map Ast.LongIdent
            let returnType = Ast.LongIdent(returnType)

            Ast.AbstractMember(identifier, parameters, returnType, isTupled, hasGetter, hasSetter)

        /// <summary>Creates an abstract member with parameters.</summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="parameters">The parameters of the member.</param>
        /// <param name="returnType">The return type of the member.</param>
        /// <param name="isTupled">Whether the parameters are tupled.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
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
                ?hasSetter: bool
            ) =
            let isTupled = defaultArg isTupled false
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false

            let parameters = List.ofSeq parameters

            for (name, _) in parameters do
                if System.String.IsNullOrEmpty name then
                    invalidArg "parameters" "Named parameters must all have a non-empty name"

            WidgetBuilder<MemberDefn>(
                AbstractSlot.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        AbstractSlot.Identifier.WithValue(identifier),
                        AbstractSlot.HasGetterSetter.WithValue(hasGetter, hasSetter),
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
                ?hasSetter: bool
            ) =
            let isTupled = defaultArg isTupled false
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let parameters = parameters |> Seq.map(fun (name, tp) -> name, Ast.LongIdent(tp))

            Ast.AbstractMember(identifier, parameters, returnType, isTupled, hasGetter, hasSetter)

        /// <summary>Creates an abstract member with parameters.</summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="parameters">The parameters of the member.</param>
        /// <param name="returnType">The return type of the member.</param>
        /// <param name="isTupled">Whether the parameters are tupled.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
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
                ?hasSetter: bool
            ) =
            let isTupled = defaultArg isTupled false
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let returnType = Ast.LongIdent(returnType)

            Ast.AbstractMember(identifier, parameters, returnType, isTupled, hasGetter, hasSetter)

        /// <summary>Creates an abstract member with parameters.</summary>
        /// <param name="identifier">The identifier of the member.</param>
        /// <param name="parameters">The parameters of the member.</param>
        /// <param name="returnType">The return type of the member.</param>
        /// <param name="isTupled">Whether the parameters are tupled.</param>
        /// <param name="hasGetter">Whether the member has a getter.</param>
        /// <param name="hasSetter">Whether the member has a setter.</param>
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
                ?hasSetter: bool
            ) =
            let isTupled = defaultArg isTupled false
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            let parameters = parameters |> Seq.map(fun (name, tp) -> name, Ast.LongIdent(tp))
            let returnType = Ast.LongIdent(returnType)

            Ast.AbstractMember(identifier, parameters, returnType, isTupled, hasGetter, hasSetter)
