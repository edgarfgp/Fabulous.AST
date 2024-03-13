namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module BindingValue =
    let WidgetKey =
        Widgets.register "Value" (fun widget ->
            let name = Widgets.getNodeFromWidget<Pattern> widget BindingNode.NameWidget
            let bodyExpr = Widgets.getScalarValue widget BindingNode.BodyExpr
            let parameters = Widgets.tryGetNodeFromWidget<Pattern> widget BindingNode.Parameters

            let hasQuotes =
                Widgets.tryGetScalarValue widget BindingNode.HasQuotes
                |> ValueOption.defaultValue true

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

            let attributes =
                Widgets.tryGetNodesFromWidgetCollection<AttributeNode> widget BindingNode.MultipleAttributes

            let multipleAttributes =
                match attributes with
                | Some values ->
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
                | None -> None

            let isMutable =
                Widgets.tryGetScalarValue widget BindingNode.IsMutable
                |> ValueOption.defaultValue false

            let isInlined =
                Widgets.tryGetScalarValue widget BindingNode.IsInlined
                |> ValueOption.defaultValue false

            let returnType = Widgets.tryGetNodeFromWidget<Type> widget BindingNode.Return

            let returnType =
                match returnType with
                | ValueSome returnType -> Some(BindingReturnInfoNode(SingleTextNode.colon, returnType, Range.Zero))
                | ValueNone -> None

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

            let parameters =
                match parameters with
                | ValueSome parameters -> [ parameters ]
                | ValueNone -> []

            let bodyExpr =
                match bodyExpr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(
                            SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value, hasQuotes))
                        )
                    )
                | StringOrWidget.WidgetExpr value -> value

            BindingNode(
                xmlDocs,
                multipleAttributes,
                MultipleTextsNode([ SingleTextNode.``let`` ], Range.Zero),
                isMutable,
                (if isInlined then Some(SingleTextNode.``inline``) else None),
                accessControl,
                Choice2Of2(name),
                typeParams,
                parameters,
                returnType,
                SingleTextNode.equals,
                bodyExpr,
                Range.Zero
            ))

[<AutoOpen>]
module BindingValueBuilders =
    type Ast with
        static member private BaseValue(name: WidgetBuilder<Pattern>, bodyExpr: StringOrWidget<Expr>) =
            WidgetBuilder<BindingNode>(
                BindingValue.WidgetKey,
                AttributesBundle(
                    StackList.one(BindingNode.BodyExpr.WithValue(bodyExpr)),
                    ValueSome [| BindingNode.NameWidget.WithValue(name.Compile()) |],
                    ValueNone
                )
            )

        static member Value(name: WidgetBuilder<Pattern>, value: WidgetBuilder<Expr>) =
            Ast.BaseValue(name, StringOrWidget.WidgetExpr(Gen.mkOak(value)))

        static member Value(name: string, value: WidgetBuilder<Expr>) =
            Ast.BaseValue(NamedPat(name), StringOrWidget.WidgetExpr(Gen.mkOak(value)))

        static member Value(name: WidgetBuilder<Pattern>, value: string) =
            Ast.BaseValue(name, StringOrWidget.StringExpr value)

        static member Value(name: string, value: string) =
            Ast.BaseValue(NamedPat(name), StringOrWidget.StringExpr value)
