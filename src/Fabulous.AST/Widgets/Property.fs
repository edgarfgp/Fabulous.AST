namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module BindingProperty =
    let Name = Attributes.defineScalar<Pattern> "Name"

    let WidgetKey =
        Widgets.register "PropertyMember" (fun widget ->
            let name = Widgets.getScalarValue widget Name

            let bodyExpr = Widgets.getNodeFromWidget widget BindingNode.BodyExpr
            let isInlined = Widgets.tryGetScalarValue widget BindingNode.IsInlined

            let isStatic =
                Widgets.tryGetScalarValue widget BindingNode.IsStatic
                |> ValueOption.defaultValue false

            let returnType = Widgets.tryGetNodeFromWidget widget BindingNode.Return

            let returnType =
                match returnType with
                | ValueNone -> None
                | ValueSome value -> Some(BindingReturnInfoNode(SingleTextNode.colon, value, Range.Zero))

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
                Widgets.tryGetScalarValue widget BindingNode.MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let inlineNode =
                match isInlined with
                | ValueSome true -> Some(SingleTextNode.``inline``)
                | ValueSome false -> None
                | ValueNone -> None

            let typeParams =
                Widgets.tryGetNodeFromWidget widget BindingNode.TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let multipleTextsNode =
                [ if isStatic then
                      SingleTextNode.``static``
                      SingleTextNode.``member``
                  else
                      SingleTextNode.``member`` ]

            BindingNode(
                xmlDocs,
                attributes,
                MultipleTextsNode(multipleTextsNode, Range.Zero),
                false,
                inlineNode,
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
module BindingPropertyBuilders =
    type Ast with
        static member Property(name: WidgetBuilder<Pattern>, body: WidgetBuilder<Expr>) =
            WidgetBuilder<BindingNode>(
                BindingProperty.WidgetKey,
                AttributesBundle(
                    StackList.one(BindingProperty.Name.WithValue(Gen.mkOak name)),
                    [| BindingNode.BodyExpr.WithValue(body.Compile()) |],
                    Array.empty
                )
            )

        static member Property(name: WidgetBuilder<Pattern>, body: WidgetBuilder<Constant>) =
            Ast.Property(name, Ast.ConstantExpr(body))

        static member Property(name: WidgetBuilder<Pattern>, body: string) =
            Ast.Property(name, Ast.ConstantExpr(Ast.Constant(body)))

        static member Property(name: WidgetBuilder<Constant>, body: WidgetBuilder<Expr>) =
            Ast.Property(Ast.ConstantPat(name), body)

        static member Property(name: WidgetBuilder<Constant>, body: WidgetBuilder<Constant>) =
            Ast.Property(name, Ast.ConstantExpr(body))

        static member Property(name: string, body: WidgetBuilder<Expr>) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name
            Ast.Property(Ast.Constant(name), body)

        static member Property(name: WidgetBuilder<Constant>, body: string) =
            Ast.Property(name, Ast.ConstantExpr(Ast.Constant(body)))

        static member Property(name: string, body: WidgetBuilder<Constant>) =
            Ast.Property(name, Ast.ConstantExpr(body))

        static member Property(name: string, body: string) =
            Ast.Property(Ast.Constant(name), Ast.Constant(body))
