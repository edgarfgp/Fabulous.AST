namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module TypeArray =
    let T = Attributes.defineWidget "Identifier"
    let Rank = Attributes.defineScalar<int> "Rank"

    let WidgetKey =
        Widgets.register "TypeArray" (fun widget ->
            let t = Widgets.getNodeFromWidget widget T
            let rank = Widgets.getScalarValue widget Rank
            Type.Array(TypeArrayNode(t, rank, Range.Zero)))

[<AutoOpen>]
module TypeArrayBuilders =
    type Ast with
        static member Array(value: WidgetBuilder<Type>, rank: int) =
            WidgetBuilder<Type>(
                TypeArray.WidgetKey,
                AttributesBundle(
                    StackList.one(TypeArray.Rank.WithValue(rank)),
                    [| TypeArray.T.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member Array(value: WidgetBuilder<Type>) = Ast.Array(value, 0)

        static member Array(value: string, rank: int) = Ast.Array(Ast.LongIdent value, rank)

        static member Array(value: string) = Ast.Array(value, 0)
