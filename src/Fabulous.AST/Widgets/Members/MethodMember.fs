namespace Fabulous.AST

open System.Runtime.CompilerServices
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
    let Name = Attributes.defineWidget "Name"
    let Parameters = Attributes.defineWidget "Parameters"
    let BodyExpr = Attributes.defineWidget "BodyExpr"
    let IsInlined = Attributes.defineScalar<bool> "IsInlined"
    let IsStatic = Attributes.defineScalar<bool> "IsStatic"
    let ReturnType = Attributes.defineScalar<Type option> "ReturnType"
    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"
    let TypeParams = Attributes.defineScalar<string list> "TypeParams"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let WidgetKey =
        Widgets.register "MethodMember" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name
            let parameters = Helpers.getNodeFromWidget<Pattern> widget Parameters
            let bodyExpr = Helpers.getNodeFromWidget<Expr> widget BodyExpr
            let isInlined = Helpers.tryGetScalarValue widget IsInlined
            let isStatic = Helpers.tryGetScalarValue widget IsStatic
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

            let multipleAttributes =
                match attributes with
                | ValueSome values -> MultipleAttributeListNode.Create values |> Some
                | ValueNone -> None

            let identifiers =
                name.Text.Trim()
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

            let multipleTextsNode =
                let isStatic = defaultValueArg isStatic false

                [ if isStatic then
                      SingleTextNode.``static``
                      SingleTextNode.``member``
                  else
                      SingleTextNode.``member`` ]

            MethodMemberNode(
                xmlDocs,
                accessControl,
                functionName,
                bodyExpr,
                [ parameters ],
                returnType,
                multipleAttributes,
                inlineNode,
                typeParams,
                multipleTextsNode
            ))

[<AutoOpen>]
module MemberBuilders =
    type Ast with
        static member inline MethodMember(name: SingleTextNode, parameters: Pattern) =
            Ast.MethodMember(Ast.EscapeHatch(name), Ast.EscapeHatch(parameters))

        static member inline MethodMember(name: string, parameters: Pattern) =
            Ast.MethodMember(SingleTextNode.Create(name), parameters)

        static member MethodMember(name: string, parameters: WidgetBuilder<Pattern>) =
            Ast.MethodMember(Ast.EscapeHatch(SingleTextNode.Create(name)), parameters)

        static member MethodMember(name: WidgetBuilder<SingleTextNode>, parameters: WidgetBuilder<Pattern>) =
            SingleChildBuilder<MethodMemberNode, Expr>(
                MethodMember.WidgetKey,
                MethodMember.BodyExpr,
                AttributesBundle(
                    StackList.one(MethodMember.IsStatic.WithValue(false)),
                    ValueSome
                        [| MethodMember.Name.WithValue(name.Compile())
                           MethodMember.Parameters.WithValue(parameters.Compile()) |],
                    ValueNone
                )
            )

        static member inline StaticMethodMember(name: SingleTextNode, parameters: Pattern) =
            Ast.StaticMethodMember(Ast.EscapeHatch(name), Ast.EscapeHatch(parameters))

        static member inline StaticMethodMember(name: string, parameters: Pattern) =
            Ast.MethodMember(SingleTextNode.Create(name), parameters)

        static member StaticMethodMember(name: string, parameters: WidgetBuilder<Pattern>) =
            Ast.StaticMethodMember(Ast.EscapeHatch(SingleTextNode.Create(name)), parameters)

        static member StaticMethodMember(name: WidgetBuilder<SingleTextNode>, parameters: WidgetBuilder<Pattern>) =
            SingleChildBuilder<MethodMemberNode, Expr>(
                MethodMember.WidgetKey,
                MethodMember.BodyExpr,
                AttributesBundle(
                    StackList.one(MethodMember.IsStatic.WithValue(true)),
                    ValueSome
                        [| MethodMember.Name.WithValue(name.Compile())
                           MethodMember.Parameters.WithValue(parameters.Compile()) |],
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
