namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module BindingFunction =
    let Name = Attributes.defineScalar<string> "Name"

    let WidgetKey =
        Widgets.register "Function" (fun widget ->
            let name = Widgets.getScalarValue widget Name
            let bodyExpr = Widgets.getNodeFromWidget<Expr> widget TopLevelBinding.BodyExpr
            let parameters = Widgets.getScalarValue widget TopLevelBinding.Parameters

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

            let attributes =
                Widgets.tryGetScalarValue widget TopLevelBinding.MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

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
                attributes,
                MultipleTextsNode([ SingleTextNode.``let`` ], Range.Zero),
                false,
                (if isInlined then Some(SingleTextNode.``inline``) else None),
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
module BindingFunctionBuilders =
    type Ast with
        static member Function(name: string, parameters: WidgetBuilder<Pattern> list, bodyExpr: WidgetBuilder<Expr>) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name
            let parameters = parameters |> List.map Gen.mkOak

            WidgetBuilder<BindingNode>(
                BindingFunction.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        BindingFunction.Name.WithValue(name),
                        TopLevelBinding.Parameters.WithValue(parameters)
                    ),
                    [| TopLevelBinding.BodyExpr.WithValue(bodyExpr.Compile()) |],
                    Array.empty
                )
            )

        static member Function
            (name: string, parameters: WidgetBuilder<Pattern> list, bodyExpr: WidgetBuilder<Constant>)
            =
            Ast.Function(name, parameters, Ast.ConstantExpr(bodyExpr))

        static member Function(name: string, parameters: WidgetBuilder<Pattern> list, bodyExpr: string) =
            Ast.Function(name, parameters, Ast.Constant(bodyExpr))

        static member Function(name: string, parameters: string list, bodyExpr: WidgetBuilder<Expr>) =
            let parameters =
                parameters
                |> List.map(fun p -> Ast.ParameterPat(Ast.ConstantPat(Ast.Constant(p))))

            Ast.Function(name, parameters, bodyExpr)

        static member Function(name: string, parameters: string list, bodyExpr: WidgetBuilder<Constant>) =
            let parameters =
                parameters |> List.map(fun p -> Ast.ParameterPat(Ast.ConstantPat(p)))

            Ast.Function(name, parameters, bodyExpr)

        static member Function(name: string, parameters: string list, bodyExpr: string) =
            let parameters =
                parameters |> List.map(fun p -> Ast.ParameterPat(Ast.ConstantPat(p)))

            Ast.Function(name, parameters, bodyExpr)

        static member Function(name: string, parameters: WidgetBuilder<Pattern>, bodyExpr: WidgetBuilder<Expr>) =
            Ast.Function(name, [ parameters ], bodyExpr)

        static member Function(name: string, parameters: WidgetBuilder<Pattern>, bodyExpr: WidgetBuilder<Constant>) =
            Ast.Function(name, [ parameters ], bodyExpr)

        static member Function(name: string, parameters: WidgetBuilder<Pattern>, bodyExpr: string) =
            Ast.Function(name, [ parameters ], bodyExpr)

        static member Function(name: string, parameters: string, bodyExpr: WidgetBuilder<Expr>) =
            Ast.Function(name, [ parameters ], bodyExpr)

        static member Function(name: string, parameters: string, bodyExpr: WidgetBuilder<Constant>) =
            Ast.Function(name, [ parameters ], bodyExpr)

        static member Function(name: string, parameters: string, bodyExpr: string) =
            Ast.Function(name, [ parameters ], bodyExpr)
