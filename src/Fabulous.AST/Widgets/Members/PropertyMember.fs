namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

type PropertyMemberNode
    (
        xmlDoc: XmlDocNode option,
        accessibility: SingleTextNode option,
        name: Pattern,
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
            Choice2Of2(name),
            None,
            [],
            returnType,
            SingleTextNode.equals,
            expr,
            Range.Zero
        )

module PropertyMember =
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"
    let Name = Attributes.defineWidget "Name"
    let BodyExpr = Attributes.defineWidget "BodyExpr"
    let IsInlined = Attributes.defineScalar<bool> "IsInlined"
    let IsStatic = Attributes.defineScalar<bool> "IsStatic"
    let ReturnType = Attributes.defineScalar<Type> "ReturnType"
    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let WidgetKey =
        Widgets.register "PropertyMember" (fun widget ->
            let name = Helpers.getNodeFromWidget<Pattern> widget Name
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
                | ValueSome returnType -> Some(BindingReturnInfoNode(SingleTextNode.colon, returnType, Range.Zero))
                | ValueNone -> None

            let multipleAttributes =
                match attributes with
                | ValueSome values -> MultipleAttributeListNode.Create values |> Some
                | ValueNone -> None

            let inlineNode =
                match isInlined with
                | ValueSome true -> Some(SingleTextNode.``inline``)
                | ValueSome false -> None
                | ValueNone -> None

            let multipleTextsNode = if isStatic then [ "static"; "member" ] else [ "member" ]

            PropertyMemberNode(
                xmlDocs,
                accessControl,
                name,
                bodyExpr,
                returnType,
                multipleAttributes,
                inlineNode,
                multipleTextsNode
            ))

[<AutoOpen>]
module PropertyMemberBuilders =
    type Ast with

        static member private BasePropertyMember(name: WidgetBuilder<Pattern>, isStatic: bool, isInlined: bool) =
            SingleChildBuilder<PropertyMemberNode, Expr>(
                PropertyMember.WidgetKey,
                PropertyMember.BodyExpr,
                AttributesBundle(
                    StackList.two(
                        PropertyMember.IsStatic.WithValue(isStatic),
                        PropertyMember.IsInlined.WithValue(isInlined)
                    ),
                    ValueSome [| PropertyMember.Name.WithValue(name.Compile()) |],
                    ValueNone
                )
            )

        static member private BaseMethodMember
            (
                name: string,
                isStatic: bool,
                isInline: bool,
                parameters: WidgetBuilder<Pattern> voption
            ) =
            let widgets =
                match parameters with
                | ValueSome parameters -> ValueSome [| MethodMember.Parameters.WithValue(parameters.Compile()) |]
                | ValueNone -> ValueNone

            SingleChildBuilder<MethodMemberNode, Expr>(
                MethodMember.WidgetKey,
                MethodMember.BodyExpr,
                AttributesBundle(
                    StackList.three(
                        MethodMember.Name.WithValue(name),
                        MethodMember.IsStatic.WithValue(isStatic),
                        MethodMember.IsInlined.WithValue(isInline)
                    ),
                    widgets,
                    ValueNone
                )
            )

        static member Member(name: WidgetBuilder<Pattern>) =
            Ast.BasePropertyMember(name, false, false)

        static member Member(name: string, parameters: WidgetBuilder<Pattern>) =
            Ast.BaseMethodMember(name, false, false, ValueSome parameters)

        static member Member(name: string) =
            Ast.BasePropertyMember(Ast.NamedPat(name), false, false)

        static member InlinedMember(name: WidgetBuilder<Pattern>) =
            Ast.BasePropertyMember(name, false, true)

        static member InlinedMember(name: string, parameters: WidgetBuilder<Pattern>) =
            Ast.BaseMethodMember(name, false, true, ValueSome parameters)

        static member InlinedMember(name: string) =
            Ast.BasePropertyMember(Ast.NamedPat(name), false, true)

        static member StaticMember(name: WidgetBuilder<Pattern>) =
            Ast.BasePropertyMember(name, true, false)

        static member StaticMember(name: string, parameters: WidgetBuilder<Pattern>) =
            Ast.BaseMethodMember(name, true, false, ValueSome parameters)

        static member StaticMember(name: string) =
            Ast.BasePropertyMember(Ast.NamedPat(name), true, false)

        static member InlinedStaticMember(name: WidgetBuilder<Pattern>) =
            Ast.BasePropertyMember(name, true, true)

        static member InlinedStaticMember(name: string, parameters: WidgetBuilder<Pattern>) =
            Ast.BaseMethodMember(name, true, true, ValueSome parameters)

        static member InlinedStaticMember(name: string) =
            Ast.BasePropertyMember(Ast.NamedPat(name), true, true)

[<Extension>]
type PropertyMemberModifiers =
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<PropertyMemberNode>, comments: string list) =
        this.AddScalar(PropertyMember.XmlDocs.WithValue(comments))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<PropertyMemberNode>, attributes) =
        this.AddScalar(PropertyMember.MultipleAttributes.WithValue(attributes))

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<PropertyMemberNode>) =
        this.AddScalar(PropertyMember.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<PropertyMemberNode>) =
        this.AddScalar(PropertyMember.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<PropertyMemberNode>) =
        this.AddScalar(PropertyMember.Accessibility.WithValue(AccessControl.Internal))

    [<Extension>]
    static member inline returnType(this: WidgetBuilder<PropertyMemberNode>, returnType: Type) =
        this.AddScalar(PropertyMember.ReturnType.WithValue(returnType))

    [<Extension>]
    static member inline returnType(this: WidgetBuilder<PropertyMemberNode>, returnType: string) =
        this.AddScalar(PropertyMember.ReturnType.WithValue(CommonType.mkLongIdent(returnType)))
