namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module BindingFunction =
    let WidgetKey =
        Widgets.register "Function" (fun widget ->
            let name = Helpers.getScalarValue widget BindingNode.NameString
            let bodyExpr = Helpers.getNodeFromWidget<Expr> widget BindingNode.BodyExpr
            let parameters = Helpers.tryGetNodeFromWidget<Pattern> widget BindingNode.Parameters

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

            let isInlined =
                Helpers.tryGetScalarValue widget BindingNode.IsInlined
                |> ValueOption.defaultValue false

            let returnType = Helpers.tryGetScalarValue widget BindingNode.Return

            let returnType =
                match returnType with
                | ValueSome returnType -> Some(BindingReturnInfoNode(SingleTextNode.colon, returnType, Range.Zero))
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

            let parameters =
                match parameters with
                | ValueSome parameters -> [ parameters ]
                | ValueNone -> []

            BindingNode(
                xmlDocs,
                multipleAttributes,
                MultipleTextsNode([ SingleTextNode.``let`` ], Range.Zero),
                false,
                (if isInlined then Some(SingleTextNode.``inline``) else None),
                accessControl,
                Choice1Of2(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create name) ], Range.Zero)),
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
            (
                name: string,
                typeParams: string list voption,
                value: WidgetBuilder<Expr>,
                parameters: WidgetBuilder<Pattern>
            ) =
            let scalars =
                match typeParams with
                | ValueNone -> StackList.one(BindingNode.NameString.WithValue(name))
                | ValueSome typeParams ->
                    StackList.two(BindingNode.NameString.WithValue(name), BindingNode.TypeParams.WithValue(typeParams))

            WidgetBuilder<BindingNode>(
                BindingFunction.WidgetKey,
                AttributesBundle(
                    scalars,
                    ValueSome
                        [| BindingNode.BodyExpr.WithValue(value.Compile())
                           BindingNode.Parameters.WithValue(parameters.Compile()) |],
                    ValueNone
                )
            )

        static member Function(name: string, parameters: WidgetBuilder<Pattern>, value: WidgetBuilder<Expr>) =
            Ast.BaseFunction(name, ValueNone, value, parameters)

        static member Function
            (
                name: string,
                typeParams: string list,
                parameters: WidgetBuilder<Pattern>,
                value: WidgetBuilder<Expr>
            ) =
            Ast.BaseFunction(name, ValueSome typeParams, value, parameters)
