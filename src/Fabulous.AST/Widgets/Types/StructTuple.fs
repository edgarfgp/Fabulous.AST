namespace Fabulous.AST

open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module TypeStructTuple =
    let Items = Attributes.defineWidgetCollection "Items"

    let WidgetKey =
        Widgets.register "TypeStructTuple" (fun widget ->
            let values = Widgets.getNodesFromWidgetCollection<Type> widget Items

            let values =
                values
                |> List.map Choice1Of2
                |> List.intersperse(Choice2Of2(SingleTextNode.comma))

            Type.StructTuple(
                TypeStructTupleNode(SingleTextNode.``struct``, values, SingleTextNode.rightParenthesis, Range.Zero)
            ))

[<AutoOpen>]
module TypeStructTupleBuilders =
    type Ast with
        static member StructTuple() =
            CollectionBuilder<Type, Type>(TypeStructTuple.WidgetKey, TypeStructTuple.Items)
