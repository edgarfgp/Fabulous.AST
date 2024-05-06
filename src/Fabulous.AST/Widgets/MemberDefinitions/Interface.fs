namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module InterfaceMember =
    let TypeValue = Attributes.defineWidget "Type"

    let Members = Attributes.defineScalar<MemberDefn list> "Members"

    let WidgetKey =
        Widgets.register "InterfaceMember" (fun widget ->
            let tp = Widgets.getNodeFromWidget widget TypeValue
            let members = Widgets.tryGetScalarValue widget Members

            let members =
                match members with
                | ValueSome members -> members
                | ValueNone -> []

            MemberDefnInterfaceNode(
                SingleTextNode.``interface``,
                tp,
                Some(SingleTextNode.``with``),
                members,
                Range.Zero
            ))

[<AutoOpen>]
module InterfaceMemberBuilders =
    type Ast with

        static member InterfaceMember(value: WidgetBuilder<Type>, members: WidgetBuilder<MemberDefn> list) =
            WidgetBuilder<MemberDefnInterfaceNode>(
                InterfaceMember.WidgetKey,
                AttributesBundle(
                    StackList.one(InterfaceMember.Members.WithValue(members |> List.map Gen.mkOak)),
                    [| InterfaceMember.TypeValue.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member InterfaceMember(value: WidgetBuilder<Type>, members: WidgetBuilder<BindingNode> list) =
            WidgetBuilder<MemberDefnInterfaceNode>(
                InterfaceMember.WidgetKey,
                AttributesBundle(
                    StackList.one(
                        InterfaceMember.Members.WithValue(members |> List.map(fun x -> MemberDefn.Member(Gen.mkOak x)))
                    ),
                    [| InterfaceMember.TypeValue.WithValue(value.Compile()) |],
                    Array.empty
                )
            )
