namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module PropertyGetSetBinding =
    let ReturnType = Attributes.defineWidget "Type"
    let IsSetter = Attributes.defineScalar<bool> "IsSetter"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"
    let BodyExpr = Attributes.defineWidget "BodyExpr"
    let IsInlined = Attributes.defineScalar<bool> "IsInlined"
    let Parameters = Attributes.defineScalar<Pattern list> "Parameters"
    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let WidgetKey =
        Widgets.register "PropertyGetSetBinding" (fun widget ->
            let leadingKeyword =
                if Widgets.getScalarValue widget IsSetter then
                    SingleTextNode.``set``
                else
                    SingleTextNode.``get``

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let inlined =
                Widgets.tryGetScalarValue widget IsInlined
                |> ValueOption.map(fun x -> if x then Some SingleTextNode.``inline`` else None)
                |> ValueOption.defaultValue None

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let bodyExpr = Widgets.getNodeFromWidget widget BodyExpr

            let returnType = Widgets.tryGetNodeFromWidget widget ReturnType

            let returnType =
                match returnType with
                | ValueNone -> None
                | ValueSome value -> Some(BindingReturnInfoNode(SingleTextNode.colon, value, Range.Zero))

            let pattern =
                Pattern.Unit(UnitNode(SingleTextNode.leftParenthesis, SingleTextNode.rightParenthesis, Range.Zero))

            let parameters =
                Widgets.tryGetScalarValue widget Parameters
                |> ValueOption.defaultValue([ pattern ])

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            PropertyGetSetBindingNode(
                inlined,
                attributes,
                accessControl,
                leadingKeyword,
                parameters,
                returnType,
                SingleTextNode.equals,
                bodyExpr,
                Range.Zero
            ))

[<AutoOpen>]
module PropertyGetSetBindingBuilders =
    type Ast with

        static member Getter(expr: WidgetBuilder<Expr>) =
            WidgetBuilder<PropertyGetSetBindingNode>(
                PropertyGetSetBinding.WidgetKey,
                AttributesBundle(
                    StackList.one(PropertyGetSetBinding.IsSetter.WithValue(false)),
                    [| PropertyGetSetBinding.BodyExpr.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member Getter(expr: WidgetBuilder<Constant>) = Ast.Getter(expr |> Ast.ConstantExpr)

        static member Getter(expr: string) = Ast.Getter(Ast.Constant(expr))

        static member Getter(parameters: WidgetBuilder<Pattern> list, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<PropertyGetSetBindingNode>(
                PropertyGetSetBinding.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        PropertyGetSetBinding.Parameters.WithValue(parameters |> List.map(Gen.mkOak)),
                        PropertyGetSetBinding.IsSetter.WithValue(false)
                    ),
                    [| PropertyGetSetBinding.BodyExpr.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member Getter(parameters: WidgetBuilder<Constant> list, expr: WidgetBuilder<Expr>) =
            let parameters = parameters |> List.map Ast.ConstantPat
            Ast.Getter(parameters, expr)

        static member Getter(parameters: string list, expr: WidgetBuilder<Expr>) =
            Ast.Getter(parameters |> List.map Ast.Constant, expr)

        static member Getter(parameters: WidgetBuilder<Pattern> list, expr: WidgetBuilder<Constant>) =
            Ast.Getter(parameters, Ast.ConstantExpr(expr))

        static member Getter(parameters: WidgetBuilder<Constant> list, expr: WidgetBuilder<Constant>) =
            let parameters = parameters |> List.map Ast.ConstantPat
            Ast.Getter(parameters, expr)

        static member Getter(parameters: string list, expr: WidgetBuilder<Constant>) =
            Ast.Getter(parameters |> List.map Ast.Constant, expr)

        static member Getter(parameters: string list, expr: string) =
            Ast.Getter(parameters, Ast.Constant(expr))

        static member Getter(parameter: WidgetBuilder<Pattern>, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<PropertyGetSetBindingNode>(
                PropertyGetSetBinding.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        PropertyGetSetBinding.Parameters.WithValue([ parameter ] |> List.map(Gen.mkOak)),
                        PropertyGetSetBinding.IsSetter.WithValue(false)
                    ),
                    [| PropertyGetSetBinding.BodyExpr.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member Getter(parameter: WidgetBuilder<Constant>, expr: WidgetBuilder<Expr>) =
            Ast.Getter(Ast.ConstantPat(parameter), expr)

        static member Getter(parameter: string, expr: WidgetBuilder<Expr>) =
            Ast.Getter(Ast.Constant(parameter), expr)

        static member Getter(parameter: WidgetBuilder<Pattern>, expr: WidgetBuilder<Constant>) =
            Ast.Getter(parameter, Ast.ConstantExpr(expr))

        static member Getter(parameter: WidgetBuilder<Constant>, expr: WidgetBuilder<Constant>) =
            Ast.Getter(Ast.ConstantPat(parameter), expr)

        static member Getter(parameter: string, expr: WidgetBuilder<Constant>) =
            Ast.Getter(Ast.Constant(parameter), expr)

        static member Getter(parameter: string, expr: string) =
            Ast.Getter(parameter, Ast.Constant(expr))

        static member Setter(expr: WidgetBuilder<Expr>) =
            WidgetBuilder<PropertyGetSetBindingNode>(
                PropertyGetSetBinding.WidgetKey,
                AttributesBundle(
                    StackList.one(PropertyGetSetBinding.IsSetter.WithValue(true)),
                    [| PropertyGetSetBinding.BodyExpr.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member Setter(expr: WidgetBuilder<Constant>) = Ast.Setter(expr |> Ast.ConstantExpr)

        static member Setter(expr: string) = Ast.Setter(Ast.Constant(expr))

        static member Setter(parameters: WidgetBuilder<Pattern> list, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<PropertyGetSetBindingNode>(
                PropertyGetSetBinding.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        PropertyGetSetBinding.Parameters.WithValue(parameters |> List.map(Gen.mkOak)),
                        PropertyGetSetBinding.IsSetter.WithValue(true)
                    ),
                    [| PropertyGetSetBinding.BodyExpr.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member Setter(parameters: WidgetBuilder<Constant> list, expr: WidgetBuilder<Expr>) =
            let parameters = parameters |> List.map Ast.ConstantPat
            Ast.Setter(parameters, expr)

        static member Setter(parameters: string list, expr: WidgetBuilder<Expr>) =
            Ast.Setter(parameters |> List.map Ast.Constant, expr)

        static member Setter(parameters: WidgetBuilder<Pattern> list, expr: WidgetBuilder<Constant>) =
            Ast.Setter(parameters, Ast.ConstantExpr(expr))

        static member Setter(parameters: WidgetBuilder<Constant> list, expr: WidgetBuilder<Constant>) =
            let parameters = parameters |> List.map Ast.ConstantPat
            Ast.Setter(parameters, expr)

        static member Setter(parameters: string list, expr: WidgetBuilder<Constant>) =
            Ast.Setter(parameters |> List.map Ast.Constant, expr)

        static member Setter(parameters: string list, expr: string) =
            Ast.Setter(parameters, Ast.Constant(expr))

        static member Setter(parameter: WidgetBuilder<Pattern>, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<PropertyGetSetBindingNode>(
                PropertyGetSetBinding.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        PropertyGetSetBinding.Parameters.WithValue([ parameter ] |> List.map(Gen.mkOak)),
                        PropertyGetSetBinding.IsSetter.WithValue(true)

                    ),
                    [| PropertyGetSetBinding.BodyExpr.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member Setter(parameter: WidgetBuilder<Constant>, expr: WidgetBuilder<Expr>) =
            Ast.Setter(Ast.ConstantPat(parameter), expr)

        static member Setter(parameter: string, expr: WidgetBuilder<Expr>) =
            Ast.Setter(Ast.Constant(parameter), expr)

        static member Setter(parameter: WidgetBuilder<Pattern>, expr: WidgetBuilder<Constant>) =
            Ast.Setter(parameter, Ast.ConstantExpr(expr))

        static member Setter(parameter: WidgetBuilder<Constant>, expr: WidgetBuilder<Constant>) =
            Ast.Setter(Ast.ConstantPat(parameter), expr)

        static member Setter(parameter: string, expr: WidgetBuilder<Constant>) =
            Ast.Setter(Ast.Constant(parameter), expr)

        static member Setter(parameter: string, expr: string) =
            Ast.Setter(parameter, Ast.Constant(expr))

type PropertyGetSetBindingModifiers =
    [<Extension>]
    static member inline toInlined(this: WidgetBuilder<PropertyGetSetBindingNode>) =
        this.AddScalar(PropertyGetSetBinding.IsInlined.WithValue(true))

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<PropertyGetSetBindingNode>) =
        this.AddScalar(PropertyGetSetBinding.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<PropertyGetSetBindingNode>) =
        this.AddScalar(PropertyGetSetBinding.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<PropertyGetSetBindingNode>) =
        this.AddScalar(PropertyGetSetBinding.Accessibility.WithValue(AccessControl.Internal))

    [<Extension>]
    static member inline returnType(this: WidgetBuilder<PropertyGetSetBindingNode>, value: WidgetBuilder<Type>) =
        this.AddWidget(PropertyGetSetBinding.ReturnType.WithValue(value.Compile()))

    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<PropertyGetSetBindingNode>, values: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            PropertyGetSetBinding.MultipleAttributes.WithValue(
                [ for vals in values do
                      Gen.mkOak vals ]
            )
        )

    [<Extension>]
    static member inline attribute
        (this: WidgetBuilder<PropertyGetSetBindingNode>, attribute: WidgetBuilder<AttributeNode>)
        =
        PropertyGetSetBindingModifiers.attributes(this, [ attribute ])

