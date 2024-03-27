namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module StructTuplePat =
    let Parameters = Attributes.defineScalar<Pattern list> "Parameters"

    let WidgetKey =
        Widgets.register "StructTuple" (fun widget ->
            let values = Widgets.getScalarValue widget Parameters

            Pattern.StructTuple(PatStructTupleNode(values, Range.Zero)))

[<AutoOpen>]
module StructTuplePatBuilders =
    type Ast with

        static member StructTuplePat(values: WidgetBuilder<Pattern> list) =
            WidgetBuilder<Pattern>(
                StructTuplePat.WidgetKey,
                AttributesBundle(
                    StackList.one(StructTuplePat.Parameters.WithValue(values |> List.map Gen.mkOak)),
                    Array.empty,
                    Array.empty
                )
            )
