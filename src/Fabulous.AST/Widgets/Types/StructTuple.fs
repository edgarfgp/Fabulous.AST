namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module TypeStructTuple =
    let Items = Attributes.defineScalar<Type list> "Items"

    let WidgetKey =
        Widgets.register "TypeStructTuple" (fun widget ->
            let values = Widgets.getScalarValue widget Items

            let values =
                values
                |> List.map Choice1Of2
                |> List.intersperse(Choice2Of2(SingleTextNode.comma))

            Type.StructTuple(
                TypeStructTupleNode(SingleTextNode.``struct``, values, SingleTextNode.rightParenthesis, Range.Zero)
            ))

[<AutoOpen>]
module TypeStructTupleBuilders =
    type Ast with
        static member StructTuple(items: WidgetBuilder<Type> list) =
            WidgetBuilder<Type>(
                TypeStructTuple.WidgetKey,
                AttributesBundle(
                    StackList.one(TypeStructTuple.Items.WithValue(items |> List.map Gen.mkOak)),
                    Array.empty,
                    Array.empty
                )
            )

        static member StructTuple(items: string list) =
            let items = items |> List.map Ast.LongIdent
            Ast.StructTuple(items)
