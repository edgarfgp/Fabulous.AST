namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module InterfaceMember =
    let Type = Attributes.defineWidget "Type"

    let Members = Attributes.defineWidgetCollection "Members"

    let WidgetKey =
        Widgets.register "InterfaceMember" (fun widget ->
            let ``type`` = Helpers.getNodeFromWidget widget Type
            let members = Helpers.tryGetNodesFromWidgetCollection<MemberDefn> widget Members

            let members =
                match members with
                | Some members -> members
                | None -> []

            MemberDefnInterfaceNode(
                SingleTextNode.``interface``,
                ``type``,
                Some(SingleTextNode.``with``),
                members,
                Range.Zero
            ))

[<AutoOpen>]
module InterfaceMemberBuilders =
    type Ast with

        static member InterfaceMember(``type``: WidgetBuilder<Type>) =
            CollectionBuilder<MemberDefnInterfaceNode, MemberDefn>(
                InterfaceMember.WidgetKey,
                InterfaceMember.Members,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| InterfaceMember.Type.WithValue(``type``.Compile()) |],
                    ValueNone
                )
            )

        static member InterfaceMember(``type``: string) =
            Ast.InterfaceMember(Ast.TypeLongIdent(``type``))

[<Extension>]
type InterfaceMemberYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<MemberDefnInterfaceNode, MemberDefn>,
            x: BindingNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            this: CollectionBuilder<MemberDefnInterfaceNode, MemberDefn>,
            x: WidgetBuilder<BindingNode>
        ) : CollectionContent =
        let node = Gen.mkOak x
        InterfaceMemberYieldExtensions.Yield(this, node)
