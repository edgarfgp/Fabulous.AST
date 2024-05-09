namespace Fabulous.AST

open System
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module TypeMeasurePower =
    let TypeWidget = Attributes.defineWidget "Type"

    let Rational = Attributes.defineWidget "Rational"

    let WidgetKey =
        Widgets.register "TypeMeasurePower" (fun widget ->
            let typeWidget = Widgets.getNodeFromWidget widget TypeWidget
            let rational = Widgets.getNodeFromWidget widget Rational
            Type.MeasurePower(TypeMeasurePowerNode(typeWidget, rational, Range.Zero)))

[<AutoOpen>]
module TypeMeasurePowerBuilders =
    type Ast with
        static member MeasurePower(value: WidgetBuilder<Type>, rational: WidgetBuilder<RationalConstNode>) =
            WidgetBuilder<Type>(
                TypeMeasurePower.WidgetKey,
                AttributesBundle(
                    StackList.empty(),

                    [| TypeMeasurePower.TypeWidget.WithValue(value.Compile())
                       TypeMeasurePower.Rational.WithValue(rational.Compile()) |],
                    Array.empty
                )
            )

        static member MeasurePower(value: string, rational: WidgetBuilder<RationalConstNode>) =
            Ast.MeasurePower(Ast.LongIdent value, rational)
