namespace Fabulous.AST

open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open Microsoft.FSharp.Collections

[<RequireQualifiedAccess>]
type AbstractSlotType =
    | Getter
    | Setter
    | GetSet
    | Method
    | Unknown

[<RequireQualifiedAccess>]
type MethodParams =
    | UnNamed of Type list
    | Named of (string * Type) list

module AbstractMember =
    let Identifier = Attributes.defineScalar "Identifier"

    let ReturnType = Attributes.defineScalar<Type option> "Type"

    let WithGetSet = Attributes.defineScalar<AbstractSlotType option> "WithGetSet"

    let Parameters =
        Attributes.defineScalar<struct (bool * MethodParams * Type)> "Parameters"

    let WidgetKey =
        Widgets.register "AbstractMember" (fun widget ->
            let identifier = Helpers.getScalarValue widget Identifier
            let returnType = Helpers.getScalarValue widget ReturnType
            let withGetSet = Helpers.tryGetScalarValue widget WithGetSet
            let parameters = Helpers.tryGetScalarValue widget Parameters

            let parameters =
                match parameters, returnType with
                | ValueSome(isTupled, parameters, returnType), None ->
                    let parameters =
                        match parameters with
                        | MethodParams.UnNamed types ->
                            types
                            |> List.mapi(fun index value ->
                                let separator =
                                    if index < types.Length - 1 && isTupled then
                                        SingleTextNode.star
                                    else
                                        SingleTextNode.rightArrow

                                (value, separator))
                        | MethodParams.Named tuples ->
                            tuples
                            |> List.mapi(fun index (name, value) ->
                                let separator =
                                    if index < tuples.Length - 1 && isTupled then
                                        SingleTextNode.star
                                    else
                                        SingleTextNode.rightArrow

                                let value =
                                    if System.String.IsNullOrEmpty(name) then
                                        value
                                    else
                                        Type.SignatureParameter(
                                            TypeSignatureParameterNode.Create(name, CommonType.Int32)
                                        )

                                (value, separator))


                    Type.Funs(TypeFunsNode(parameters, returnType, Range.Zero))
                | ValueNone, Some returnType -> Type.Funs(TypeFunsNode([], returnType, Range.Zero))
                | _ -> failwithf $"Invalid parameters and return type combination for abstract member %s{identifier}"

            let withGetSetText =
                match withGetSet with
                | ValueNone -> None
                | ValueSome slot ->
                    match slot with
                    | None -> None
                    | Some slot ->
                        match slot with
                        | AbstractSlotType.Method
                        | AbstractSlotType.Unknown -> None
                        | AbstractSlotType.Getter -> Some(MultipleTextsNode.Create([ "with"; "get" ]))
                        | AbstractSlotType.Setter -> Some(MultipleTextsNode.Create([ "with"; "set" ]))
                        | AbstractSlotType.GetSet -> Some(MultipleTextsNode.Create([ "with"; "get,"; "set" ]))

            MemberDefnAbstractSlotNode(
                None,
                None,
                MultipleTextsNode.Create([ "abstract"; "member" ]),
                SingleTextNode.Create(identifier),
                None,
                parameters,
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
                AbstractMember.ReturnType.WithValue(Some returnType),
                AbstractMember.WithGetSet.WithValue(Some AbstractSlotType.Unknown)
            )

        static member AbstractGetMember(identifier: string, returnType: Type) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AbstractMember.Identifier.WithValue(identifier),
                AbstractMember.ReturnType.WithValue(Some returnType),
                AbstractMember.WithGetSet.WithValue(Some AbstractSlotType.Getter)
            )

        static member AbstractSetMember(identifier: string, returnType: Type) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AbstractMember.Identifier.WithValue(identifier),
                AbstractMember.ReturnType.WithValue(Some returnType),
                AbstractMember.WithGetSet.WithValue(Some AbstractSlotType.Setter)
            )

        static member AbstractGetSetMember(identifier: string, returnType: Type) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AbstractMember.Identifier.WithValue(identifier),
                AbstractMember.ReturnType.WithValue(Some returnType),
                AbstractMember.WithGetSet.WithValue(Some AbstractSlotType.GetSet)
            )

        static member AbstractCurriedMethodMember(identifier: string, parameters: Type list, returnType: Type) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AbstractMember.Identifier.WithValue(identifier),
                AbstractMember.ReturnType.WithValue(None),
                AbstractMember.Parameters.WithValue(struct (false, MethodParams.UnNamed(parameters), returnType))
            )

        static member AbstractCurriedMethodMember
            (
                identifier: string,
                parameters: (string * Type) list,
                returnType: Type
            ) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AbstractMember.Identifier.WithValue(identifier),
                AbstractMember.ReturnType.WithValue(None),
                AbstractMember.Parameters.WithValue(struct (false, MethodParams.Named(parameters), returnType))
            )

        static member AbstractTupledMethodMember(identifier: string, parameters: Type list, returnType: Type) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AbstractMember.Identifier.WithValue(identifier),
                AbstractMember.ReturnType.WithValue(None),
                AbstractMember.Parameters.WithValue(struct (true, MethodParams.UnNamed(parameters), returnType))
            )

        static member AbstractTupledMethodMember
            (
                identifier: string,
                parameters: (string * Type) list,
                returnType: Type
            ) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AbstractMember.Identifier.WithValue(identifier),
                AbstractMember.ReturnType.WithValue(None),
                AbstractMember.Parameters.WithValue(struct (true, MethodParams.Named(parameters), returnType))
            )
