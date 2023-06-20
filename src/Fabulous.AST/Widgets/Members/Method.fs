namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS
open Fantomas.FCS.Text
open Fantomas.FCS.Syntax
open type Fabulous.AST.Ast

module Method =
    let Name = Attributes.defineScalar "Name"
    let Parameters = Attributes.defineScalar<Pattern option> "Parameters"
    let BodyExpr = Attributes.defineWidget "BodyExpr"
    let IsInlined = Attributes.defineScalar<bool> "IsInlined"
    let IsStatic = Attributes.defineScalar<bool> "IsStatic"
    let ReturnType = Attributes.defineScalar<BindingReturnInfoNode> "ReturnType"
    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"

    let WidgetKey =
        Widgets.register "MemberDefn" (fun widget ->
            let name = Helpers.getScalarValue widget Name
            let parameters = Helpers.getScalarValue widget Parameters
            let bodyExpr = Helpers.getNodeFromWidget<Expr> widget BodyExpr
            let isInlined = Helpers.tryGetScalarValue widget IsInlined
            let isStatic = Helpers.tryGetScalarValue widget IsStatic
            let attributes = Helpers.tryGetScalarValue widget MultipleAttributes
            let returnType = Helpers.tryGetScalarValue widget ReturnType

            let multipleAttributes =
                match attributes with
                | ValueSome values -> MultipleAttributeListNode.Create values |> Some
                | ValueNone -> None

            let returnType =
                match returnType with
                | ValueSome value -> Some value
                | ValueNone -> None

            let staticOrInstanceMember =
                match isStatic with
                | ValueNone
                | ValueSome false -> MultipleTextsNode([ SingleTextNode("member", Range.Zero) ], Range.Zero)
                | ValueSome true ->
                    MultipleTextsNode(
                        [ SingleTextNode("static", Range.Zero); SingleTextNode("member", Range.Zero) ],
                        Range.Zero
                    )

            let memberDefn =
                match isStatic with
                | ValueNone
                | ValueSome false ->
                    Choice1Of2(
                        IdentListNode(
                            [ IdentifierOrDot.Ident(SingleTextNode("this", Range.Zero))
                              IdentifierOrDot.Ident(SingleTextNode(".", Range.Zero))
                              IdentifierOrDot.Ident(SingleTextNode(name, Range.Zero)) ],
                            Range.Zero
                        )
                    )

                | ValueSome true ->
                    Choice1Of2(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode(name, Range.Zero)) ], Range.Zero))

            match parameters with
            | None ->
                MemberDefn.Member(
                    BindingNode(
                        None,
                        multipleAttributes,
                        staticOrInstanceMember,
                        false,
                        (match isInlined with
                         | ValueNone
                         | ValueSome false -> None
                         | ValueSome true -> Some(SingleTextNode("inline", Range.Zero))),
                        None,
                        memberDefn,
                        None,
                        [],
                        returnType,
                        SingleTextNode("=", Range.Zero),
                        bodyExpr,
                        Range.Zero
                    )
                )
            | Some pattern ->
                MemberDefn.Member(
                    BindingNode(
                        None,
                        multipleAttributes,
                        staticOrInstanceMember,
                        false,
                        (match isInlined with
                         | ValueNone
                         | ValueSome false -> None
                         | ValueSome true -> Some(SingleTextNode("inline", Range.Zero))),
                        None,
                        memberDefn,
                        None,
                        [ pattern ],
                        returnType,
                        SingleTextNode("=", Range.Zero),
                        bodyExpr,
                        Range.Zero
                    )
                )

        )

[<AutoOpen>]
module MemberBuilders =
    type Fabulous.AST.Ast with

        static member inline Method(name: string, ?parameters: Pattern) =
            SingleChildBuilder<MemberDefn, Expr>(
                Method.WidgetKey,
                Method.BodyExpr,
                AttributesBundle(
                    StackList.two(Method.Name.WithValue(name), Method.Parameters.WithValue(parameters)),
                    ValueNone,
                    ValueNone
                )
            )



[<Extension>]
type MemberModifiers =
    [<Extension>]
    static member inline isInlined(this: WidgetBuilder<MemberDefn>) =
        this.AddScalar(Method.IsInlined.WithValue(true))

    [<Extension>]
    static member inline isStatic(this: WidgetBuilder<MemberDefn>) =
        this.AddScalar(Method.IsStatic.WithValue(true))


    [<Extension>]
    static member inline attributes(this: WidgetBuilder<MemberDefn>, attributes) =
        this.AddScalar(Method.MultipleAttributes.WithValue(attributes))

    [<Extension>]
    static member inline returns(this: WidgetBuilder<MemberDefn>, returnType: Type) =
        let node =
            BindingReturnInfoNode(SingleTextNode(":", Range.Zero), returnType, Range.Zero)

        this.AddScalar(Method.ReturnType.WithValue(node))
