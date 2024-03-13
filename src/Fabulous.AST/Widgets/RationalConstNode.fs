namespace Fabulous.AST

open System
open Fabulous.AST.StackAllocatedCollections.StackList
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
            let minus = Widgets.getScalarValue widget Value
            let rationalConst = Widgets.getNodeFromWidget<RationalConstNode> widget Node
            RationalConstNode.Negate(NegateRationalNode(SingleTextNode.Create(minus), rationalConst, Range.Zero)))

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

        static member Negate(minus: string, rationalConst: WidgetBuilder<RationalConstNode>) =
            WidgetBuilder<RationalConstNode>(
                RationalConstNode.WidgetNegateKey,
                AttributesBundle(
                    StackList.one(RationalConstNode.Value.WithValue(minus)),
                    [| RationalConstNode.Node.WithValue(rationalConst.Compile()) |],
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
