namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module TypeTuple =
    let Items = Attributes.defineScalar<Type list> "Items"
    let Exponent = Attributes.defineScalar<string> "Exponent"

    let WidgetKey =
        Widgets.register "TypeTuple" (fun widget ->
            let exponent =
                Widgets.tryGetScalarValue widget Exponent
                |> ValueOption.map(SingleTextNode.Create)
                |> ValueOption.defaultValue(SingleTextNode.star)

            let items =
                Widgets.getScalarValue widget Items
                |> List.map Choice1Of2
                |> List.intersperse(Choice2Of2(exponent))

            Type.Tuple(TypeTupleNode(items, Range.Zero)))

[<AutoOpen>]
module TypeTupleBuilders =
    type Ast with
        static member Tuple(items: WidgetBuilder<Type> list) =
            let items = items |> List.map Gen.mkOak

            WidgetBuilder<Type>(TypeTuple.WidgetKey, TypeTuple.Items.WithValue(items))

        static member Tuple(items: WidgetBuilder<Type> list, exponent: string) =
            let items = items |> List.map Gen.mkOak

            WidgetBuilder<Type>(
                TypeTuple.WidgetKey,
                AttributesBundle(
                    StackList.two(TypeTuple.Exponent.WithValue(exponent), TypeTuple.Items.WithValue(items)),
                    Array.empty,
                    Array.empty
                )
            )

        static member Tuple(items: string list) =
            let items = items |> List.map Ast.LongIdent
            Ast.Tuple(items)

        static member Tuple(items: string list, exponent: string) =
            let items = items |> List.map Ast.LongIdent
            Ast.Tuple(items, exponent)
