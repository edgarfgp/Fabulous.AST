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
        static member TypeMeasurePower(t: WidgetBuilder<Type>, rational: WidgetBuilder<RationalConstNode>) =
            WidgetBuilder<Type>(
                TypeMeasurePower.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome
                        [| TypeMeasurePower.TypeWidget.WithValue(t.Compile())
                           TypeMeasurePower.Rational.WithValue(rational.Compile()) |],
                    ValueNone
                )
            )
            
        static member TypeMeasurePower(t: string, rational: WidgetBuilder<RationalConstNode>) =
            WidgetBuilder<Type>(
                TypeMeasurePower.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome
                        [| TypeMeasurePower.TypeWidget.WithValue(Ast.TypeLongIdent(t).Compile())
                           TypeMeasurePower.Rational.WithValue(rational.Compile()) |],
                    ValueNone
                )
            )

        static member TypeMeasurePower(t: string, rational: string) =
            Ast.TypeMeasurePower(Ast.TypeLongIdent(t), Ast.Integer(rational))

        static member TypeMeasurePower(t: string list, rational: string) =
            Ast.TypeMeasurePower(Ast.TypeLongIdent(t), Ast.Integer(rational))
