namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open Microsoft.FSharp.Collections

type MethodParamsType =
    | UnNamed of parameters: (WidgetBuilder<Type> list) * isTupled: bool
    | Named of types: (string option * WidgetBuilder<Type>) list * isTupled: bool

module AbstractMember =
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"
    let Identifier = Attributes.defineScalar<string> "Identifier"
    let ReturnType = Attributes.defineWidget "Type"
    let Parameters = Attributes.defineScalar<MethodParamsType> "Parameters"
    let HasGetterSetter = Attributes.defineScalar<bool * bool> "HasGetterSetter"
    let MultipleAttributes = Attributes.defineWidgetCollection "MultipleAttributes"

    let WidgetKey =
        Widgets.register "AbstractMember" (fun widget ->
            let identifier = Widgets.getScalarValue widget Identifier
            let returnType = Widgets.getNodeFromWidget widget ReturnType
            let parameters = Widgets.tryGetScalarValue widget Parameters
            let hasGetterSetter = Widgets.tryGetScalarValue widget HasGetterSetter

            let attributes =
                Widgets.tryGetNodesFromWidgetCollection<AttributeNode> widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | Some values ->
                    Some(
                        MultipleAttributeListNode(
                            [ AttributeListNode(
                                  SingleTextNode.leftAttribute,
                                  values,
                                  SingleTextNode.rightAttribute,
                                  Range.Zero
                              ) ],
                            Range.Zero
                        )
                    )
                | None -> None

            let lines = Widgets.tryGetScalarValue widget XmlDocs

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

                            (Gen.mkOak value, separator))

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

                                let value =
                                    Type.SignatureParameter(
                                        TypeSignatureParameterNode(
                                            None,
                                            Some(SingleTextNode.Create(name)),
                                            Gen.mkOak(value),
                                            Range.Zero
                                        )
                                    )

                                (value, separator)

                            | None -> (Gen.mkOak(value), separator))

                    parameters, returnType

            let withGetSetText =
                match hasGetterSetter with
                | ValueSome(true, true) -> Some(MultipleTextsNode.Create([ "with"; "get,"; "set" ]))
                | ValueSome(true, false) -> Some(MultipleTextsNode.Create([ "with"; "get" ]))
                | ValueSome(false, true) -> Some(MultipleTextsNode.Create([ "with"; "set" ]))
                | ValueSome(false, false)
                | ValueNone -> None

            let parameters, returnType = typeFun

            MemberDefnAbstractSlotNode(
                xmlDocs,
                multipleAttributes,
                MultipleTextsNode.Create([ "abstract"; "member" ]),
                SingleTextNode(identifier, Range.Zero),
                None,
                Type.Funs(TypeFunsNode(parameters, returnType, Range.Zero)),
                withGetSetText,
                Range.Zero
            ))

[<AutoOpen>]
module AbstractMemberBuilders =
    type Ast with

        static member AbstractProperty(identifier: string, returnType: WidgetBuilder<Type>) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        AbstractMember.Identifier.WithValue(identifier),
                        AbstractMember.HasGetterSetter.WithValue(false, false)
                    ),
                    ValueSome [| AbstractMember.ReturnType.WithValue(returnType.Compile()) |],
                    ValueNone
                )
            )

        static member AbstractProperty(identifier: string, returnType: string) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        AbstractMember.Identifier.WithValue(identifier),
                        AbstractMember.HasGetterSetter.WithValue(false, false)
                    ),
                    ValueSome [| AbstractMember.ReturnType.WithValue(Ast.LongIdent(returnType).Compile()) |],
                    ValueNone
                )
            )

        static member AbstractGet(identifier: string, returnType: WidgetBuilder<Type>) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        AbstractMember.Identifier.WithValue(identifier),
                        AbstractMember.HasGetterSetter.WithValue(true, false)
                    ),
                    ValueSome [| AbstractMember.ReturnType.WithValue(returnType.Compile()) |],
                    ValueNone
                )
            )

        static member AbstractGet(identifier: string, returnType: string) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        AbstractMember.Identifier.WithValue(identifier),
                        AbstractMember.HasGetterSetter.WithValue(true, false)
                    ),
                    ValueSome [| AbstractMember.ReturnType.WithValue(Ast.LongIdent(returnType).Compile()) |],
                    ValueNone
                )
            )

        static member AbstractSet(identifier: string, returnType: WidgetBuilder<Type>) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        AbstractMember.Identifier.WithValue(identifier),
                        AbstractMember.HasGetterSetter.WithValue(false, true)
                    ),
                    ValueSome [| AbstractMember.ReturnType.WithValue(returnType.Compile()) |],
                    ValueNone
                )
            )

        static member AbstractSet(identifier: string, returnType: string) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        AbstractMember.Identifier.WithValue(identifier),
                        AbstractMember.HasGetterSetter.WithValue(false, true)
                    ),
                    ValueSome [| AbstractMember.ReturnType.WithValue(Ast.LongIdent(returnType).Compile()) |],
                    ValueNone
                )
            )

        static member AbstractGetSet(identifier: string, returnType: WidgetBuilder<Type>) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        AbstractMember.Identifier.WithValue(identifier),
                        AbstractMember.HasGetterSetter.WithValue(true, true)
                    ),
                    ValueSome [| AbstractMember.ReturnType.WithValue(returnType.Compile()) |],
                    ValueNone
                )
            )

        static member AbstractGetSet(identifier: string, returnType: string) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        AbstractMember.Identifier.WithValue(identifier),
                        AbstractMember.HasGetterSetter.WithValue(true, true)
                    ),
                    ValueSome [| AbstractMember.ReturnType.WithValue(Ast.LongIdent(returnType).Compile()) |],
                    ValueNone
                )
            )

        static member AbstractTupledMethod
            (identifier: string, parameters: WidgetBuilder<Type> list, returnType: WidgetBuilder<Type>)
            =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        AbstractMember.Identifier.WithValue(identifier),
                        AbstractMember.Parameters.WithValue(UnNamed(parameters, true))
                    ),
                    ValueSome [| AbstractMember.ReturnType.WithValue(returnType.Compile()) |],
                    ValueNone
                )
            )

        static member AbstractTupledMethod(identifier: string, parameters: string list, returnType: string) =
            let parameters = parameters |> List.map(Ast.LongIdent)

            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        AbstractMember.Identifier.WithValue(identifier),
                        AbstractMember.Parameters.WithValue(UnNamed(parameters, true))
                    ),
                    ValueSome [| AbstractMember.ReturnType.WithValue(Ast.LongIdent(returnType).Compile()) |],
                    ValueNone
                )
            )

        static member AbstractTupledMethod
            (identifier: string, parameters: (string option * WidgetBuilder<Type>) list, returnType: WidgetBuilder<Type>) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        AbstractMember.Identifier.WithValue(identifier),
                        AbstractMember.Parameters.WithValue(Named(parameters, true))
                    ),
                    ValueSome [| AbstractMember.ReturnType.WithValue(returnType.Compile()) |],
                    ValueNone
                )
            )

        static member AbstractTupledMethod
            (identifier: string, parameters: (string option * string) list, returnType: string)
            =
            let parameters =
                parameters |> List.map(fun (name, value) -> name, Ast.LongIdent value)

            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        AbstractMember.Identifier.WithValue(identifier),
                        AbstractMember.Parameters.WithValue(Named(parameters, true))
                    ),
                    ValueSome [| AbstractMember.ReturnType.WithValue(Ast.LongIdent(returnType).Compile()) |],
                    ValueNone
                )
            )

        static member AbstractCurriedMethod
            (identifier: string, parameters: WidgetBuilder<Type> list, returnType: WidgetBuilder<Type>)
            =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        AbstractMember.Identifier.WithValue(identifier),
                        AbstractMember.Parameters.WithValue(UnNamed(parameters, false))
                    ),
                    ValueSome [| AbstractMember.ReturnType.WithValue(returnType.Compile()) |],
                    ValueNone
                )
            )

        static member AbstractCurriedMethod(identifier: string, parameters: string list, returnType: string) =
            let parameters = parameters |> List.map Ast.LongIdent

            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        AbstractMember.Identifier.WithValue(identifier),
                        AbstractMember.Parameters.WithValue(UnNamed(parameters, false))
                    ),
                    ValueSome [| AbstractMember.ReturnType.WithValue(Ast.LongIdent(returnType).Compile()) |],
                    ValueNone
                )
            )

        static member AbstractCurriedMethod
            (identifier: string, parameters: (string option * WidgetBuilder<Type>) list, returnType: WidgetBuilder<Type>) =
            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        AbstractMember.Identifier.WithValue(identifier),
                        AbstractMember.Parameters.WithValue(Named(parameters, false))
                    ),
                    ValueSome [| AbstractMember.ReturnType.WithValue(returnType.Compile()) |],
                    ValueNone
                )
            )

        static member AbstractCurriedMethod
            (identifier: string, parameters: (string option * string) list, returnType: string)
            =
            let parameters =
                parameters |> List.map(fun (name, value) -> name, Ast.LongIdent value)

            WidgetBuilder<MemberDefnAbstractSlotNode>(
                AbstractMember.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        AbstractMember.Identifier.WithValue(identifier),
                        AbstractMember.Parameters.WithValue(Named(parameters, false))
                    ),
                    ValueSome [| AbstractMember.ReturnType.WithValue(Ast.LongIdent(returnType).Compile()) |],
                    ValueNone
                )
            )

[<Extension>]
type AbstractMemberModifiers =
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<MemberDefnAbstractSlotNode>, comments: string list) =
        this.AddScalar(AbstractMember.XmlDocs.WithValue(comments))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<MemberDefnAbstractSlotNode>) =
        AttributeCollectionBuilder<MemberDefnAbstractSlotNode, AttributeNode>(this, AbstractMember.MultipleAttributes)

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<MemberDefnAbstractSlotNode>, attributes: string list) =
        AttributeCollectionBuilder<MemberDefnAbstractSlotNode, AttributeNode>(this, AbstractMember.MultipleAttributes) {
            for attribute in attributes do
                Ast.Attribute(attribute)
        }

    [<Extension>]
    static member inline attribute
        (this: WidgetBuilder<MemberDefnAbstractSlotNode>, attribute: WidgetBuilder<AttributeNode>)
        =
        AttributeCollectionBuilder<MemberDefnAbstractSlotNode, AttributeNode>(this, AbstractMember.MultipleAttributes) {
            attribute
        }

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<MemberDefnAbstractSlotNode>, attribute: string) =
        AttributeCollectionBuilder<MemberDefnAbstractSlotNode, AttributeNode>(this, AbstractMember.MultipleAttributes) {
            Ast.Attribute(attribute)
        }
