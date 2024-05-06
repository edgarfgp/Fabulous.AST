namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open Microsoft.FSharp.Collections

module PropertyGetSetBinding =
    let ReturnType = Attributes.defineWidget "Type"
    let IsSetter = Attributes.defineScalar<bool> "IsSetter"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"
    let BodyExpr = Attributes.defineWidget "BodyExpr"
    let IsInlined = Attributes.defineScalar<bool> "IsInlined"
    let Parameters = Attributes.defineScalar<Pattern list> "Parameters"

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

            PropertyGetSetBindingNode(
                inlined,
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

        static member GetterBinding(expr: WidgetBuilder<Expr>) =
            WidgetBuilder<PropertyGetSetBindingNode>(
                PropertyGetSetBinding.WidgetKey,
                AttributesBundle(
                    StackList.one(PropertyGetSetBinding.IsSetter.WithValue(false)),
                    [| PropertyGetSetBinding.BodyExpr.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member GetterBinding(parameters: WidgetBuilder<Pattern> list, expr: WidgetBuilder<Expr>) =
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

        static member GetterBinding(parameter: WidgetBuilder<Pattern>, expr: WidgetBuilder<Expr>) =
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

        static member SetterBinding(expr: WidgetBuilder<Expr>) =
            WidgetBuilder<PropertyGetSetBindingNode>(
                PropertyGetSetBinding.WidgetKey,
                AttributesBundle(
                    StackList.one(PropertyGetSetBinding.IsSetter.WithValue(true)),
                    [| PropertyGetSetBinding.BodyExpr.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member SetterBinding(parameters: WidgetBuilder<Pattern> list, expr: WidgetBuilder<Expr>) =
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

        static member SetterBinding(parameter: WidgetBuilder<Pattern>, expr: WidgetBuilder<Expr>) =
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
