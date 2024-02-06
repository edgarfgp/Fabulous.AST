namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ListCons =
    let LHSPattern = Attributes.defineWidget "LHSPattern"

    let MHSPattern = Attributes.defineScalar<SingleTextNode> "MHSPattern"

    let RHSPattern = Attributes.defineWidget "RHSPattern"

    let WidgetKey =
        Widgets.register "ListCons" (fun widget ->
            let lhs = Helpers.getNodeFromWidget widget LHSPattern
            let mhs = Helpers.getScalarValue widget MHSPattern
            let rhs = Helpers.getNodeFromWidget widget RHSPattern
            Pattern.ListCons(PatLeftMiddleRight(lhs, Choice1Of2(mhs), rhs, Range.Zero)))

[<AutoOpen>]
module ListConsBuilders =
    type Ast with

        static member ListConsPat(lhs: WidgetBuilder<Pattern>, rhs: WidgetBuilder<Pattern>) =
            WidgetBuilder<Pattern>(
                ListCons.WidgetKey,
                AttributesBundle(
                    StackList.one(ListCons.MHSPattern.WithValue(SingleTextNode.doubleColon)),
                    ValueSome
                        [| ListCons.LHSPattern.WithValue(lhs.Compile())
                           ListCons.RHSPattern.WithValue(rhs.Compile()) |],
                    ValueNone
                )
            )

        static member ListConsPat(lhs: WidgetBuilder<Pattern>, mhs: string, rhs: WidgetBuilder<Pattern>) =
            WidgetBuilder<Pattern>(
                ListCons.WidgetKey,
                AttributesBundle(
                    StackList.one(ListCons.MHSPattern.WithValue(SingleTextNode.Create(mhs))),
                    ValueSome
                        [| ListCons.LHSPattern.WithValue(lhs.Compile())
                           ListCons.RHSPattern.WithValue(rhs.Compile()) |],
                    ValueNone
                )
            )
