namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module AbstractSlot =
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"
    let Identifier = Attributes.defineScalar<string> "Identifier"
    let ReturnType = Attributes.defineWidget "Type"
    let Parameters = Attributes.defineScalar<MethodParamsType> "Parameters"
    let HasGetterSetter = Attributes.defineScalar<bool * bool> "HasGetterSetter"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let TypeParams = Attributes.defineWidget "TypeParams"

    let WidgetKey =
        Widgets.register "AbstractMember" (fun widget ->
            let identifier = Widgets.getScalarValue widget Identifier
            let returnType = Widgets.getNodeFromWidget widget ReturnType
            let parameters = Widgets.tryGetScalarValue widget Parameters
            let hasGetterSetter = Widgets.tryGetScalarValue widget HasGetterSetter

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let lines = Widgets.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            let returnType =
                match parameters with
                | ValueNone -> [], returnType
                | ValueSome(UnNamed(parameters, isTupled)) ->
                    let parameters =
                        parameters
                        |> List.mapi(fun index value ->
                            let separator =
                                if index < parameters.Length - 1 && isTupled then
                                    SingleTextNode.star
                                else
                                    SingleTextNode.rightArrow

                            (Gen.mkOak value, separator))

                    parameters, returnType
                | ValueSome(Named(parameters, isTupled)) ->
                    let parameters =
                        parameters
                        |> List.mapi(fun index (name, value) ->
                            let separator =
                                if index < parameters.Length - 1 && isTupled then
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

                    parameters, returnType

            let withGetSetText =
                match hasGetterSetter with
                | ValueSome(true, true) ->
                    Some(
                        MultipleTextsNode.Create(
                            [ SingleTextNode.``with``; SingleTextNode.Create("get,"); SingleTextNode.set ]
                        )
                    )
                | ValueSome(true, false) ->
                    Some(MultipleTextsNode.Create([ SingleTextNode.``with``; SingleTextNode.get ]))
                | ValueSome(false, true) ->
                    Some(MultipleTextsNode.Create([ SingleTextNode.``with``; SingleTextNode.set ]))
                | ValueSome(false, false)
                | ValueNone -> None

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
        static member AbstractMember
            (identifier: string, returnType: WidgetBuilder<Type>, ?hasGetter: bool, ?hasSetter: bool)
            =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false

            WidgetBuilder<MemberDefnAbstractSlotNode>(
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

        static member AbstractMember(identifier: string, returnType: string, ?hasGetter: bool, ?hasSetter: bool) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            Ast.AbstractMember(identifier, Ast.LongIdent(returnType), hasGetter, hasSetter)

        static member AbstractMember
            (identifier: string, parameters: WidgetBuilder<Type> list, returnType: WidgetBuilder<Type>, ?isTupled: bool) =
            let isTupled = defaultArg isTupled false

            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractSlot.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        AbstractSlot.Identifier.WithValue(identifier),
                        AbstractSlot.Parameters.WithValue(UnNamed(parameters, isTupled))
                    ),
                    [| AbstractSlot.ReturnType.WithValue(returnType.Compile()) |],
                    Array.empty
                )
            )

        static member AbstractMember
            (identifier: string, parameters: string list, returnType: WidgetBuilder<Type>, ?isTupled: bool)
            =
            let isTupled = defaultArg isTupled false
            let parameters = parameters |> List.map Ast.LongIdent

            Ast.AbstractMember(identifier, parameters, returnType, isTupled)

        static member AbstractMember
            (identifier: string, parameters: WidgetBuilder<Type> list, returnType: string, ?isTupled: bool)
            =
            let isTupled = defaultArg isTupled false
            let returnType = Ast.LongIdent(returnType)
            Ast.AbstractMember(identifier, parameters, returnType, isTupled)

        static member AbstractMember(identifier: string, parameters: string list, returnType: string, ?isTupled: bool) =
            let parameters = parameters |> List.map Ast.LongIdent
            let isTupled = defaultArg isTupled false
            let returnType = Ast.LongIdent(returnType)

            Ast.AbstractMember(identifier, parameters, returnType, isTupled)

        static member AbstractMember
            (
                identifier: string,
                parameters: (string * WidgetBuilder<Type>) list,
                returnType: WidgetBuilder<Type>,
                ?isTupled: bool
            ) =
            let isTupled = defaultArg isTupled false

            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractSlot.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        AbstractSlot.Identifier.WithValue(identifier),
                        AbstractSlot.Parameters.WithValue(Named(parameters, isTupled))
                    ),
                    [| AbstractSlot.ReturnType.WithValue(returnType.Compile()) |],
                    Array.empty
                )
            )

        static member AbstractMember
            (identifier: string, parameters: (string * string) list, returnType: WidgetBuilder<Type>, ?isTupled: bool) =
            let isTupled = defaultArg isTupled false

            let parameters =
                parameters
                |> List.map(fun (name, tp) -> Ast.LongIdent(tp) |> fun tp -> name, tp)

            Ast.AbstractMember(identifier, parameters, returnType, isTupled)

        static member AbstractMember
            (identifier: string, parameters: (string * WidgetBuilder<Type>) list, returnType: string, ?isTupled: bool) =
            let isTupled = defaultArg isTupled false
            let returnType = Ast.LongIdent(returnType)

            Ast.AbstractMember(identifier, parameters, returnType, isTupled)

        static member AbstractMember
            (identifier: string, parameters: (string * string) list, returnType: string, ?isTupled: bool)
            =
            let isTupled = defaultArg isTupled false

            let parameters =
                parameters
                |> List.map(fun (name, tp) -> Ast.LongIdent(tp) |> fun tp -> name, tp)

            let returnType = Ast.LongIdent(returnType)

            Ast.AbstractMember(identifier, parameters, returnType, isTupled)

type AbstractMemberModifiers =
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<MemberDefnAbstractSlotNode>, comments: string list) =
        this.AddScalar(AbstractSlot.XmlDocs.WithValue(comments))

    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<MemberDefnAbstractSlotNode>, values: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            AbstractSlot.MultipleAttributes.WithValue(
                [ for vals in values do
                      Gen.mkOak vals ]
            )
        )

    [<Extension>]
    static member inline attribute
        (this: WidgetBuilder<MemberDefnAbstractSlotNode>, attribute: WidgetBuilder<AttributeNode>)
        =
        AbstractMemberModifiers.attributes(this, [ attribute ])

    [<Extension>]
    static member inline typeParams
        (this: WidgetBuilder<MemberDefnAbstractSlotNode>, typeParams: WidgetBuilder<TyparDecls>)
        =
        this.AddWidget(AbstractSlot.TypeParams.WithValue(typeParams.Compile()))
