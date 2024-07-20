namespace Fabulous.AST

open Fabulous.Builders
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ArrayOrListPat =

    let OpenTextNode = Attributes.defineScalar<SingleTextNode> "OpenTextNode"

    let CloseTextNode = Attributes.defineScalar<SingleTextNode> "CloseTextNode"

    let Parameters = Attributes.defineScalar<Pattern list> "Parameters"

    let WidgetKey =
        Widgets.register "ArrayOrList" (fun widget ->
            let openTextNode = Widgets.getScalarValue widget OpenTextNode
            let values = Widgets.getScalarValue widget Parameters
            let closeTextNode = Widgets.getScalarValue widget CloseTextNode

            Pattern.ArrayOrList(PatArrayOrListNode(openTextNode, values, closeTextNode, Range.Zero)))

[<AutoOpen>]
module ArrayOrListPatBuilders =
    type Ast with

        static member ListPat(values: WidgetBuilder<Pattern> list) =
            WidgetBuilder<Pattern>(
                ArrayOrListPat.WidgetKey,
                ArrayOrListPat.Parameters.WithValue(values |> List.map Gen.mkOak),
                ArrayOrListPat.OpenTextNode.WithValue(SingleTextNode.leftBracket),
                ArrayOrListPat.CloseTextNode.WithValue(SingleTextNode.rightBracket)
            )

        static member ListPat(values: WidgetBuilder<Constant> list) =
            Ast.ListPat(values |> List.map Ast.ConstantPat)

        static member ListPat(values: string list) =
            Ast.ListPat(values |> List.map Ast.ConstantPat)

        static member ArrayPat(values: WidgetBuilder<Pattern> list) =
            WidgetBuilder<Pattern>(
                ArrayOrListPat.WidgetKey,
                ArrayOrListPat.Parameters.WithValue(values |> List.map Gen.mkOak),
                ArrayOrListPat.OpenTextNode.WithValue(SingleTextNode.leftArray),
                ArrayOrListPat.CloseTextNode.WithValue(SingleTextNode.rightArray)
            )

        static member ArrayPat(values: WidgetBuilder<Constant> list) =
            Ast.ArrayPat(values |> List.map Ast.ConstantPat)

        static member ArrayPat(values: string list) =
            Ast.ArrayPat(values |> List.map Ast.ConstantPat)
