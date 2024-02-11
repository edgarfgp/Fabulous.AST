namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module As =
    let LHSPattern = Attributes.defineWidget "LHSPattern"

    let MHSPattern = Attributes.defineScalar<SingleTextNode> "MHSPattern"

    let RHSPattern = Attributes.defineWidget "RHSPattern"

    let WidgetKey =
        Widgets.register "As" (fun widget ->
            let lhs = Helpers.getNodeFromWidget widget LHSPattern
            let mhs = Helpers.getScalarValue widget MHSPattern
            let rhs = Helpers.getNodeFromWidget widget RHSPattern
            Pattern.As(PatLeftMiddleRight(lhs, Choice1Of2(mhs), rhs, Range.Zero)))

[<AutoOpen>]
module AsBuilders =
    type Ast with

        static member AsPat(lhs: WidgetBuilder<Pattern>, rhs: WidgetBuilder<Pattern>) =
            WidgetBuilder<Pattern>(
                As.WidgetKey,
                AttributesBundle(
                    StackList.one(As.MHSPattern.WithValue(SingleTextNode.``as``)),
                    ValueSome
                        [| As.LHSPattern.WithValue(lhs.Compile())
                           As.RHSPattern.WithValue(rhs.Compile()) |],
                    ValueNone
                )
            )

        static member AsPat(lhs: WidgetBuilder<Pattern>, mhs: string, rhs: WidgetBuilder<Pattern>) =
            WidgetBuilder<Pattern>(
                As.WidgetKey,
                AttributesBundle(
                    StackList.one(As.MHSPattern.WithValue(SingleTextNode.Create(mhs))),
                    ValueSome
                        [| As.LHSPattern.WithValue(lhs.Compile())
                           As.RHSPattern.WithValue(rhs.Compile()) |],
                    ValueNone
                )
            )
