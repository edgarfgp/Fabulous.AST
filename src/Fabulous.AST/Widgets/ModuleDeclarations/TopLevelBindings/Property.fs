namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module BindingProperty =
    let WidgetKey =
        Widgets.register "PropertyMember" (fun widget ->
            let name = Widgets.getScalarValue widget BindingNode.NameString
            let bodyExpr = Widgets.getNodeFromWidget<Expr> widget BindingNode.BodyExpr
            let isInlined = Widgets.tryGetScalarValue widget BindingNode.IsInlined

            let isStatic =
                Widgets.tryGetScalarValue widget BindingNode.IsStatic
                |> ValueOption.defaultValue false

            let returnType = Widgets.tryGetNodeFromWidget<Type> widget BindingNode.Return

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

            let returnType =
                match returnType with
                | ValueSome returnType -> Some(BindingReturnInfoNode(SingleTextNode.colon, returnType, Range.Zero))
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

            let inlineNode =
                match isInlined with
                | ValueSome true -> Some(SingleTextNode.``inline``)
                | ValueSome false -> None
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
                [],
                returnType,
                SingleTextNode.equals,
                bodyExpr,
                Range.Zero
            ))

[<AutoOpen>]
module BindingPropertyBuilders =
    type Ast with
        static member Property(name: string, body: WidgetBuilder<Expr>) =
            WidgetBuilder<BindingNode>(
                BindingProperty.WidgetKey,
                AttributesBundle(
                    StackList.one(BindingNode.NameString.WithValue(name)),
                    ValueSome [| BindingNode.BodyExpr.WithValue(body.Compile()) |],
                    ValueNone
                )
            )
