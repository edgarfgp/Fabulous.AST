namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

type PropertyMemberNode
    (
        xmlDoc: XmlDocNode option,
        accessibility: SingleTextNode option,
        functionName: Choice<IdentListNode, Pattern>,
        expr: Expr,
        returnType: BindingReturnInfoNode option,
        multipleAttributes: MultipleAttributeListNode option,
        inlineNode: SingleTextNode option,
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
            None,
            [],
            returnType,
            SingleTextNode.equals,
            expr,
            Range.Zero
        )

module PropertyMember =
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"
    let Name = Attributes.defineScalar<string> "Name"
    let BodyExpr = Attributes.defineWidget "BodyExpr"
    let IsInlined = Attributes.defineScalar<bool> "IsInlined"
    let IsStatic = Attributes.defineScalar<bool> "IsStatic"
    let ReturnType = Attributes.defineScalar<Type option> "ReturnType"
    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let WidgetKey =
        Widgets.register "PropertyMember" (fun widget ->
            let name = Helpers.getScalarValue widget Name
            let bodyExpr = Helpers.getNodeFromWidget<Expr> widget BodyExpr
            let isInlined = Helpers.tryGetScalarValue widget IsInlined
            let attributes = Helpers.tryGetScalarValue widget MultipleAttributes
            let returnType = Helpers.tryGetScalarValue widget ReturnType
            let lines = Helpers.tryGetScalarValue widget XmlDocs
            let isStatic = Helpers.getScalarValue widget IsStatic

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

            let multipleTextsNode = if isStatic then [ "static"; "member" ] else [ "member" ]

            PropertyMemberNode(
                xmlDocs,
                accessControl,
                functionName,
                bodyExpr,
                returnType,
                multipleAttributes,
                inlineNode,
                multipleTextsNode
            ))

[<AutoOpen>]
module PropertyMemberBuilders =
    type Ast with

        static member PropertyMember(name: string, ?returnType: Type) =
            SingleChildBuilder<PropertyMemberNode, Expr>(
                PropertyMember.WidgetKey,
                PropertyMember.BodyExpr,
                AttributesBundle(
                    StackList.three(
                        PropertyMember.Name.WithValue(name),
                        PropertyMember.ReturnType.WithValue(returnType),
                        PropertyMember.IsStatic.WithValue(false)
                    ),
                    ValueNone,
                    ValueNone
                )
            )

        static member StaticPropertyMember(name: string, ?returnType: Type) =
            SingleChildBuilder<PropertyMemberNode, Expr>(
                PropertyMember.WidgetKey,
                PropertyMember.BodyExpr,
                AttributesBundle(
                    StackList.three(
                        PropertyMember.Name.WithValue(name),
                        PropertyMember.ReturnType.WithValue(returnType),
                        PropertyMember.IsStatic.WithValue(true)
                    ),
                    ValueNone,
                    ValueNone
                )
            )

[<Extension>]
type PropertyMemberModifiers =
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<PropertyMemberNode>, comments: string list) =
        this.AddScalar(PropertyMember.XmlDocs.WithValue(comments))

    [<Extension>]
    static member inline isInlined(this: WidgetBuilder<PropertyMemberNode>) =
        this.AddScalar(PropertyMember.IsInlined.WithValue(true))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<PropertyMemberNode>, attributes) =
        this.AddScalar(PropertyMember.MultipleAttributes.WithValue(attributes))

    [<Extension>]
    static member inline accessibility(this: WidgetBuilder<PropertyMemberNode>, value: AccessControl) =
        this.AddScalar(PropertyMember.Accessibility.WithValue(value))
