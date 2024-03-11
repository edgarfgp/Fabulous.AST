namespace Fabulous.AST

open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module TuplePat =
    let Parameters = Attributes.defineWidgetCollection "Parameters"

    let WidgetKey =
        Widgets.register "Tuple" (fun widget ->
            let values = Widgets.getNodesFromWidgetCollection<Pattern> widget Parameters

            let values =
                values
                |> List.map Choice1Of2
                |> List.intersperse(Choice2Of2(SingleTextNode.comma))

            Pattern.Tuple(PatTupleNode(values, Range.Zero)))

[<AutoOpen>]
module TuplePatBuilders =
    type Ast with

        static member TuplePat() =
            CollectionBuilder<Pattern, Pattern>(TuplePat.WidgetKey, TuplePat.Parameters)
