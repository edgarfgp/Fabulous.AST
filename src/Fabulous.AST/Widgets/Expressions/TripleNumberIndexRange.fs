namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module TripleNumberIndexRange =
    let Start = Attributes.defineScalar<string> "Start"
    let Center = Attributes.defineScalar<string> "Center"
    let End = Attributes.defineScalar<string> "End"

    let WidgetKey =
        Widgets.register "TripleNumberIndexRange" (fun widget ->
            let start = Widgets.getScalarValue widget Start
            let center = Widgets.getScalarValue widget Center
            let ``end`` = Widgets.getScalarValue widget End

            Expr.TripleNumberIndexRange(
                ExprTripleNumberIndexRangeNode(
                    SingleTextNode.Create(start),
                    SingleTextNode.``..``,
                    SingleTextNode.Create(center),
                    SingleTextNode.``..``,
                    SingleTextNode.Create(``end``),
                    Range.Zero
                )
            ))

[<AutoOpen>]
module TripleNumberIndexRangeBuilders =
    type Ast with

        static member TripleNumberIndexRangeExpr(start: string, center: string, ``end``: string) =
            WidgetBuilder<Expr>(
                TripleNumberIndexRange.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        TripleNumberIndexRange.Start.WithValue(start),
                        TripleNumberIndexRange.Center.WithValue(center),
                        TripleNumberIndexRange.End.WithValue(``end``)
                    ),
                    Array.empty,
                    Array.empty
                )
            )
