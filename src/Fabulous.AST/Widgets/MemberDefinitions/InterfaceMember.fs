namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module InterfaceMember =
    let TypeValue = Attributes.defineWidget "Type"

    let Members = Attributes.defineWidgetCollection "Members"

    let WidgetKey =
        Widgets.register "InterfaceMember" (fun widget ->
            let tp = Widgets.getNodeFromWidget widget TypeValue

            let members =
                Widgets.tryGetNodesFromWidgetCollection<MemberDefn> widget Members
                |> ValueOption.defaultValue []

            let withNode =
                if members.IsEmpty then
                    None
                else
                    Some(SingleTextNode.``with``)

            MemberDefnInterfaceNode(SingleTextNode.``interface``, tp, withNode, members, Range.Zero))

[<AutoOpen>]
module InterfaceMemberBuilders =
    type Ast with

        static member InterfaceMember(value: WidgetBuilder<Type>) =
            CollectionBuilder<MemberDefnInterfaceNode, MemberDefn>(
                InterfaceMember.WidgetKey,
                InterfaceMember.Members,
                AttributesBundle(
                    StackList.empty(),
                    [| InterfaceMember.TypeValue.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member InterfaceMember(name: string) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name
            Ast.InterfaceMember(Ast.LongIdent name)

type InterfaceMemberYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<MemberDefnInterfaceNode, MemberDefn>, x: BindingNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x))
        { Widgets = MutStackArray1.One(widget.Compile()) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<MemberDefnInterfaceNode, MemberDefn>, x: WidgetBuilder<BindingNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        InterfaceMemberYieldExtensions.Yield(this, node)
