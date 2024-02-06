namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Or =
    let LHSPattern = Attributes.defineWidget "LHSPattern"

    let MHSPattern = Attributes.defineScalar<SingleTextNode> "MHSPattern"

    let RHSPattern = Attributes.defineWidget "RHSPattern"

    let WidgetKey =
        Widgets.register "Or" (fun widget ->
            let lhs = Helpers.getNodeFromWidget widget LHSPattern
            let mhs = Helpers.getScalarValue widget MHSPattern
            let rhs = Helpers.getNodeFromWidget widget RHSPattern
            Pattern.Or(PatLeftMiddleRight(lhs, Choice1Of2(mhs), rhs, Range.Zero)))

[<AutoOpen>]
module OrBuilders =
    type Ast with

        static member OrPat(lhs: WidgetBuilder<Pattern>, rhs: WidgetBuilder<Pattern>) =
            WidgetBuilder<Pattern>(
                Or.WidgetKey,
                AttributesBundle(
                    StackList.one(Or.MHSPattern.WithValue(SingleTextNode.bar)),
                    ValueSome
                        [| Or.LHSPattern.WithValue(lhs.Compile())
                           Or.RHSPattern.WithValue(rhs.Compile()) |],
                    ValueNone
                )
            )

        static member OrPat(lhs: WidgetBuilder<Pattern>, mhs: string, rhs: WidgetBuilder<Pattern>) =
            WidgetBuilder<Pattern>(
                Or.WidgetKey,
                AttributesBundle(
                    StackList.one(Or.MHSPattern.WithValue(SingleTextNode.Create(mhs))),
                    ValueSome
                        [| Or.LHSPattern.WithValue(lhs.Compile())
                           Or.RHSPattern.WithValue(rhs.Compile()) |],
                    ValueNone
                )
            )
