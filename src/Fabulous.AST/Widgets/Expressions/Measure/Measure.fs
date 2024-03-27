namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Measure =
    let Value = Attributes.defineScalar<string> "Value"

    let LHS = Attributes.defineWidget "LHS"

    let RHS = Attributes.defineWidget "RHS"

    let Node = Attributes.defineWidget "Node"

    let Measures = Attributes.defineScalar<Measure list> "Measures"

    let Content = Attributes.defineScalar<string list> "Content"

    let WidgetMeasureSingleKey =
        Widgets.register "MeasureSingle" (fun widget ->
            let value = Widgets.getScalarValue widget Value
            Measure.Single(SingleTextNode.Create(value)))

    let WidgetMeasureOperatorKey =
        Widgets.register "MeasureOperator" (fun widget ->
            let operator = Widgets.getScalarValue widget Value
            let lhs = Widgets.getNodeFromWidget<Measure> widget LHS
            let rhs = Widgets.getNodeFromWidget<Measure> widget RHS
            Measure.Operator(MeasureOperatorNode(lhs, SingleTextNode.Create(operator), rhs, Range.Zero)))

    let WidgetMeasureDivideKey =
        Widgets.register "MeasureDivide" (fun widget ->
            let operator = Widgets.getScalarValue widget Value
            let lhs = Widgets.tryGetNodeFromWidget<Measure> widget LHS
            let rhs = Widgets.getNodeFromWidget<Measure> widget RHS

            let lhs =
                match lhs with
                | ValueSome x -> Some x
                | ValueNone -> None

            Measure.Divide(MeasureDivideNode(lhs, SingleTextNode.Create(operator), rhs, Range.Zero)))

    let WidgetMeasurePowerKey =
        Widgets.register "MeasurePower" (fun widget ->
            let operator = Widgets.getScalarValue widget Value
            let measure = Widgets.getNodeFromWidget<Measure> widget LHS
            let node = Widgets.getNodeFromWidget<RationalConstNode> widget RHS
            Measure.Power(MeasurePowerNode(measure, SingleTextNode.Create(operator), node, Range.Zero)))

    let WidgetMeasureMultiplyKey =
        Widgets.register "MeasureMultiply" (fun widget ->
            let content = Widgets.getScalarValue widget Content

            Measure.Multiple(
                IdentListNode(
                    [ for ident in content do
                          IdentifierOrDot.Ident(SingleTextNode.Create(ident)) ],
                    Range.Zero

                )
            ))

    let WidgetSequenceKey =
        Widgets.register "Sequence" (fun widget ->
            let measures = Widgets.getScalarValue widget Measures
            Measure.Seq(MeasureSequenceNode(measures, Range.Zero)))

    let WidgetParenthesisKey =
        Widgets.register "Parenthesis" (fun widget ->
            let measure = Widgets.getNodeFromWidget<Measure> widget Node

            Measure.Paren(
                MeasureParenNode(SingleTextNode.leftParenthesis, measure, SingleTextNode.rightParenthesis, Range.Zero)
            ))

[<AutoOpen>]
module MeasureBuilders =
    type Ast with

        static member MeasureSingle(value: string) =
            WidgetBuilder<Measure>(
                Measure.WidgetMeasureSingleKey,
                AttributesBundle(StackList.one(Measure.Value.WithValue(value)), Array.empty, Array.empty)
            )

        static member MeasureOperator(operator: string, lhs: WidgetBuilder<Measure>, rhs: WidgetBuilder<Measure>) =
            WidgetBuilder<Measure>(
                Measure.WidgetMeasureOperatorKey,
                AttributesBundle(
                    StackList.one(Measure.Value.WithValue(operator)),
                    [| Measure.LHS.WithValue(lhs.Compile()); Measure.RHS.WithValue(rhs.Compile()) |],
                    Array.empty
                )
            )

        static member MeasureDivide(operator: string, lhs: WidgetBuilder<Measure>, rhs: WidgetBuilder<Measure>) =
            WidgetBuilder<Measure>(
                Measure.WidgetMeasureDivideKey,
                AttributesBundle(
                    StackList.one(Measure.Value.WithValue(operator)),
                    [| Measure.LHS.WithValue(lhs.Compile()); Measure.RHS.WithValue(rhs.Compile()) |],
                    Array.empty
                )
            )

        static member MeasurePower
            (operator: string, measure: WidgetBuilder<Measure>, node: WidgetBuilder<RationalConstNode>)
            =
            WidgetBuilder<Measure>(
                Measure.WidgetMeasurePowerKey,
                AttributesBundle(
                    StackList.one(Measure.Value.WithValue(operator)),
                    [| Measure.LHS.WithValue(measure.Compile())
                       Measure.RHS.WithValue(node.Compile()) |],
                    Array.empty
                )
            )

        static member MeasureMultiple(content: string list) =
            WidgetBuilder<Measure>(
                Measure.WidgetMeasureMultiplyKey,
                AttributesBundle(StackList.one(Measure.Content.WithValue(content)), Array.empty, Array.empty)
            )

        static member MeasureSeq(value: WidgetBuilder<Measure> list) =
            let measures = value |> List.map Gen.mkOak

            WidgetBuilder<Measure>(
                Measure.WidgetSequenceKey,
                AttributesBundle(StackList.one(Measure.Measures.WithValue(measures)), Array.empty, Array.empty)
            )

        static member MeasureParen(measure: WidgetBuilder<Measure>) =
            WidgetBuilder<Measure>(
                Measure.WidgetParenthesisKey,
                AttributesBundle(StackList.empty(), [| Measure.Node.WithValue(measure.Compile()) |], Array.empty)
            )
