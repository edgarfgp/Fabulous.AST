namespace Fabulous.AST

open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module StructTuplePat =
    let Parameters = Attributes.defineWidgetCollection "Parameters"

    let WidgetKey =
        Widgets.register "StructTuple" (fun widget ->
            let values = Helpers.getNodesFromWidgetCollection<Pattern> widget Parameters

            Pattern.StructTuple(PatStructTupleNode(values, Range.Zero)))

[<AutoOpen>]
module StructTuplePatBuilders =
    type Ast with

        static member StructTuplePat() =
            CollectionBuilder<Pattern, Pattern>(StructTuplePat.WidgetKey, StructTuplePat.Parameters)
