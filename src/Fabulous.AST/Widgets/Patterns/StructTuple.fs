namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module StructTuplePat =
    let Parameters = Attributes.defineScalar<Pattern list> "Parameters"

    let WidgetKey =
        Widgets.register "StructTuple" (fun widget ->
            let values = Widgets.getScalarValue widget Parameters

            Pattern.StructTuple(PatStructTupleNode(values, Range.Zero)))

[<AutoOpen>]
module StructTuplePatBuilders =
    type Ast with

        static member StructTuplePat(values: WidgetBuilder<Pattern> list) =
            WidgetBuilder<Pattern>(
                StructTuplePat.WidgetKey,
                StructTuplePat.Parameters.WithValue(values |> List.map Gen.mkOak)
            )

        static member StructTuplePat(values: WidgetBuilder<Constant> list) =
            let values = values |> List.map Ast.ConstantPat
            Ast.StructTuplePat(values)

        static member StructTuplePat(values: string list) =
            let values = values |> List.map(Ast.Constant)
            Ast.StructTuplePat(values)
