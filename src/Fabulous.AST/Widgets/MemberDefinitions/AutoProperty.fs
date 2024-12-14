namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module AutoPropertyMember =
    let XmlDocs = Attributes.defineWidget "XmlDocs"
    let Identifier = Attributes.defineScalar<string> "Identifier"
    let ReturnType = Attributes.defineWidget "Type"
    let Parameters = Attributes.defineScalar<MethodParamsType> "Parameters"
    let HasGetterSetter = Attributes.defineScalar<bool * bool> "HasGetterSetter"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let IsStatic = Attributes.defineScalar<bool> "IsStatic"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"
    let BodyExpr = Attributes.defineWidget "BodyExpr"

    let WidgetKey =
        Widgets.register "AutoPropertyMember" (fun widget ->
            let identifier = Widgets.getScalarValue widget Identifier
            let hasGetterSetter = Widgets.tryGetScalarValue widget HasGetterSetter

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let isStatic =
                Widgets.tryGetScalarValue widget IsStatic |> ValueOption.defaultValue false

            let returnType = Widgets.tryGetNodeFromWidget widget ReturnType

            let bodyExpr = Widgets.getNodeFromWidget widget BodyExpr

            let returnType =
                match returnType with
                | ValueSome tp -> Some tp
                | ValueNone -> None

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let multipleTextsNode =
                MultipleTextsNode(
                    [ if isStatic then
                          SingleTextNode.``static``
                          SingleTextNode.``member``
                          SingleTextNode.``val``
                      else
                          SingleTextNode.``member``
                          SingleTextNode.``val`` ],
                    Range.Zero
                )

            let withGetSetText =
                match hasGetterSetter with
                | ValueSome(true, true) ->
                    Some(
                        MultipleTextsNode.Create(
                            [ SingleTextNode.``with``; SingleTextNode.Create("get,"); SingleTextNode.set ]
                        )
                    )
                | ValueSome(true, false) ->
                    Some(MultipleTextsNode.Create([ SingleTextNode.``with``; SingleTextNode.get ]))
                | ValueSome(false, true) ->
                    Some(MultipleTextsNode.Create([ SingleTextNode.``with``; SingleTextNode.set ]))
                | ValueSome(false, false)
                | ValueNone -> None

            MemberDefnAutoPropertyNode(
                xmlDocs,
                attributes,
                multipleTextsNode,
                accessControl,
                SingleTextNode.Create(identifier),
                returnType,
                SingleTextNode.equals,
                bodyExpr,
                withGetSetText,
                Range.Zero
            ))

[<AutoOpen>]
module AutoPropertyMemberBuilders =
    type Ast with
        static member MemberVal(identifier: string, expr: WidgetBuilder<Expr>, ?hasGetter: bool, ?hasSetter: bool) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false

            WidgetBuilder<MemberDefnAutoPropertyNode>(
                AutoPropertyMember.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        AutoPropertyMember.Identifier.WithValue(identifier),
                        AutoPropertyMember.HasGetterSetter.WithValue(hasGetter, hasSetter)
                    ),
                    [| AutoPropertyMember.BodyExpr.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member MemberVal(identifier: string, expr: WidgetBuilder<Constant>, ?hasGetter: bool, ?hasSetter: bool) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            Ast.MemberVal(identifier, Ast.ConstantExpr(expr), hasGetter, hasSetter)

        static member MemberVal(identifier: string, expr: string, ?hasGetter: bool, ?hasSetter: bool) =
            let hasGetter = defaultArg hasGetter false
            let hasSetter = defaultArg hasSetter false
            Ast.MemberVal(identifier, Ast.Constant(expr), hasGetter, hasSetter)

type AutoPropertyMemberModifiers =
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<MemberDefnAutoPropertyNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(AutoPropertyMember.XmlDocs.WithValue(xmlDocs.Compile()))

    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<MemberDefnAutoPropertyNode>, xmlDocs: string list) =
        AutoPropertyMemberModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<MemberDefnAutoPropertyNode>, values: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            AutoPropertyMember.MultipleAttributes.WithValue(
                [ for vals in values do
                      Gen.mkOak vals ]
            )
        )

    [<Extension>]
    static member inline attribute
        (this: WidgetBuilder<MemberDefnAutoPropertyNode>, value: WidgetBuilder<AttributeNode>)
        =
        AutoPropertyMemberModifiers.attributes(this, [ value ])

    [<Extension>]
    static member inline toStatic(this: WidgetBuilder<MemberDefnAutoPropertyNode>) =
        this.AddScalar(AutoPropertyMember.IsStatic.WithValue(true))

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<MemberDefnAutoPropertyNode>) =
        this.AddScalar(AutoPropertyMember.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<MemberDefnAutoPropertyNode>) =
        this.AddScalar(AutoPropertyMember.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<MemberDefnAutoPropertyNode>) =
        this.AddScalar(AutoPropertyMember.Accessibility.WithValue(AccessControl.Internal))

    [<Extension>]
    static member inline returnType(this: WidgetBuilder<MemberDefnAutoPropertyNode>, value: WidgetBuilder<Type>) =
        this.AddWidget(AutoPropertyMember.ReturnType.WithValue(value.Compile()))
