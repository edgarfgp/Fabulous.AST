namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

type MethodMemberNode
    (
        xmlDoc: XmlDocNode option,
        accessibility: SingleTextNode option,
        functionName: Choice<IdentListNode, Pattern>,
        expr: Expr,
        parameter: Pattern list,
        returnType: BindingReturnInfoNode option,
        multipleAttributes: MultipleAttributeListNode option,
        inlineNode: SingleTextNode option,
        genericTypeParameters: TyparDecls option,
        multipleTextsNode: string list
    ) =
    inherit
        BindingNode(
            xmlDoc,
            multipleAttributes,
            MultipleTextsNode.Create(multipleTextsNode),
            false,
            inlineNode,
            accessibility,
            functionName,
            genericTypeParameters,
            parameter,
            returnType,
            SingleTextNode.equals,
            expr,
            Range.Zero
        )

module MethodMember =
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"
    let Name = Attributes.defineScalar<string> "Name"
    let Parameters = Attributes.defineScalar<Pattern list option> "Parameters"
    let BodyExpr = Attributes.defineWidget "BodyExpr"
    let IsInlined = Attributes.defineScalar<bool> "IsInlined"
    let IsStatic = Attributes.defineScalar<bool> "IsStatic"
    let ReturnType = Attributes.defineScalar<Type option> "ReturnType"
    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"
    let TypeParams = Attributes.defineScalar<string list> "TypeParams"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let WidgetKey =
        Widgets.register "MethodMember" (fun widget ->
            let name = Helpers.getScalarValue widget Name
            let parameters = Helpers.tryGetScalarValue widget Parameters
            let bodyExpr = Helpers.getNodeFromWidget<Expr> widget BodyExpr
            let isInlined = Helpers.tryGetScalarValue widget IsInlined
            let isStatic = Helpers.getScalarValue widget IsStatic
            let attributes = Helpers.tryGetScalarValue widget MultipleAttributes
            let returnType = Helpers.tryGetScalarValue widget ReturnType
            let lines = Helpers.tryGetScalarValue widget XmlDocs

            let accessControl =
                Helpers.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            let returnType =
                match returnType with
                | ValueSome(Some returnType) ->
                    Some(BindingReturnInfoNode(SingleTextNode.colon, returnType, Range.Zero))
                | ValueSome(None) -> None
                | ValueNone -> None

            let parameters =
                match parameters with
                | ValueNone
                | ValueSome(None) -> [ Pattern.CreateUnit() ]
                | ValueSome(Some parameters) when parameters.IsEmpty -> [ Pattern.CreateUnit() ]
                | ValueSome(Some parameters) -> parameters

            let multipleAttributes =
                match attributes with
                | ValueSome values -> MultipleAttributeListNode.Create values |> Some
                | ValueNone -> None

            let identifiers =
                name.Trim()
                |> Seq.toArray
                |> Array.map(fun s -> IdentifierOrDot.CreateIdent($"{s}"))
                |> Array.toList

            let inlineNode =
                match isInlined with
                | ValueSome true -> Some(SingleTextNode.``inline``)
                | ValueSome false -> None
                | ValueNone -> None

            let functionName = Choice1Of2(IdentListNode.Create(identifiers))

            let typeParams = Helpers.tryGetScalarValue widget TypeParams

            let typeParams =
                match typeParams with
                | ValueSome values ->
                    TyparDeclsPostfixListNode(
                        SingleTextNode.lessThan,
                        [ for v in values do
                              // FIXME - Update
                              TyparDeclNode(None, SingleTextNode.Create v, [], Range.Zero) ],
                        [],
                        SingleTextNode.greaterThan,
                        Range.Zero
                    )
                    |> TyparDecls.PostfixList
                    |> Some
                | ValueNone -> None

            let multipleTextsNode = if isStatic then [ "static"; "member" ] else [ "member" ]

            MethodMemberNode(
                xmlDocs,
                accessControl,
                functionName,
                bodyExpr,
                parameters,
                returnType,
                multipleAttributes,
                inlineNode,
                typeParams,
                multipleTextsNode
            ))

[<AutoOpen>]
module MemberBuilders =
    type Ast with

        static member MethodMember(name: string, ?parameters: Pattern list) =
            SingleChildBuilder<MethodMemberNode, Expr>(
                MethodMember.WidgetKey,
                MethodMember.BodyExpr,
                AttributesBundle(
                    StackList.three(
                        MethodMember.Name.WithValue(name),
                        MethodMember.Parameters.WithValue(parameters),
                        MethodMember.IsStatic.WithValue(false)
                    ),
                    ValueNone,
                    ValueNone
                )
            )

        static member inline StaticMethodMember(name: string, ?parameters: Pattern list) =
            SingleChildBuilder<MethodMemberNode, Expr>(
                MethodMember.WidgetKey,
                MethodMember.BodyExpr,
                AttributesBundle(
                    StackList.three(
                        MethodMember.Name.WithValue(name),
                        MethodMember.Parameters.WithValue(parameters),
                        MethodMember.IsStatic.WithValue(true)
                    ),
                    ValueNone,
                    ValueNone
                )
            )

[<Extension>]
type MethodMemberModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<MethodMemberNode>, comments: string list) =
        this.AddScalar(MethodMember.XmlDocs.WithValue(comments))

    [<Extension>]
    static member inline isInlined(this: WidgetBuilder<MethodMemberNode>) =
        this.AddScalar(MethodMember.IsInlined.WithValue(true))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<MethodMemberNode>, attributes) =
        this.AddScalar(MethodMember.MultipleAttributes.WithValue(attributes))

    [<Extension>]
    static member inline genericTypeParameters(this: WidgetBuilder<MethodMemberNode>, typeParams: string list) =
        this.AddScalar(MethodMember.TypeParams.WithValue(typeParams))

    [<Extension>]
    static member inline accessibility(this: WidgetBuilder<MethodMemberNode>, value: AccessControl) =
        this.AddScalar(MethodMember.Accessibility.WithValue(value))

    [<Extension>]
    static member inline returnType(this: WidgetBuilder<MethodMemberNode>, returnType: Type) =
        this.AddScalar(MethodMember.ReturnType.WithValue(Some(returnType)))
