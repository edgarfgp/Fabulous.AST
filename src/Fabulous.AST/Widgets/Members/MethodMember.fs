namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

type MethodMemberNode
    (
        xmlDoc: XmlDocNode option,
        accessibility: SingleTextNode option,
        name: string,
        expr: Expr,
        parameter: Pattern list,
        returnType: BindingReturnInfoNode option,
        multipleAttributes: MultipleAttributeListNode option,
        inlineNode: SingleTextNode option,
        genericTypeParameters: TyparDecls option,
        multipleTextsNode: SingleTextNode list
    ) =
    inherit
        BindingNode(
            xmlDoc,
            multipleAttributes,
            MultipleTextsNode(multipleTextsNode, Range.Zero),
            false,
            inlineNode,
            accessibility,
            Choice1Of2(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero)),
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
    let Parameters = Attributes.defineWidget "Parameters"
    let BodyExpr = Attributes.defineWidget "BodyExpr"
    let IsInlined = Attributes.defineScalar<bool> "IsInlined"
    let IsStatic = Attributes.defineScalar<bool> "IsStatic"
    let ReturnType = Attributes.defineScalar<Type> "ReturnType"
    let MultipleAttributes = Attributes.defineWidget "MultipleAttributes"
    let TypeParams = Attributes.defineScalar<string list> "TypeParams"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let WidgetKey =
        Widgets.register "MethodMember" (fun widget ->
            let name = Helpers.getScalarValue widget Name
            let parameters = Helpers.tryGetNodeFromWidget<Pattern> widget Parameters

            let parameters =
                match parameters with
                | ValueSome parameters -> [ parameters ]
                | ValueNone -> []

            let bodyExpr = Helpers.getNodeFromWidget<Expr> widget BodyExpr
            let isInlined = Helpers.tryGetScalarValue widget IsInlined

            let isStatic =
                Helpers.tryGetScalarValue widget IsStatic |> ValueOption.defaultValue false

            let returnType = Helpers.tryGetScalarValue widget ReturnType

            let accessControl =
                Helpers.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let lines = Helpers.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            let returnType =
                match returnType with
                | ValueSome returnType -> Some(BindingReturnInfoNode(SingleTextNode.colon, returnType, Range.Zero))
                | ValueNone -> None

            let attributes =
                Helpers.tryGetNodeFromWidget<AttributeListNode> widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | ValueSome values -> Some(MultipleAttributeListNode([ values ], Range.Zero))
                | ValueNone -> None


            let inlineNode =
                match isInlined with
                | ValueSome true -> Some(SingleTextNode.``inline``)
                | ValueSome false -> None
                | ValueNone -> None

            let typeParams = Helpers.tryGetScalarValue widget TypeParams

            let typeParams =
                match typeParams with
                | ValueSome values ->
                    TyparDeclsPostfixListNode(
                        SingleTextNode.lessThan,
                        [ for v in values do
                              TyparDeclNode(None, SingleTextNode.Create v, [], Range.Zero) ],
                        [],
                        SingleTextNode.greaterThan,
                        Range.Zero
                    )
                    |> TyparDecls.PostfixList
                    |> Some
                | ValueNone -> None

            let multipleTextsNode =
                [ if isStatic then
                      SingleTextNode.``static``
                      SingleTextNode.``member``
                  else
                      SingleTextNode.``member`` ]

            MethodMemberNode(
                xmlDocs,
                accessControl,
                name,
                bodyExpr,
                parameters,
                returnType,
                multipleAttributes,
                inlineNode,
                typeParams,
                multipleTextsNode
            ))

[<Extension>]
type MethodMemberModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<MethodMemberNode>, comments: string list) =
        this.AddScalar(MethodMember.XmlDocs.WithValue(comments))

    [<Extension>]
    static member inline attributes
        (
            this: WidgetBuilder<MethodMemberNode>,
            attributes: WidgetBuilder<AttributeListNode>
        ) =
        this.AddWidget(MethodMember.MultipleAttributes.WithValue(attributes.Compile()))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<MethodMemberNode>, attribute: WidgetBuilder<AttributeNode>) =
        this.AddWidget(MethodMember.MultipleAttributes.WithValue((Ast.AttributeNodes() { attribute }).Compile()))


    [<Extension>]
    static member inline typeParameters(this: WidgetBuilder<MethodMemberNode>, typeParams: string list) =
        this.AddScalar(MethodMember.TypeParams.WithValue(typeParams))

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<MethodMemberNode>) =
        this.AddScalar(MethodMember.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<MethodMemberNode>) =
        this.AddScalar(MethodMember.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<MethodMemberNode>) =
        this.AddScalar(MethodMember.Accessibility.WithValue(AccessControl.Internal))

    [<Extension>]
    static member inline returnType(this: WidgetBuilder<MethodMemberNode>, returnType: Type) =
        this.AddScalar(MethodMember.ReturnType.WithValue(returnType))

    [<Extension>]
    static member inline returnType(this: WidgetBuilder<MethodMemberNode>, returnType: string) =
        this.AddScalar(MethodMember.ReturnType.WithValue(CommonType.mkLongIdent(returnType)))
