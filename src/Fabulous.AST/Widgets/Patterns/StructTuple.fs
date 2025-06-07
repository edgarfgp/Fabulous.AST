namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module StructTuplePat =
    let Parameters = Attributes.defineScalar<Pattern seq> "Parameters"

    let WidgetKey =
        Widgets.register "StructTuple" (fun widget ->
            let values = Widgets.getScalarValue widget Parameters |> List.ofSeq

            Pattern.StructTuple(PatStructTupleNode(values, Range.Zero)))

[<AutoOpen>]
module StructTuplePatBuilders =
    type Ast with

        static member StructTuplePat(values: WidgetBuilder<Pattern> seq) =
            WidgetBuilder<Pattern>(
                StructTuplePat.WidgetKey,
                StructTuplePat.Parameters.WithValue(values |> Seq.map Gen.mkOak)
            )

        static member StructTuplePat(values: WidgetBuilder<Constant> seq) =
            let values = values |> Seq.map Ast.ConstantPat
            Ast.StructTuplePat(values)

        static member StructTuplePat(values: string seq) =
            let values = values |> Seq.map(Ast.Constant)
            Ast.StructTuplePat(values)
