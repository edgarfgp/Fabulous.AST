namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module BindingMethodNode =
    let WidgetKey =
        Widgets.register "MethodMember" (fun widget ->
            let name = Helpers.getScalarValue widget BindingNode.NameString
            let parameters = Helpers.getNodeFromWidget<Pattern> widget BindingNode.Parameters

            let bodyExpr = Helpers.getNodeFromWidget<Expr> widget BindingNode.BodyExpr
            let isInlined = Helpers.tryGetScalarValue widget BindingNode.IsInlined

            let isStatic =
                Helpers.tryGetScalarValue widget BindingNode.IsStatic
                |> ValueOption.defaultValue false

            let returnType = Helpers.tryGetScalarValue widget BindingNode.Return

            let accessControl =
                Helpers.tryGetScalarValue widget BindingNode.Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let lines = Helpers.tryGetScalarValue widget BindingNode.XmlDocs

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
                Helpers.tryGetNodesFromWidgetCollection<AttributeNode> widget BindingNode.MultipleAttributes

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

            let inlineNode =
                match isInlined with
                | ValueSome true -> Some(SingleTextNode.``inline``)
                | ValueSome false -> None
                | ValueNone -> None

            let typeParams = Helpers.tryGetScalarValue widget BindingNode.TypeParams

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

            BindingNode(
                xmlDocs,
                multipleAttributes,
                MultipleTextsNode(multipleTextsNode, Range.Zero),
                false,
                inlineNode,
                accessControl,
                Choice1Of2(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode(name, Range.Zero)) ], Range.Zero)),
                typeParams,
                [ parameters ],
                returnType,
                SingleTextNode.equals,
                bodyExpr,
                Range.Zero
            ))

[<AutoOpen>]
module BindingMethodBuilders =
    type Ast with
        static member Method(name: string, parameters: WidgetBuilder<Pattern>, body: WidgetBuilder<Expr>) =
            WidgetBuilder<BindingNode>(
                BindingMethodNode.WidgetKey,
                AttributesBundle(
                    StackList.one(BindingNode.NameString.WithValue(name)),
                    ValueSome
                        [| BindingNode.Parameters.WithValue(parameters.Compile())
                           BindingNode.BodyExpr.WithValue(body.Compile()) |],
                    ValueNone
                )
            )
