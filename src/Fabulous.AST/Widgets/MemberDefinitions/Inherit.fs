namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Inherit =
    let TypeValue = Attributes.defineWidget "Type"

    let WidgetKey =
        Widgets.register "InheritMember" (fun widget ->
            let tp = Widgets.getNodeFromWidget widget TypeValue
            MemberDefnInheritNode(SingleTextNode.``inherit``, tp, Range.Zero))

[<AutoOpen>]
module InheritMemberBuilders =
    type Ast with

        static member Inherit(value: WidgetBuilder<Type>) =
            WidgetBuilder<MemberDefnInheritNode>(
                Inherit.WidgetKey,
                AttributesBundle(StackList.empty(), [| Inherit.TypeValue.WithValue(value.Compile()) |], Array.empty)
            )

        static member Inherit(value: string) = Ast.Inherit(Ast.LongIdent(value))
