namespace Fabulous.AST

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
            let value = Helpers.getScalarValue widget Value
            RationalConstNode.Integer(SingleTextNode.Create(value)))

    let WidgetNegateKey =
        Widgets.register "RationalConstNodeNegate" (fun widget ->
            let minus = Helpers.getScalarValue widget Value
            let rationalConst = Helpers.getNodeFromWidget<RationalConstNode> widget Node
            RationalConstNode.Negate(NegateRationalNode(SingleTextNode.Create(minus), rationalConst, Range.Zero)))

    let WidgetRationalKey =
        Widgets.register "RationalConstNodeRational" (fun widget ->
            let numerator = Helpers.getScalarValue widget Value
            let divOp = Helpers.getScalarValue widget DivOp
            let denominator = Helpers.getScalarValue widget Denominator

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

        /// <summary>
        /// Create a RationalConstNode with an integer value
        /// </summary>
        /// <param name="value">The integer value</param>
        /// <code language="fsharp">
        /// Integer("123")
        /// </code>
        static member Integer(value: string) =
            WidgetBuilder<RationalConstNode>(
                RationalConstNode.WidgetIntegerKey,
                AttributesBundle(StackList.one(RationalConstNode.Value.WithValue(value)), ValueNone, ValueNone)
            )

        /// <summary>
        /// Create a RationalConstNode with a negated value
        /// </summary>
        /// <param name="minus">The minus value</param>
        /// <param name="rationalConst">The RationalConstNode to negate</param>
        /// <code language="fsharp">
        /// Negate("-", Integer("123"))
        /// </code>
        static member Negate(minus: string, rationalConst: WidgetBuilder<RationalConstNode>) =
            WidgetBuilder<RationalConstNode>(
                RationalConstNode.WidgetNegateKey,
                AttributesBundle(
                    StackList.one(RationalConstNode.Value.WithValue(minus)),
                    ValueSome [| RationalConstNode.Node.WithValue(rationalConst.Compile()) |],
                    ValueNone
                )
            )

        /// <summary>
        /// Create a RationalConstNode with a rational value
        /// </summary>
        /// <param name="numerator">The numerator used in the rational value</param>
        /// <param name="divOp">The division operator used in the rational value</param>
        /// <param name="denominator">The denominator used in the rational value</param>
        /// <code language="fsharp">
        /// Rational("123", "/", "456")
        /// </code>
        static member Rational(numerator: string, divOp: string, denominator: string) =
            WidgetBuilder<RationalConstNode>(
                RationalConstNode.WidgetRationalKey,
                AttributesBundle(
                    StackList.three(
                        RationalConstNode.Value.WithValue(numerator),
                        RationalConstNode.DivOp.WithValue(divOp),
                        RationalConstNode.Denominator.WithValue(denominator)
                    ),
                    ValueNone,
                    ValueNone
                )
            )
