namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module InterfaceMember =
    let TypeValue = Attributes.defineScalar<StringOrWidget<Type>> "Type"

    let Members = Attributes.defineScalar<MemberDefn list> "Members"

    let WidgetKey =
        Widgets.register "InterfaceMember" (fun widget ->
            let tp = Widgets.getScalarValue widget TypeValue
            let members = Widgets.tryGetScalarValue widget Members

            let members =
                match members with
                | ValueSome members -> members
                | ValueNone -> []

            let tp =
                match tp with
                | StringOrWidget.StringExpr value ->
                    let value = StringParsing.normalizeIdentifierBackticks value
                    Type.LongIdent(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(value)) ], Range.Zero))
                | StringOrWidget.WidgetExpr widget -> widget

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
                    StackList.two(
                        InterfaceMember.TypeValue.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak value)),
                        InterfaceMember.Members.WithValue(members |> List.map Gen.mkOak)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member InterfaceMember(value: string, members: WidgetBuilder<BindingNode> list) =
            WidgetBuilder<MemberDefnInterfaceNode>(
                InterfaceMember.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        InterfaceMember.TypeValue.WithValue(StringOrWidget.StringExpr(Unquoted value)),
                        InterfaceMember.Members.WithValue(members |> List.map(fun x -> MemberDefn.Member(Gen.mkOak x)))
                    ),
                    Array.empty,
                    Array.empty
                )
            )
