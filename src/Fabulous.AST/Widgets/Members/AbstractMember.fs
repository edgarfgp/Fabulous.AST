namespace Fabulous.AST

open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open Microsoft.FSharp.Collections

type AbstractSlotType =
    | WithGet
    | WithSet
    | WithGetSet
    | Method
    | Unknown

module AbstractMember =
    let Identifier = Attributes.defineScalar "Identifier"

    let ReturnType = Attributes.defineScalar<Type option> "Type"

    let WithGetSet = Attributes.defineScalar<AbstractSlotType option> "WithGetSet"

    let Parameters = Attributes.defineScalar<struct (Type list * Type)> "Parameters"

    let WidgetKey =
        Widgets.register "AbstractMember" (fun widget ->
            let identifier = Helpers.getScalarValue widget Identifier
            let returnType = Helpers.getScalarValue widget ReturnType
            let withGetSet = Helpers.tryGetScalarValue widget WithGetSet
            let parameters = Helpers.tryGetScalarValue widget Parameters

            let parameters =
                match parameters, returnType with
                | ValueSome(parameters, returnType), None ->
                    let parameters =
                        parameters |> List.map(fun value -> (value, SingleTextNode.rightArrow))

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
                        | AbstractSlotType.WithGet -> Some(MultipleTextsNode.Create([ "with"; "get" ]))
                        | AbstractSlotType.WithSet -> Some(MultipleTextsNode.Create([ "with"; "set" ]))
                        | AbstractSlotType.WithGetSet -> Some(MultipleTextsNode.Create([ "with"; "get,"; "set" ]))

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
                AbstractMember.WithGetSet.WithValue(Some AbstractSlotType.WithGet)
            )

        static member AbstractSetMember(identifier: string, returnType: Type) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AbstractMember.Identifier.WithValue(identifier),
                AbstractMember.ReturnType.WithValue(Some returnType),
                AbstractMember.WithGetSet.WithValue(Some AbstractSlotType.WithSet)
            )

        static member AbstractGetSetMember(identifier: string, returnType: Type) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AbstractMember.Identifier.WithValue(identifier),
                AbstractMember.ReturnType.WithValue(Some returnType),
                AbstractMember.WithGetSet.WithValue(Some AbstractSlotType.WithGetSet)
            )

        static member AbstractMethodMember(identifier: string, parameters: Type list, returnType: Type) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AbstractMember.Identifier.WithValue(identifier),
                AbstractMember.ReturnType.WithValue(None),
                AbstractMember.Parameters.WithValue(struct (parameters, returnType))
            )
