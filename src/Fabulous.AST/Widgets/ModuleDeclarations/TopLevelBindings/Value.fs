namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module BindingValue =

    let LeadingKeyword = Attributes.defineScalar<SingleTextNode> "LeadingKeyword"

    let WidgetKey =
        Widgets.register "Value" (fun widget ->
            let name = Widgets.getScalarValue widget BindingNode.Name
            let leadingKeyword = Widgets.getScalarValue widget LeadingKeyword

            let name =
                match name with
                | StringOrWidget.StringExpr name ->
                    let name =
                        name |> StringParsing.normalizeIdentifierBackticks |> SingleTextNode.Create

                    Pattern.Named(PatNamedNode(None, name, Range.Zero))
                | StringOrWidget.WidgetExpr pattern -> pattern

            let bodyExpr = Widgets.getScalarValue widget BindingNode.BodyExpr

            let accessControl =
                Widgets.tryGetScalarValue widget BindingNode.Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let lines = Widgets.tryGetScalarValue widget BindingNode.XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            let attributes = Widgets.tryGetScalarValue widget BindingNode.MultipleAttributes

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

            let isMutable =
                Widgets.tryGetScalarValue widget BindingNode.IsMutable
                |> ValueOption.defaultValue false

            let isInlined =
                Widgets.tryGetScalarValue widget BindingNode.IsInlined
                |> ValueOption.defaultValue false

            let returnType = Widgets.tryGetScalarValue widget BindingNode.Return

            let returnType =
                match returnType with
                | ValueNone -> None
                | ValueSome value ->
                    match value with
                    | StringOrWidget.StringExpr value ->
                        let returnType =
                            Type.LongIdent(
                                IdentListNode(
                                    [ IdentifierOrDot.Ident(SingleTextNode.Create(StringVariant.normalize value)) ],
                                    Range.Zero
                                )
                            )

                        Some(BindingReturnInfoNode(SingleTextNode.colon, returnType, Range.Zero))
                    | StringOrWidget.WidgetExpr returnType ->
                        Some(BindingReturnInfoNode(SingleTextNode.colon, returnType, Range.Zero))

            let typeParams = Widgets.tryGetScalarValue widget BindingNode.TypeParams

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

            let bodyExpr =
                match bodyExpr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value)))
                    )
                | StringOrWidget.WidgetExpr value -> value

            BindingNode(
                xmlDocs,
                multipleAttributes,
                MultipleTextsNode([ leadingKeyword ], Range.Zero),
                isMutable,
                (if isInlined then Some(SingleTextNode.``inline``) else None),
                accessControl,
                Choice2Of2(name),
                typeParams,
                [],
                returnType,
                SingleTextNode.equals,
                bodyExpr,
                Range.Zero
            ))

[<AutoOpen>]
module BindingValueBuilders =
    type Ast with
        static member private BaseValue
            (name: StringOrWidget<Pattern>, bodyExpr: StringOrWidget<Expr>, leadingKeyword: SingleTextNode)
            =
            WidgetBuilder<BindingNode>(
                BindingValue.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        BindingNode.Name.WithValue(name),
                        BindingNode.BodyExpr.WithValue(bodyExpr),
                        BindingValue.LeadingKeyword.WithValue(leadingKeyword)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member Value(name: WidgetBuilder<Pattern>, value: WidgetBuilder<Expr>) =
            Ast.BaseValue(
                StringOrWidget.WidgetExpr(Gen.mkOak name),
                StringOrWidget.WidgetExpr(Gen.mkOak(value)),
                SingleTextNode.``let``
            )

        static member Value(name: string, value: WidgetBuilder<Expr>) =
            Ast.BaseValue(
                StringOrWidget.StringExpr(Unquoted(name)),
                StringOrWidget.WidgetExpr(Gen.mkOak(value)),
                SingleTextNode.``let``
            )

        static member Value(name: WidgetBuilder<Pattern>, value: StringVariant) =
            Ast.BaseValue(
                StringOrWidget.WidgetExpr(Gen.mkOak name),
                StringOrWidget.StringExpr value,
                SingleTextNode.``let``
            )

        static member Value(name: string, value: StringVariant) =
            Ast.BaseValue(
                StringOrWidget.StringExpr(Unquoted(name)),
                StringOrWidget.StringExpr value,
                SingleTextNode.``let``
            )

        static member Use(name: WidgetBuilder<Pattern>, value: WidgetBuilder<Expr>) =
            Ast.BaseValue(
                StringOrWidget.WidgetExpr(Gen.mkOak name),
                StringOrWidget.WidgetExpr(Gen.mkOak(value)),
                SingleTextNode.``use``
            )

        static member Use(name: string, value: WidgetBuilder<Expr>) =
            Ast.BaseValue(
                StringOrWidget.StringExpr(Unquoted(name)),
                StringOrWidget.WidgetExpr(Gen.mkOak(value)),
                SingleTextNode.``use``
            )

        static member Use(name: WidgetBuilder<Pattern>, value: StringVariant) =
            Ast.BaseValue(
                StringOrWidget.WidgetExpr(Gen.mkOak name),
                StringOrWidget.StringExpr value,
                SingleTextNode.``use``
            )

        static member Use(name: string, value: StringVariant) =
            Ast.BaseValue(
                StringOrWidget.StringExpr(Unquoted(name)),
                StringOrWidget.StringExpr value,
                SingleTextNode.``use``
            )
