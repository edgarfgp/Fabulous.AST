namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Ands =
    let Items = Attributes.defineScalar<Pattern seq> "Items"

    let WidgetKey =
        Widgets.register "Ands" (fun widget ->
            let items = Widgets.getScalarValue widget Items |> List.ofSeq
            Pattern.Ands(PatAndsNode(items, Range.Zero)))

[<AutoOpen>]
module AndsBuilders =
    type Ast with

        static member AndsPat(values: WidgetBuilder<Pattern> seq) =
            WidgetBuilder<Pattern>(Ands.WidgetKey, Ands.Items.WithValue(values |> Seq.map Gen.mkOak))

        static member AndsPat(values: WidgetBuilder<Constant> seq) =
            let values = values |> Seq.map Ast.ConstantPat
            Ast.AndsPat(values)

        static member AndsPat(values: string seq) =
            let values = values |> Seq.map(Ast.Constant)
            Ast.AndsPat(values)
