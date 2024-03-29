namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module BindingFunction =
    let WidgetKey =
        Widgets.register "Function" (fun widget ->
            let name = Widgets.getScalarValue widget BindingNode.Name

            let name =
                match name with
                | StringOrWidget.StringExpr name ->
                    name |> StringParsing.normalizeIdentifierBackticks |> SingleTextNode.Create
                | StringOrWidget.WidgetExpr _ -> failwith "Unexpected widget"

            let bodyExpr = Widgets.getScalarValue widget BindingNode.BodyExpr
            let parameters = Widgets.getScalarValue widget BindingNode.Parameters

            let bodyExpr =
                match bodyExpr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value)))
                    )
                | StringOrWidget.WidgetExpr value -> value

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
                        let value = StringParsing.normalizeIdentifierBackticks value

                        let returnType =
                            Type.LongIdent(
                                IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(value)) ], Range.Zero)
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

            BindingNode(
                xmlDocs,
                multipleAttributes,
                MultipleTextsNode([ SingleTextNode.``let`` ], Range.Zero),
                false,
                (if isInlined then Some(SingleTextNode.``inline``) else None),
                accessControl,
                Choice1Of2(IdentListNode([ IdentifierOrDot.Ident(name) ], Range.Zero)),
                typeParams,
                parameters,
                returnType,
                SingleTextNode.equals,
                bodyExpr,
                Range.Zero
            ))

[<AutoOpen>]
module BindingFunctionBuilders =
    type Ast with
        static member private BaseFunction
            (name: string, bodyExpr: StringOrWidget<Expr>, parameters: WidgetBuilder<Pattern> list)
            =
            let parameters = parameters |> List.map Gen.mkOak

            WidgetBuilder<BindingNode>(
                BindingFunction.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        BindingNode.BodyExpr.WithValue(bodyExpr),
                        BindingNode.Name.WithValue(StringOrWidget.StringExpr(Unquoted name)),
                        BindingNode.Parameters.WithValue(parameters)
                    ),
                    [||],
                    Array.empty
                )
            )

        static member Function(name: string, parameters: WidgetBuilder<Pattern> list, value: StringVariant) =
            Ast.BaseFunction(name, StringOrWidget.StringExpr(value), parameters)

        static member Function(name: string, parameters: WidgetBuilder<Pattern>, value: StringVariant) =
            Ast.BaseFunction(name, StringOrWidget.StringExpr(value), [ parameters ])

        static member Function(name: string, parameters: WidgetBuilder<Pattern>, value: WidgetBuilder<Expr>) =
            Ast.BaseFunction(name, StringOrWidget.WidgetExpr(Gen.mkOak value), [ parameters ])

        static member Function(name: string, parameters: WidgetBuilder<Pattern> list, value: WidgetBuilder<Expr>) =
            Ast.BaseFunction(name, StringOrWidget.WidgetExpr(Gen.mkOak value), parameters)
