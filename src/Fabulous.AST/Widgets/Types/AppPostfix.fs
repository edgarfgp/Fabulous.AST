namespace Fabulous.AST

open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module TypeAppPostfix =
    let First = Attributes.defineWidget "Type"

    let Last = Attributes.defineWidget "Type"

    let WidgetKey =
        Widgets.register "TypeAppPostfix" (fun widget ->
            let first = Widgets.getNodeFromWidget<Type> widget First
            let last = Widgets.getNodeFromWidget<Type> widget Last
            Type.AppPostfix(TypeAppPostFixNode(first, last, Range.Zero)))

[<AutoOpen>]
module TypeAppPostfixBuilders =
    type Ast with
        static member AppPostfix(first: WidgetBuilder<Type>, last: WidgetBuilder<Type>) =
            WidgetBuilder<Type>(
                TypeAppPostfix.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome
                        [| TypeAppPostfix.First.WithValue(first.Compile())
                           TypeAppPostfix.Last.WithValue(last.Compile()) |],
                    ValueNone
                )
            )
