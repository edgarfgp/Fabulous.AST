namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module TypeTuple =
    let Items = Attributes.defineScalar<Type seq> "Items"
    let Exponent = Attributes.defineScalar<string> "Exponent"

    let WidgetKey =
        Widgets.register "TypeTuple" (fun widget ->
            let exponent =
                Widgets.tryGetScalarValue widget Exponent
                |> ValueOption.map(SingleTextNode.Create)
                |> ValueOption.defaultValue(SingleTextNode.star)

            let items =
                Widgets.getScalarValue widget Items
                |> Seq.map Choice1Of2
                |> Seq.intersperse(Choice2Of2(exponent))

            Type.Tuple(TypeTupleNode(items, Range.Zero)))

[<AutoOpen>]
module TypeTupleBuilders =
    type Ast with
        static member Tuple(items: WidgetBuilder<Type> seq) =
            let items = items |> Seq.map Gen.mkOak

            WidgetBuilder<Type>(TypeTuple.WidgetKey, TypeTuple.Items.WithValue(items))

        static member Tuple(items: WidgetBuilder<Type> seq, exponent: string) =
            let items = items |> Seq.map Gen.mkOak

            WidgetBuilder<Type>(
                TypeTuple.WidgetKey,
                AttributesBundle(
                    StackList.two(TypeTuple.Exponent.WithValue(exponent), TypeTuple.Items.WithValue(items)),
                    Array.empty,
                    Array.empty
                )
            )

        static member Tuple(items: string seq) =
            let items = items |> Seq.map Ast.LongIdent
            Ast.Tuple(items)

        static member Tuple(items: string seq, exponent: string) =
            let items = items |> Seq.map Ast.LongIdent
            Ast.Tuple(items, exponent)
