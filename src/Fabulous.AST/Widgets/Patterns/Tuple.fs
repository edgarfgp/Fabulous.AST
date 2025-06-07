namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module TuplePat =
    let Parameters = Attributes.defineScalar<Pattern seq> "Parameters"

    let WidgetKey =
        Widgets.register "Tuple" (fun widget ->
            let values = Widgets.getScalarValue widget Parameters

            let values =
                values
                |> Seq.map Choice1Of2
                |> Seq.intersperse(Choice2Of2(SingleTextNode.comma))

            Pattern.Tuple(PatTupleNode(values, Range.Zero)))

[<AutoOpen>]
module TuplePatBuilders =
    type Ast with

        static member TuplePat(value: WidgetBuilder<Pattern> seq) =
            let parameters = value |> Seq.map Gen.mkOak

            WidgetBuilder<Pattern>(TuplePat.WidgetKey, TuplePat.Parameters.WithValue(parameters))

        static member TuplePat(values: WidgetBuilder<Constant> seq) =
            let values = values |> Seq.map Ast.ConstantPat
            Ast.TuplePat(values)

        static member TuplePat(values: string seq) =
            let values = values |> Seq.map(Ast.Constant)
            Ast.TuplePat(values)
