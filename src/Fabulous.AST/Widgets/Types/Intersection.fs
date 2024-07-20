namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Intersection =
    let Values = Attributes.defineScalar<Type list> "Identifier"

    let WidgetKey =
        Widgets.register "Intersection" (fun widget ->
            let types =
                match Widgets.getScalarValue widget Values with
                | [] -> []
                | [ one ] -> [ Choice1Of2(one) ]
                | types ->
                    types
                    |> List.map(Choice1Of2)
                    |> List.intersperse(Choice2Of2(SingleTextNode.ampersand))

            Type.Intersection(TypeIntersectionNode(types, Range.Zero)))

[<AutoOpen>]
module IntersectionBuilders =
    type Ast with
        static member Intersection(values: WidgetBuilder<Type> list) =
            let values = values |> List.map Gen.mkOak

            WidgetBuilder<Type>(
                Intersection.WidgetKey,
                AttributesBundle(StackList.one(Intersection.Values.WithValue(values)), Array.empty, Array.empty)
            )

        static member Intersection(values: string list) =
            Ast.Intersection(values |> List.map Ast.LongIdent)
