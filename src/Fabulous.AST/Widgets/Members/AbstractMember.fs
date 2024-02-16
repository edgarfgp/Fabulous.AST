namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open Microsoft.FSharp.Collections

type MethodParamsType =
    | UnNamed of parameters: (Type list) * isTupled: bool
    | Named of types: ((string option * Type) list) * isTupled: bool

module AbstractMember =
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"
    let Identifier = Attributes.defineScalar<string> "Identifier"
    let ReturnType = Attributes.defineScalar<Type> "Type"
    let Parameters = Attributes.defineScalar<MethodParamsType> "Parameters"
    let HasGetterSetter = Attributes.defineScalar<bool * bool> "HasGetterSetter"
    let MultipleAttributes = Attributes.defineWidget "MultipleAttributes"

    let WidgetKey =
        Widgets.register "AbstractMember" (fun widget ->
            let identifier = Helpers.getScalarValue widget Identifier
            let returnType = Helpers.getScalarValue widget ReturnType
            let parameters = Helpers.tryGetScalarValue widget Parameters
            let hasGetterSetter = Helpers.tryGetScalarValue widget HasGetterSetter

            let attributes =
                Helpers.tryGetNodeFromWidget<AttributeListNode> widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | ValueSome values -> Some(MultipleAttributeListNode([ values ], Range.Zero))
                | ValueNone -> None

            let lines = Helpers.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            let typeFun =
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

                            (value, separator))

                    parameters, returnType
                | ValueSome(Named((parameters), isTupled)) ->
                    let parameters =
                        parameters
                        |> List.mapi(fun index (name, value) ->
                            let separator =
                                if index < parameters.Length - 1 && isTupled then
                                    SingleTextNode.star
                                else
                                    SingleTextNode.rightArrow

                            match name with
                            | Some name ->
                                if System.String.IsNullOrEmpty(name) then
                                    failwith "Named parameters must have a name"

                                let value = Type.SignatureParameter(TypeSignatureParameterNode.Create(name, value))
                                (value, separator)

                            | None -> (value, separator))

                    parameters, returnType

            let withGetSetText =
                match hasGetterSetter with
                | ValueSome(true, true) -> Some(MultipleTextsNode.Create([ "with"; "get,"; "set" ]))
                | ValueSome(true, false) -> Some(MultipleTextsNode.Create([ "with"; "get" ]))
                | ValueSome(false, true) -> Some(MultipleTextsNode.Create([ "with"; "set" ]))
                | ValueSome(false, false)
                | ValueNone -> None

            let parameters, returnTYpe = typeFun

            MemberDefnAbstractSlotNode(
                xmlDocs,
                multipleAttributes,
                MultipleTextsNode.Create([ "abstract"; "member" ]),
                SingleTextNode(identifier, Range.Zero),
                None,
                Type.Funs(TypeFunsNode(parameters, returnTYpe, Range.Zero)),
                withGetSetText,
                Range.Zero
            ))

[<AutoOpen>]
module AbstractMemberBuilders =
    type Ast with

        static member AbstractPropertyMember(identifier: string, returnType: Type) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AbstractMember.Identifier.WithValue(identifier),
                AbstractMember.HasGetterSetter.WithValue(false, false),
                AbstractMember.ReturnType.WithValue(returnType)
            )

        static member AbstractPropertyMember(identifier: string, returnType: string) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AbstractMember.Identifier.WithValue(identifier),
                AbstractMember.HasGetterSetter.WithValue(false, false),
                AbstractMember.ReturnType.WithValue(CommonType.mkLongIdent(returnType))
            )

        static member AbstractGetMember(identifier: string, returnType: Type) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AbstractMember.Identifier.WithValue(identifier),
                AbstractMember.HasGetterSetter.WithValue(true, false),
                AbstractMember.ReturnType.WithValue(returnType)
            )

        static member AbstractGetMember(identifier: string, returnType: string) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AbstractMember.Identifier.WithValue(identifier),
                AbstractMember.HasGetterSetter.WithValue(true, false),
                AbstractMember.ReturnType.WithValue(CommonType.mkLongIdent(returnType))
            )

        static member AbstractSetMember(identifier: string, returnType: Type) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AbstractMember.Identifier.WithValue(identifier),
                AbstractMember.HasGetterSetter.WithValue(false, true),
                AbstractMember.ReturnType.WithValue(returnType)
            )

        static member AbstractSetMember(identifier: string, returnType: string) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AbstractMember.Identifier.WithValue(identifier),
                AbstractMember.HasGetterSetter.WithValue(false, true),
                AbstractMember.ReturnType.WithValue(CommonType.mkLongIdent(returnType))
            )

        static member AbstractGetSetMember(identifier: string, returnType: Type) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AbstractMember.Identifier.WithValue(identifier),
                AbstractMember.HasGetterSetter.WithValue(true, true),
                AbstractMember.ReturnType.WithValue(returnType)
            )

        static member AbstractGetSetMember(identifier: string, returnType: string) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AbstractMember.Identifier.WithValue(identifier),
                AbstractMember.HasGetterSetter.WithValue(true, true),
                AbstractMember.ReturnType.WithValue(CommonType.mkLongIdent(returnType))
            )

        static member AbstractTupledMethodMember(identifier: string, parameters: Type list, returnType: Type) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AbstractMember.Identifier.WithValue(identifier),
                AbstractMember.ReturnType.WithValue(returnType),
                AbstractMember.Parameters.WithValue(UnNamed(parameters, true))
            )

        static member AbstractTupledMethodMember(identifier: string, parameters: string list, returnType: string) =
            let parameters = parameters |> List.map CommonType.mkLongIdent

            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AbstractMember.Identifier.WithValue(identifier),
                AbstractMember.ReturnType.WithValue(CommonType.mkLongIdent(returnType)),
                AbstractMember.Parameters.WithValue(UnNamed(parameters, true))
            )

        static member AbstractTupledMethodMember
            (
                identifier: string,
                parameters: (string option * Type) list,
                returnType: Type
            ) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AbstractMember.Identifier.WithValue(identifier),
                AbstractMember.ReturnType.WithValue(returnType),
                AbstractMember.Parameters.WithValue(Named(parameters, true))
            )

        static member AbstractTupledMethodMember
            (
                identifier: string,
                parameters: (string option * string) list,
                returnType: string
            ) =
            let parameters =
                parameters |> List.map(fun (name, value) -> name, CommonType.mkLongIdent value)

            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AbstractMember.Identifier.WithValue(identifier),
                AbstractMember.ReturnType.WithValue(CommonType.mkLongIdent(returnType)),
                AbstractMember.Parameters.WithValue(Named(parameters, true))
            )


        static member AbstractCurriedMethodMember(identifier: string, parameters: Type list, returnType: Type) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AbstractMember.Identifier.WithValue(identifier),
                AbstractMember.ReturnType.WithValue(returnType),
                AbstractMember.Parameters.WithValue(UnNamed(parameters, false))
            )

        static member AbstractCurriedMethodMember(identifier: string, parameters: string list, returnType: string) =
            let parameters = parameters |> List.map CommonType.mkLongIdent

            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AbstractMember.Identifier.WithValue(identifier),
                AbstractMember.ReturnType.WithValue(CommonType.mkLongIdent(returnType)),
                AbstractMember.Parameters.WithValue(UnNamed(parameters, false))
            )

        static member AbstractCurriedMethodMember
            (
                identifier: string,
                parameters: (string option * Type) list,
                returnType: Type
            ) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AbstractMember.Identifier.WithValue(identifier),
                AbstractMember.ReturnType.WithValue(returnType),
                AbstractMember.Parameters.WithValue(Named(parameters, false))
            )

        static member AbstractCurriedMethodMember
            (
                identifier: string,
                parameters: (string option * string) list,
                returnType: string
            ) =
            let parameters =
                parameters |> List.map(fun (name, value) -> name, CommonType.mkLongIdent value)

            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AbstractMember.Identifier.WithValue(identifier),
                AbstractMember.ReturnType.WithValue(CommonType.mkLongIdent(returnType)),
                AbstractMember.Parameters.WithValue(Named(parameters, false))
            )

[<Extension>]
type AbstractMemberModifiers =
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<MemberDefnAbstractSlotNode>, comments: string list) =
        this.AddScalar(AbstractMember.XmlDocs.WithValue(comments))

    [<Extension>]
    static member inline attributes
        (
            this: WidgetBuilder<MemberDefnAbstractSlotNode>,
            attributes: WidgetBuilder<AttributeListNode>
        ) =
        this.AddWidget(AbstractMember.MultipleAttributes.WithValue(attributes.Compile()))

    [<Extension>]
    static member inline attributes
        (
            this: WidgetBuilder<MemberDefnAbstractSlotNode>,
            attribute: WidgetBuilder<AttributeNode>
        ) =
        this.AddWidget(AbstractMember.MultipleAttributes.WithValue((Ast.AttributeNodes() { attribute }).Compile()))
