namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module BindingMethodNode =
    let Name = Attributes.defineScalar<string> "Name"

    let WidgetKey =
        Widgets.register "MethodMember" (fun widget ->
            let name = Widgets.getScalarValue widget Name
            let parameters = Widgets.getScalarValue widget BindingNode.Parameters
            let bodyExpr = Widgets.getNodeFromWidget widget BindingNode.BodyExpr
            let isInlined = Widgets.tryGetScalarValue widget BindingNode.IsInlined

            let isStatic =
                Widgets.tryGetScalarValue widget BindingNode.IsStatic
                |> ValueOption.defaultValue false

            let returnType = Widgets.tryGetNodeFromWidget widget BindingNode.Return

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
                | ValueNone -> None
                | ValueSome value -> Some(BindingReturnInfoNode(SingleTextNode.colon, value, Range.Zero))

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
                Choice1Of2(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero)),
                typeParams,
                parameters,
                returnType,
                SingleTextNode.equals,
                bodyExpr,
                Range.Zero
            ))

[<AutoOpen>]
module BindingMethodBuilders =
    type Ast with

        static member Method(name: string, parameters: WidgetBuilder<Pattern> list, body: WidgetBuilder<Expr>) =
            let parameters = parameters |> List.map Gen.mkOak

            WidgetBuilder<BindingNode>(
                BindingMethodNode.WidgetKey,
                AttributesBundle(
                    StackList.two(BindingMethodNode.Name.WithValue(name), BindingNode.Parameters.WithValue(parameters)),
                    [| BindingNode.BodyExpr.WithValue(body.Compile()) |],
                    Array.empty
                )
            )

        static member Method(name: string, parameter: WidgetBuilder<Pattern>, body: WidgetBuilder<Expr>) =
            Ast.Method(name, [ parameter ], body)

        static member Method(name: string, parameters: WidgetBuilder<Pattern> list, bodyExpr: WidgetBuilder<Constant>) =
            Ast.Method(name, parameters, Ast.ConstantExpr(bodyExpr))

        static member Method(name: string, parameters: WidgetBuilder<Pattern> list, bodyExpr: string) =
            Ast.Method(name, parameters, Ast.Constant(bodyExpr))

        static member Method(name: string, parameters: string list, bodyExpr: WidgetBuilder<Constant>) =
            let parameters =
                parameters |> List.map(fun p -> Ast.ParameterPat(Ast.ConstantPat(p)))

            Ast.Method(name, parameters, bodyExpr)

        static member Method(name: string, parameters: string list, bodyExpr: string) =
            let parameters =
                parameters |> List.map(fun p -> Ast.ParameterPat(Ast.ConstantPat(p)))

            Ast.Method(name, parameters, bodyExpr)

        static member Method(name: string, parameters: WidgetBuilder<Pattern>, bodyExpr: WidgetBuilder<Constant>) =
            Ast.Method(name, [ parameters ], bodyExpr)

        static member Method(name: string, parameters: WidgetBuilder<Pattern>, bodyExpr: string) =
            Ast.Method(name, [ parameters ], bodyExpr)

        static member Method(name: string, parameters: string, bodyExpr: WidgetBuilder<Constant>) =
            Ast.Method(name, [ parameters ], bodyExpr)

        static member Method(name: string, parameters: string, bodyExpr: string) =
            Ast.Method(name, [ parameters ], bodyExpr)
