namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text


module Constant =
    let ValueString = Attributes.defineScalar<string> "Value"
    let Value = Attributes.defineWidget "Value"
    let Measure = Attributes.defineWidget "Measure"

    let WidgetFromTextKey =
        Widgets.register "ConstantFromText" (fun widget ->
            let value = Helpers.getScalarValue widget ValueString
            Constant.FromText(SingleTextNode.Create(value)))

    let WidgetMeasureKey =
        Widgets.register "ConstantMeasure" (fun widget ->
            let value = Helpers.getNodeFromWidget<Constant> widget Value
            let measure = Helpers.getNodeFromWidget<Measure> widget Measure

            Constant.Measure(
                ConstantMeasureNode(
                    value,
                    UnitOfMeasureNode(SingleTextNode.lessThan, measure, SingleTextNode.greaterThan, Range.Zero),
                    Range.Zero
                )
            ))

    let WidgetUnitKey =
        Widgets.register "ConstantUnit" (fun _ ->
            Constant.Unit(UnitNode(SingleTextNode.leftParenthesis, SingleTextNode.rightParenthesis, Range.Zero)))

[<AutoOpen>]
module ConstantBuilders =
    type Ast with
        static member Constant(value: string, ?hasQuotes: bool) =
            match hasQuotes with
            | None
            | Some true ->
                WidgetBuilder<Constant>(
                    Constant.WidgetFromTextKey,
                    AttributesBundle(
                        StackList.one(Constant.ValueString.WithValue($"\"{value}\"")),
                        ValueNone,
                        ValueNone
                    )
                )
            | _ ->
                WidgetBuilder<Constant>(
                    Constant.WidgetFromTextKey,
                    AttributesBundle(StackList.one(Constant.ValueString.WithValue(value)), ValueNone, ValueNone)
                )


        static member ConstantMeasure(constant: WidgetBuilder<Constant>, measure: WidgetBuilder<Measure>) =
            WidgetBuilder<Constant>(
                Constant.WidgetMeasureKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome
                        [| Constant.Value.WithValue(constant.Compile())
                           Constant.Measure.WithValue(measure.Compile()) |],
                    ValueNone
                )
            )

        static member ConstantUnit() =
            WidgetBuilder<Constant>(
                Constant.WidgetUnitKey,
                AttributesBundle(StackList.empty(), ValueSome [||], ValueNone)
            )
