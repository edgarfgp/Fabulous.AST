namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ExplicitConstructorMember =
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let Pat = Attributes.defineScalar<StringOrWidget<Pattern>> "Pat"

    let Alias = Attributes.defineScalar<string> "Alias"

    let ExprValue = Attributes.defineScalar<StringOrWidget<Expr>> "ExprValue"

    let WidgetKey =
        Widgets.register "ExplicitConstructorMember" (fun widget ->
            let lines = Widgets.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            let attributes = Widgets.tryGetScalarValue widget MultipleAttributes

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

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let pat = Widgets.getScalarValue widget Pat

            let pat =
                match pat with
                | StringOrWidget.StringExpr value ->
                    let value = StringParsing.normalizeIdentifierQuotes value
                    Pattern.Named(PatNamedNode(None, SingleTextNode.Create(value), Range.Zero))
                | StringOrWidget.WidgetExpr pat -> pat

            let alias =
                match Widgets.tryGetScalarValue widget Alias with
                | ValueSome value -> Some(SingleTextNode.Create value)
                | ValueNone -> None

            let expr = Widgets.getScalarValue widget ExprValue

            let expr =
                match expr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value)))
                    )
                | StringOrWidget.WidgetExpr expr -> expr

            MemberDefnExplicitCtorNode(
                xmlDocs,
                multipleAttributes,
                accessControl,
                SingleTextNode.``new``,
                pat,
                alias,
                SingleTextNode.equals,
                expr,
                Range.Zero
            ))

[<AutoOpen>]
module ExplicitConstructorBuilders =
    type Ast with

        static member ExplicitCtor(pattern: WidgetBuilder<Pattern>, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<MemberDefnExplicitCtorNode>(
                ExplicitConstructorMember.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        ExplicitConstructorMember.Pat.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak pattern)),
                        ExplicitConstructorMember.ExprValue.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak expr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member ExplicitCtor(pattern: StringVariant, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<MemberDefnExplicitCtorNode>(
                ExplicitConstructorMember.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        ExplicitConstructorMember.Pat.WithValue(StringOrWidget.StringExpr(pattern)),
                        ExplicitConstructorMember.ExprValue.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak expr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member ExplicitCtor(pattern: WidgetBuilder<Pattern>, expr: StringVariant) =
            WidgetBuilder<MemberDefnExplicitCtorNode>(
                ExplicitConstructorMember.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        ExplicitConstructorMember.Pat.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak pattern)),
                        ExplicitConstructorMember.ExprValue.WithValue(StringOrWidget.StringExpr(expr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member ExplicitCtor(pattern: WidgetBuilder<Pattern>, expr: WidgetBuilder<Expr>, alias: string) =
            WidgetBuilder<MemberDefnExplicitCtorNode>(
                ExplicitConstructorMember.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        ExplicitConstructorMember.Pat.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak pattern)),
                        ExplicitConstructorMember.ExprValue.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak expr)),
                        ExplicitConstructorMember.Alias.WithValue(alias)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member ExplicitCtor(pattern: StringVariant, expr: WidgetBuilder<Expr>, alias: string) =
            WidgetBuilder<MemberDefnExplicitCtorNode>(
                ExplicitConstructorMember.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        ExplicitConstructorMember.Pat.WithValue(StringOrWidget.StringExpr(pattern)),
                        ExplicitConstructorMember.ExprValue.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak expr)),
                        ExplicitConstructorMember.Alias.WithValue(alias)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member ExplicitCtor(pattern: WidgetBuilder<Pattern>, expr: StringVariant, alias: string) =
            WidgetBuilder<MemberDefnExplicitCtorNode>(
                ExplicitConstructorMember.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        ExplicitConstructorMember.Pat.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak pattern)),
                        ExplicitConstructorMember.ExprValue.WithValue(StringOrWidget.StringExpr(expr)),
                        ExplicitConstructorMember.Alias.WithValue(alias)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member ExplicitCtor(pattern: StringVariant, expr: StringVariant) =
            WidgetBuilder<MemberDefnExplicitCtorNode>(
                ExplicitConstructorMember.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        ExplicitConstructorMember.Pat.WithValue(StringOrWidget.StringExpr(pattern)),
                        ExplicitConstructorMember.ExprValue.WithValue(StringOrWidget.StringExpr(expr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member ExplicitCtor(pattern: StringVariant, expr: StringVariant, alias: string) =
            WidgetBuilder<MemberDefnExplicitCtorNode>(
                ExplicitConstructorMember.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        ExplicitConstructorMember.Pat.WithValue(StringOrWidget.StringExpr(pattern)),
                        ExplicitConstructorMember.ExprValue.WithValue(StringOrWidget.StringExpr(expr)),
                        ExplicitConstructorMember.Alias.WithValue(alias)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

type ExplicitConstructorModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<MemberDefnExplicitCtorNode>, xmlDocs: string list) =
        this.AddScalar(ExplicitConstructorMember.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<MemberDefnExplicitCtorNode>, attributes: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            ExplicitConstructorMember.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<MemberDefnExplicitCtorNode>, attributes: string list) =
        ExplicitConstructorModifiers.attributes(
            this,
            [ for attr in attributes do
                  Ast.Attribute(attr) ]
        )

    [<Extension>]
    static member inline attribute
        (this: WidgetBuilder<MemberDefnExplicitCtorNode>, attribute: WidgetBuilder<AttributeNode>)
        =
        ExplicitConstructorModifiers.attributes(this, [ attribute ])

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<MemberDefnExplicitCtorNode>, attribute: string) =
        ExplicitConstructorModifiers.attribute(this, Ast.Attribute(attribute))

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<MemberDefnExplicitCtorNode>) =
        this.AddScalar(ExplicitConstructorMember.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<MemberDefnExplicitCtorNode>) =
        this.AddScalar(ExplicitConstructorMember.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<MemberDefnExplicitCtorNode>) =
        this.AddScalar(ExplicitConstructorMember.Accessibility.WithValue(AccessControl.Internal))
