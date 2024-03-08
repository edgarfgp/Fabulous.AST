namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module BindingValue =
    let WidgetKey =
        Widgets.register "Value" (fun widget ->
            let name = Helpers.getNodeFromWidget<Pattern> widget BindingNode.NameWidget
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

            let isMutable =
                Helpers.tryGetScalarValue widget BindingNode.IsMutable
                |> ValueOption.defaultValue false

            let isInlined =
                Helpers.tryGetScalarValue widget BindingNode.IsInlined
                |> ValueOption.defaultValue false

            let returnType = Helpers.tryGetNodeFromWidget<Type> widget BindingNode.Return

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
        static member private BaseValue
            (
                name: WidgetBuilder<Pattern>,
                typeParams: string list voption,
                value: WidgetBuilder<Expr>
            ) =
            let scalars =
                match typeParams with
                | ValueNone -> StackList.empty()
                | ValueSome typeParams -> StackList.one(BindingNode.TypeParams.WithValue(typeParams))

            WidgetBuilder<BindingNode>(
                BindingValue.WidgetKey,
                AttributesBundle(
                    scalars,
                    ValueSome
                        [| BindingNode.NameWidget.WithValue(name.Compile())
                           BindingNode.BodyExpr.WithValue(value.Compile()) |],
                    ValueNone
                )
            )

        static member Value(name: WidgetBuilder<Pattern>, value: WidgetBuilder<Expr>) =
            Ast.BaseValue(name, ValueNone, value)

        static member Value(name: string, value: WidgetBuilder<Expr>) =
            Ast.BaseValue(NamedPat(name), ValueNone, value)

        static member Value(name: WidgetBuilder<Pattern>, value: string, ?hasQuotes: bool) =
            match hasQuotes with
            | Some false -> Ast.BaseValue(name, ValueNone, Ast.ConstantExpr(value, false))
            | _ -> Ast.BaseValue(name, ValueNone, Ast.ConstantExpr(value, true))

        static member Value(name: WidgetBuilder<Pattern>, typeParams: string list, value: WidgetBuilder<Expr>) =
            Ast.BaseValue(name, ValueSome typeParams, value)

        static member Value(name: string, typeParams: string list, value: WidgetBuilder<Expr>) =
            Ast.BaseValue(NamedPat(name), ValueSome typeParams, value)

        static member Value(name: string, value: string, ?hasQuotes: bool) =
            match hasQuotes with
            | Some false -> Ast.BaseValue(NamedPat(name), ValueNone, Ast.ConstantExpr(value, false))
            | _ -> Ast.BaseValue(NamedPat(name), ValueNone, Ast.ConstantExpr(value, true))

        static member Value(name: string, typeParams: string list, value: string, ?hasQuotes: bool) =
            match hasQuotes with
            | Some false -> Ast.BaseValue(NamedPat(name), ValueSome typeParams, Ast.ConstantExpr(value, false))
            | _ -> Ast.BaseValue(NamedPat(name), ValueSome typeParams, Ast.ConstantExpr(value, true))
