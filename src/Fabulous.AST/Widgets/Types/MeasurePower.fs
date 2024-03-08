namespace Fabulous.AST

open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module TypeMeasurePower =
    let TypeWidget = Attributes.defineWidget "Type"

    let Rational = Attributes.defineWidget "Rational"

    let WidgetKey =
        Widgets.register "TypeMeasurePower" (fun widget ->
            let typeWidget = Helpers.getNodeFromWidget widget TypeWidget
            let rational = Helpers.getNodeFromWidget widget Rational
            Type.MeasurePower(TypeMeasurePowerNode(typeWidget, rational, Range.Zero)))

[<AutoOpen>]
module TypeMeasurePowerBuilders =
    type Ast with
        static member MeasurePower(value: WidgetBuilder<Type>, rational: WidgetBuilder<RationalConstNode>) =
            WidgetBuilder<Type>(
                TypeMeasurePower.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome
                        [| TypeMeasurePower.TypeWidget.WithValue(value.Compile())
                           TypeMeasurePower.Rational.WithValue(rational.Compile()) |],
                    ValueNone
                )
            )

        static member MeasurePower(value: string, rational: WidgetBuilder<RationalConstNode>) =
            WidgetBuilder<Type>(
                TypeMeasurePower.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome
                        [| TypeMeasurePower.TypeWidget.WithValue(Ast.LongIdent(value).Compile())
                           TypeMeasurePower.Rational.WithValue(rational.Compile()) |],
                    ValueNone
                )
            )

        static member MeasurePower(value: string list, rational: WidgetBuilder<RationalConstNode>) =
            WidgetBuilder<Type>(
                TypeMeasurePower.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome
                        [| TypeMeasurePower.TypeWidget.WithValue(Ast.LongIdent(value).Compile())
                           TypeMeasurePower.Rational.WithValue(rational.Compile()) |],
                    ValueNone
                )
            )

        static member MeasurePowerInteger(value: string, rational: string) =
            Ast.MeasurePower(Ast.LongIdent(value), Ast.Integer(rational))

        static member MeasurePowerInteger(value: string list, rational: string) =
            Ast.MeasurePower(Ast.LongIdent(value), Ast.Integer(rational))
