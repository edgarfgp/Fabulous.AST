namespace Fabulous.AST

open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Text

module TypeArray =
    let T = Attributes.defineWidget "Identifier"
    let Rank = Attributes.defineScalar<int> "Rank"

    let WidgetKey =
        Widgets.register "TypeArray" (fun widget ->
            let t = Widgets.getNodeFromWidget<Type> widget T
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
                    ValueSome [| TypeArray.T.WithValue(value.Compile()) |],
                    ValueNone
                )
            )
