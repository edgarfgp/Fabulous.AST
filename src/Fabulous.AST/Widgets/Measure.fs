namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module RationalConstNode =
    let Value = Attributes.defineScalar<string> "Value"

    let DivOp = Attributes.defineScalar<string> "DivOp"

    let Denominator = Attributes.defineScalar<string> "DivOp"

    let Node = Attributes.defineWidget "Node"

    let WidgetIntegerKey =
        Widgets.register "RationalConstNodeInteger" (fun widget ->
            let value = Widgets.getScalarValue widget Value

            RationalConstNode.Integer(SingleTextNode.Create(value)))

    let WidgetNegateKey =
        Widgets.register "RationalConstNodeNegate" (fun widget ->
            let rationalConst = Widgets.getNodeFromWidget<RationalConstNode> widget Node
            RationalConstNode.Negate(NegateRationalNode(SingleTextNode.minus, rationalConst, Range.Zero)))

    let WidgetRationalKey =
        Widgets.register "RationalConstNodeRational" (fun widget ->
            let numerator = Widgets.getScalarValue widget Value

            let divOp = Widgets.getScalarValue widget DivOp
            let denominator = Widgets.getScalarValue widget Denominator

            RationalConstNode.Rational(
                RationalNode(
                    SingleTextNode.leftParenthesis,
                    SingleTextNode.Create(numerator),
                    SingleTextNode.Create(divOp),
                    SingleTextNode.Create(denominator),
                    SingleTextNode.rightParenthesis,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module RationalConstNodeBuilders =
    type Ast with
        static member Integer(value: string) =
            WidgetBuilder<RationalConstNode>(
                RationalConstNode.WidgetIntegerKey,
                AttributesBundle(StackList.one(RationalConstNode.Value.WithValue(value)), Array.empty, Array.empty)
            )

        static member Integer(value: int) = Ast.Integer($"{value}")

        static member Negate(value: WidgetBuilder<RationalConstNode>) =
            WidgetBuilder<RationalConstNode>(
                RationalConstNode.WidgetNegateKey,
                AttributesBundle(
                    StackList.empty(),
                    [| RationalConstNode.Node.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member Rational(numerator: string, divOp: string, denominator: string) =
            WidgetBuilder<RationalConstNode>(
                RationalConstNode.WidgetRationalKey,
                AttributesBundle(
                    StackList.three(
                        RationalConstNode.Value.WithValue(numerator),
                        RationalConstNode.DivOp.WithValue(divOp),
                        RationalConstNode.Denominator.WithValue(denominator)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

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
            let lhs = Widgets.tryGetNodeFromWidget<Measure> widget LHS
            let rhs = Widgets.getNodeFromWidget<Measure> widget RHS

            let lhs =
                match lhs with
                | ValueSome x -> Some x
                | ValueNone -> None

            Measure.Divide(MeasureDivideNode(lhs, SingleTextNode.forwardSlash, rhs, Range.Zero)))

    let WidgetMeasurePowerKey =
        Widgets.register "MeasurePower" (fun widget ->
            let measure = Widgets.getNodeFromWidget<Measure> widget LHS
            let node = Widgets.getNodeFromWidget<RationalConstNode> widget RHS
            Measure.Power(MeasurePowerNode(measure, SingleTextNode.power, node, Range.Zero)))

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

        static member MeasureOperator(operator: string, lhs: string, rhs: string) =
            WidgetBuilder<Measure>(
                Measure.WidgetMeasureOperatorKey,
                AttributesBundle(
                    StackList.one(Measure.Value.WithValue(operator)),
                    [| Measure.LHS.WithValue(Ast.MeasureSingle(lhs).Compile())
                       Measure.RHS.WithValue(Ast.MeasureSingle(rhs).Compile()) |],
                    Array.empty
                )
            )

        static member MeasureDivide(lhs: WidgetBuilder<Measure>, rhs: WidgetBuilder<Measure>) =
            WidgetBuilder<Measure>(
                Measure.WidgetMeasureDivideKey,
                AttributesBundle(
                    StackList.empty(),
                    [| Measure.LHS.WithValue(lhs.Compile()); Measure.RHS.WithValue(rhs.Compile()) |],
                    Array.empty
                )
            )

        static member MeasureDivide(lhs: string, rhs: string) =
            WidgetBuilder<Measure>(
                Measure.WidgetMeasureDivideKey,
                AttributesBundle(
                    StackList.empty(),
                    [| Measure.LHS.WithValue(Ast.MeasureSingle(lhs).Compile())
                       Measure.RHS.WithValue(Ast.MeasureSingle(rhs).Compile()) |],
                    Array.empty
                )
            )

        static member MeasurePower(measure: WidgetBuilder<Measure>, node: WidgetBuilder<RationalConstNode>) =
            WidgetBuilder<Measure>(
                Measure.WidgetMeasurePowerKey,
                AttributesBundle(
                    StackList.empty(),
                    [| Measure.LHS.WithValue(measure.Compile())
                       Measure.RHS.WithValue(node.Compile()) |],
                    Array.empty
                )
            )

        static member MeasurePower(measure: string, node: string) =
            WidgetBuilder<Measure>(
                Measure.WidgetMeasurePowerKey,
                AttributesBundle(
                    StackList.empty(),
                    [| Measure.LHS.WithValue(Ast.MeasureSingle(measure).Compile())
                       Measure.RHS.WithValue(Ast.MeasureSingle(node).Compile()) |],
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

        static member MeasureSeq(values: string list) =
            let measures =
                [ for value in values do
                      Gen.mkOak(Ast.MeasureSingle(value)) ]

            WidgetBuilder<Measure>(
                Measure.WidgetSequenceKey,
                AttributesBundle(StackList.one(Measure.Measures.WithValue(measures)), Array.empty, Array.empty)
            )

        static member MeasureParen(value: WidgetBuilder<Measure>) =
            WidgetBuilder<Measure>(
                Measure.WidgetParenthesisKey,
                AttributesBundle(StackList.empty(), [| Measure.Node.WithValue(value.Compile()) |], Array.empty)
            )

        static member MeasureParen(value: string) =
            WidgetBuilder<Measure>(
                Measure.WidgetParenthesisKey,
                AttributesBundle(
                    StackList.empty(),
                    [| Measure.Node.WithValue(Ast.MeasureSingle(value).Compile()) |],
                    Array.empty
                )
            )
