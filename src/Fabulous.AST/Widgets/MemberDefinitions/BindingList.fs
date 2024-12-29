namespace Fabulous.AST

open Fabulous.AST
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
                BindingList.Bindings.WithValue(bindings |> List.map Gen.mkOak)
            )

        static member LetBinding(binding: WidgetBuilder<BindingNode>) =
            WidgetBuilder<BindingListNode>(
                BindingList.WidgetKey,
                BindingList.Bindings.WithValue([ binding ] |> List.map Gen.mkOak)
            )
