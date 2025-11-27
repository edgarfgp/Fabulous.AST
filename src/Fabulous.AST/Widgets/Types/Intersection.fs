namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Intersection =
    let Values = Attributes.defineScalar<Type seq> "Identifier"

    let WidgetKey =
        Widgets.register "Intersection" (fun widget ->
            let types =
                match List.ofSeq(Widgets.getScalarValue widget Values) with
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
        static member Intersection(values: WidgetBuilder<Type> seq) =
            let values = values |> Seq.map Gen.mkOak

            WidgetBuilder<Type>(Intersection.WidgetKey, Intersection.Values.WithValue(values))

        static member Intersection(values: string seq) =
            Ast.Intersection(values |> Seq.map Ast.LongIdent)
