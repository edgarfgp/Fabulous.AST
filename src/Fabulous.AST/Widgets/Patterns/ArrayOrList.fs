namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ArrayOrListPat =

    let OpenTextNode = Attributes.defineScalar<SingleTextNode> "OpenTextNode"

    let CloseTextNode = Attributes.defineScalar<SingleTextNode> "CloseTextNode"

    let Parameters = Attributes.defineScalar<Pattern seq> "Parameters"

    let WidgetKey =
        Widgets.register "ArrayOrList" (fun widget ->
            let openTextNode = Widgets.getScalarValue widget OpenTextNode
            let values = Widgets.getScalarValue widget Parameters |> List.ofSeq
            let closeTextNode = Widgets.getScalarValue widget CloseTextNode

            Pattern.ArrayOrList(PatArrayOrListNode(openTextNode, values, closeTextNode, Range.Zero)))

[<AutoOpen>]
module ArrayOrListPatBuilders =
    type Ast with

        static member ListPat(values: WidgetBuilder<Pattern> seq) =
            WidgetBuilder<Pattern>(
                ArrayOrListPat.WidgetKey,
                ArrayOrListPat.Parameters.WithValue(values |> Seq.map Gen.mkOak),
                ArrayOrListPat.OpenTextNode.WithValue(SingleTextNode.leftBracket),
                ArrayOrListPat.CloseTextNode.WithValue(SingleTextNode.rightBracket)
            )

        static member ListPat(values: WidgetBuilder<Constant> seq) =
            Ast.ListPat(values |> Seq.map Ast.ConstantPat)

        static member ListPat(values: string seq) =
            Ast.ListPat(values |> Seq.map Ast.ConstantPat)

        static member ArrayPat(values: WidgetBuilder<Pattern> seq) =
            WidgetBuilder<Pattern>(
                ArrayOrListPat.WidgetKey,
                ArrayOrListPat.Parameters.WithValue(values |> Seq.map Gen.mkOak),
                ArrayOrListPat.OpenTextNode.WithValue(SingleTextNode.leftArray),
                ArrayOrListPat.CloseTextNode.WithValue(SingleTextNode.rightArray)
            )

        static member ArrayPat(values: WidgetBuilder<Constant> seq) =
            Ast.ArrayPat(values |> Seq.map Ast.ConstantPat)

        static member ArrayPat(values: string seq) =
            Ast.ArrayPat(values |> Seq.map Ast.ConstantPat)
