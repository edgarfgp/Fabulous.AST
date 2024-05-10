namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module LetBindingMember =
    let Bindings = Attributes.defineScalar<BindingNode list> "Type"

    let WidgetKey =
        Widgets.register "LetBindingMember" (fun widget ->
            let bindings = Widgets.getScalarValue widget Bindings
            BindingListNode(bindings, Range.Zero))

[<AutoOpen>]
module LetBindingMemberBuilders =
    type Ast with

        static member LetBindings(bindings: WidgetBuilder<BindingNode> list) =
            WidgetBuilder<BindingListNode>(
                LetBindingMember.WidgetKey,
                AttributesBundle(
                    StackList.one(LetBindingMember.Bindings.WithValue(bindings |> List.map Gen.mkOak)),
                    Array.empty,
                    Array.empty
                )
            )

        static member LetBinding(binding: WidgetBuilder<BindingNode>) =
            WidgetBuilder<BindingListNode>(
                LetBindingMember.WidgetKey,
                AttributesBundle(
                    StackList.one(LetBindingMember.Bindings.WithValue([ binding ] |> List.map Gen.mkOak)),
                    Array.empty,
                    Array.empty
                )
            )
