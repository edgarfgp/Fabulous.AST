namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module TypeStructTuple =
    let Items = Attributes.defineScalar<Type seq> "Items"

    let WidgetKey =
        Widgets.register "TypeStructTuple" (fun widget ->
            let values = Widgets.getScalarValue widget Items

            let values =
                values
                |> Seq.map Choice1Of2
                |> Seq.intersperse(Choice2Of2(SingleTextNode.comma))

            Type.StructTuple(
                TypeStructTupleNode(SingleTextNode.``struct``, values, SingleTextNode.rightParenthesis, Range.Zero)
            ))

[<AutoOpen>]
module TypeStructTupleBuilders =
    type Ast with
        static member StructTuple(items: WidgetBuilder<Type> seq) =
            WidgetBuilder<Type>(TypeStructTuple.WidgetKey, TypeStructTuple.Items.WithValue(items |> Seq.map Gen.mkOak))

        static member StructTuple(items: string seq) =
            let items = items |> Seq.map Ast.LongIdent
            Ast.StructTuple(items)
