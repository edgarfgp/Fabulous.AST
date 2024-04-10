namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open Microsoft.FSharp.Collections

module AutoPropertyMember =
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"
    let Identifier = Attributes.defineScalar<string> "Identifier"
    let ReturnType = Attributes.defineScalar<StringOrWidget<Type>> "Type"
    let Parameters = Attributes.defineScalar<MethodParamsType> "Parameters"
    let HasGetterSetter = Attributes.defineScalar<bool * bool> "HasGetterSetter"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let IsStatic = Attributes.defineScalar<bool> "IsStatic"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"
    let BodyExpr = Attributes.defineScalar<StringOrWidget<Expr>> "BodyExpr"

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

            let attributes = Widgets.tryGetScalarValue widget MultipleAttributes

            let isStatic =
                Widgets.tryGetScalarValue widget IsStatic |> ValueOption.defaultValue false

            let returnType = Widgets.tryGetScalarValue widget ReturnType

            let bodyExpr = Widgets.getScalarValue widget BodyExpr

            let bodyExpr =
                match bodyExpr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value)))
                    )
                | StringOrWidget.WidgetExpr value -> value

            let returnType =
                match returnType with
                | ValueSome tp ->
                    match tp with
                    | StringOrWidget.StringExpr value ->
                        let value = StringParsing.normalizeIdentifierBackticks value

                        Some(
                            Type.LongIdent(
                                IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(value)) ], Range.Zero)
                            )
                        )
                    | StringOrWidget.WidgetExpr widget -> Some widget
                | ValueNone -> None

            let multipleAttributes =
                match attributes with
                | ValueSome values ->
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
                | ValueNone -> None

            let lines = Widgets.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

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
                | ValueSome(true, true) -> Some(MultipleTextsNode.Create([ "with"; "get,"; "set" ]))
                | ValueSome(true, false) -> Some(MultipleTextsNode.Create([ "with"; "get" ]))
                | ValueSome(false, true) -> Some(MultipleTextsNode.Create([ "with"; "set" ]))
                | ValueSome(false, false)
                | ValueNone -> None

            MemberDefnAutoPropertyNode(
                xmlDocs,
                multipleAttributes,
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

        static member AutoProperty(identifier: string, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<MemberDefnAutoPropertyNode>(
                AutoPropertyMember.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        AutoPropertyMember.Identifier.WithValue(identifier),
                        AutoPropertyMember.HasGetterSetter.WithValue(true, true),
                        AutoPropertyMember.BodyExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak expr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member AutoPropertyGet(identifier: string, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<MemberDefnAutoPropertyNode>(
                AutoPropertyMember.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        AutoPropertyMember.Identifier.WithValue(identifier),
                        AutoPropertyMember.HasGetterSetter.WithValue(true, false),
                        AutoPropertyMember.BodyExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak expr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member AutoProperty(identifier: string, expr: StringVariant) =
            WidgetBuilder<MemberDefnAutoPropertyNode>(
                AutoPropertyMember.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        AutoPropertyMember.Identifier.WithValue(identifier),
                        AutoPropertyMember.HasGetterSetter.WithValue(true, true),
                        AutoPropertyMember.BodyExpr.WithValue(StringOrWidget.StringExpr(expr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member AutoPropertyGet(identifier: string, expr: StringVariant) =
            WidgetBuilder<MemberDefnAutoPropertyNode>(
                AutoPropertyMember.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        AutoPropertyMember.Identifier.WithValue(identifier),
                        AutoPropertyMember.HasGetterSetter.WithValue(true, false),
                        AutoPropertyMember.BodyExpr.WithValue(StringOrWidget.StringExpr(expr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

[<Extension>]
type AutoPropertyMemberModifiers =
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<MemberDefnAutoPropertyNode>, values: string list) =
        this.AddScalar(AutoPropertyMember.XmlDocs.WithValue(values))

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
    static member inline attributes(this: WidgetBuilder<MemberDefnAutoPropertyNode>, values: string list) =
        AutoPropertyMemberModifiers.attributes(
            this,
            [ for attribute in values do
                  Ast.Attribute(attribute) ]
        )

    [<Extension>]
    static member inline attribute
        (this: WidgetBuilder<MemberDefnAutoPropertyNode>, value: WidgetBuilder<AttributeNode>)
        =
        AutoPropertyMemberModifiers.attributes(this, [ value ])

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<MemberDefnAutoPropertyNode>, value: string) =
        AutoPropertyMemberModifiers.attributes(this, [ Ast.Attribute(value) ])

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
        this.AddScalar(AutoPropertyMember.ReturnType.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak value)))

    [<Extension>]
    static member inline returnType(this: WidgetBuilder<MemberDefnAutoPropertyNode>, value: string) =
        this.AddScalar(AutoPropertyMember.ReturnType.WithValue(StringOrWidget.StringExpr(Unquoted value)))
