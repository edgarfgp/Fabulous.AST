namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module InheritMember =
    let TypeValue = Attributes.defineScalar<StringOrWidget<Type>> "Type"

    let WidgetKey =
        Widgets.register "InheritMember" (fun widget ->
            let tp = Widgets.getScalarValue widget TypeValue

            let tp =
                match tp with
                | StringOrWidget.StringExpr value ->
                    let value = StringParsing.normalizeIdentifierQuotes value
                    Type.LongIdent(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(value)) ], Range.Zero))
                | StringOrWidget.WidgetExpr widget -> widget

            MemberDefnInheritNode(SingleTextNode.``inherit``, tp, Range.Zero))

[<AutoOpen>]
module InheritMemberBuilders =
    type Ast with

        static member Inherit(value: WidgetBuilder<Type>) =
            WidgetBuilder<MemberDefnInheritNode>(
                InheritMember.WidgetKey,
                AttributesBundle(
                    StackList.one(InheritMember.TypeValue.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak value))),
                    Array.empty,
                    Array.empty
                )
            )

        static member Inherit(value: string) =
            WidgetBuilder<MemberDefnInheritNode>(
                InheritMember.WidgetKey,
                AttributesBundle(
                    StackList.one(InheritMember.TypeValue.WithValue(StringOrWidget.StringExpr(Unquoted value))),
                    Array.empty,
                    Array.empty
                )
            )
