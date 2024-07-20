namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Ands =
    let Items = Attributes.defineScalar<Pattern list> "Items"

    let WidgetKey =
        Widgets.register "Ands" (fun widget ->
            let items = Widgets.getScalarValue widget Items
            Pattern.Ands(PatAndsNode(items, Range.Zero)))

[<AutoOpen>]
module AndsBuilders =
    type Ast with

        static member AndsPat(values: WidgetBuilder<Pattern> list) =
            WidgetBuilder<Pattern>(
                Ands.WidgetKey,
                AttributesBundle(
                    StackList.one(Ands.Items.WithValue(values |> List.map Gen.mkOak)),
                    Array.empty,
                    Array.empty
                )
            )

        static member AndsPat(values: WidgetBuilder<Constant> list) =
            let values = values |> List.map Ast.ConstantPat
            Ast.AndsPat(values)

        static member AndsPat(values: string list) =
            let values = values |> List.map(Ast.Constant)
            Ast.AndsPat(values)
