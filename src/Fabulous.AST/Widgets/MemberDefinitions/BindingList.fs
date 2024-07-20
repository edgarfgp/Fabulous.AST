namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module BindingList =
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
                BindingList.WidgetKey,
                AttributesBundle(
                    StackList.one(BindingList.Bindings.WithValue(bindings |> List.map Gen.mkOak)),
                    Array.empty,
                    Array.empty
                )
            )

        static member LetBinding(binding: WidgetBuilder<BindingNode>) =
            WidgetBuilder<BindingListNode>(
                BindingList.WidgetKey,
                AttributesBundle(
                    StackList.one(BindingList.Bindings.WithValue([ binding ] |> List.map Gen.mkOak)),
                    Array.empty,
                    Array.empty
                )
            )
