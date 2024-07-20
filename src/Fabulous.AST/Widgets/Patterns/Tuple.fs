namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module TuplePat =
    let Parameters = Attributes.defineScalar<Pattern list> "Parameters"

    let WidgetKey =
        Widgets.register "Tuple" (fun widget ->
            let values = Widgets.getScalarValue widget Parameters

            let values =
                values
                |> List.map Choice1Of2
                |> List.intersperse(Choice2Of2(SingleTextNode.comma))

            Pattern.Tuple(PatTupleNode(values, Range.Zero)))

[<AutoOpen>]
module TuplePatBuilders =
    type Ast with

        static member TuplePat(value: WidgetBuilder<Pattern> list) =
            let parameters = value |> List.map Gen.mkOak

            WidgetBuilder<Pattern>(
                TuplePat.WidgetKey,
                AttributesBundle(StackList.one(TuplePat.Parameters.WithValue(parameters)), Array.empty, Array.empty)
            )

        static member TuplePat(values: WidgetBuilder<Constant> list) =
            let values = values |> List.map Ast.ConstantPat
            Ast.TuplePat(values)

        static member TuplePat(values: string list) =
            let values = values |> List.map(Ast.Constant)
            Ast.TuplePat(values)
