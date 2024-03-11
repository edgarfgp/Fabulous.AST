namespace Fabulous.AST

open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Ands =
    let Items = Attributes.defineWidgetCollection "Items"

    let WidgetKey =
        Widgets.register "Ands" (fun widget ->
            let items = Widgets.getNodesFromWidgetCollection<Pattern> widget Items
            Pattern.Ands((PatAndsNode(items, Range.Zero))))

[<AutoOpen>]
module AndsBuilders =
    type Ast with

        static member AndsPat() =
            CollectionBuilder<Pattern, Pattern>(Ands.WidgetKey, Ands.Items)
