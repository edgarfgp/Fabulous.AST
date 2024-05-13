namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module BindingValue =

    let Name = Attributes.defineScalar<Pattern> "Name"

    let LeadingKeyword = Attributes.defineScalar<SingleTextNode> "LeadingKeyword"

    let WidgetKey =
        Widgets.register "Value" (fun widget ->
            let name = Widgets.getScalarValue widget Name
            let leadingKeyword = Widgets.getScalarValue widget LeadingKeyword
            let bodyExpr = Widgets.getNodeFromWidget widget TopLevelBinding.BodyExpr

            let accessControl =
                Widgets.tryGetScalarValue widget TopLevelBinding.Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let lines = Widgets.tryGetScalarValue widget TopLevelBinding.XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            let attributes = Widgets.tryGetScalarValue widget TopLevelBinding.MultipleAttributes

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
                Widgets.tryGetScalarValue widget TopLevelBinding.IsMutable
                |> ValueOption.defaultValue false

            let isInlined =
                Widgets.tryGetScalarValue widget TopLevelBinding.IsInlined
                |> ValueOption.defaultValue false

            let returnType = Widgets.tryGetNodeFromWidget widget TopLevelBinding.Return

            let returnType =
                match returnType with
                | ValueNone -> None
                | ValueSome value -> Some(BindingReturnInfoNode(SingleTextNode.colon, value, Range.Zero))

            let typeParams =
                Widgets.tryGetNodeFromWidget widget TopLevelBinding.TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

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
            (name: WidgetBuilder<Pattern>, bodyExpr: WidgetBuilder<Expr>, leadingKeyword: SingleTextNode)
            =
            WidgetBuilder<BindingNode>(
                BindingValue.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        BindingValue.Name.WithValue(Gen.mkOak name),
                        BindingValue.LeadingKeyword.WithValue(leadingKeyword)
                    ),
                    [| TopLevelBinding.BodyExpr.WithValue(bodyExpr.Compile()) |],
                    Array.empty
                )
            )

        static member Value(name: WidgetBuilder<Pattern>, value: WidgetBuilder<Expr>) =
            Ast.BaseValue(name, value, SingleTextNode.``let``)

        static member Value(name: WidgetBuilder<Pattern>, value: WidgetBuilder<Constant>) =
            Ast.Value(name, Ast.ConstantExpr(value))

        static member Value(name: WidgetBuilder<Pattern>, value: string) =
            Ast.Value(name, Ast.ConstantExpr(Ast.Constant(value)))

        static member Value(name: WidgetBuilder<Constant>, value: WidgetBuilder<Constant>) =
            Ast.Value(Ast.ConstantPat(name), Ast.ConstantExpr(value))

        static member Value(name: string, value: WidgetBuilder<Expr>) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name
            Ast.Value(Ast.ConstantPat(Ast.Constant(name)), value)

        static member Value(name: string, value: WidgetBuilder<Constant>) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name
            Ast.Value(Ast.ConstantPat(Ast.Constant(name)), Ast.ConstantExpr(value))

        static member Value(name: string, value: string) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name
            Ast.Value(Ast.ConstantPat(Ast.Constant(name)), Ast.ConstantExpr(Ast.Constant(value)))

        static member Use(name: WidgetBuilder<Pattern>, value: WidgetBuilder<Expr>) =
            Ast.BaseValue(name, value, SingleTextNode.``use``)

        static member Use(name: WidgetBuilder<Pattern>, value: WidgetBuilder<Constant>) =
            Ast.BaseValue(name, Ast.ConstantExpr(value), SingleTextNode.``use``)

        static member Use(name: WidgetBuilder<Pattern>, value: string) = Ast.Use(name, Ast.Constant(value))

        static member Use(name: string, value: WidgetBuilder<Expr>) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name
            Ast.Use(Ast.ConstantPat(name), value)

        static member Use(name: string, value: WidgetBuilder<Constant>) = Ast.Use(name, Ast.ConstantExpr(value))

        static member Use(name: string, value: string) = Ast.Use(name, Ast.Constant(value))
