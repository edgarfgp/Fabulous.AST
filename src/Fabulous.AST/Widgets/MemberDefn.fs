namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Microsoft.FSharp.Collections

module MemberDefn =
    let MemberDefn = Attributes.defineScalar<MemberDefn> "MemberDefn"

    let WidgetKey =
        Widgets.register "MemberDefn" (fun widget ->
            let modeDecl = Widgets.getScalarValue widget MemberDefn
            modeDecl)

[<AutoOpen>]
module MemberDefnBuilders =
    type Ast with

        static member private BaseAny(value: MemberDefn) =
            WidgetBuilder<MemberDefn>(
                MemberDefn.WidgetKey,
                AttributesBundle(StackList.one(MemberDefn.MemberDefn.WithValue(value)), Array.empty, Array.empty)
            )

        static member AnyMemberDefn(value: WidgetBuilder<InheritConstructor>) =
            let memberDefn = MemberDefn.ImplicitInherit(Gen.mkOak value)
            Ast.BaseAny(memberDefn)

        static member AnyMemberDefn(value: WidgetBuilder<MemberDefnInterfaceNode>) =
            let memberDefn = MemberDefn.Interface(Gen.mkOak value)
            Ast.BaseAny(memberDefn)

        static member AnyMemberDefn(value: WidgetBuilder<MemberDefnInheritNode>) =
            let memberDefn = MemberDefn.Inherit(Gen.mkOak value)
            Ast.BaseAny(memberDefn)

        static member AnyMemberDefn(value: WidgetBuilder<FieldNode>) =
            let memberDefn = MemberDefn.ValField(Gen.mkOak value)
            Ast.BaseAny(memberDefn)

        static member AnyMemberDefn(value: WidgetBuilder<BindingNode>) =
            let memberDefn = MemberDefn.Member(Gen.mkOak value)
            Ast.BaseAny(memberDefn)

        static member AnyMemberDefn(value: WidgetBuilder<ExternBindingNode>) =
            let memberDefn = MemberDefn.ExternBinding(Gen.mkOak value)
            Ast.BaseAny(memberDefn)

        static member AnyMemberDefn(value: WidgetBuilder<ExprSingleNode>) =
            let memberDefn = MemberDefn.DoExpr(Gen.mkOak value)
            Ast.BaseAny(memberDefn)

        static member AnyMemberDefn(value: WidgetBuilder<BindingListNode>) =
            let memberDefn = MemberDefn.LetBinding(Gen.mkOak value)
            Ast.BaseAny(memberDefn)

        static member AnyMemberDefn(value: WidgetBuilder<MemberDefnExplicitCtorNode>) =
            let memberDefn = MemberDefn.ExplicitCtor(Gen.mkOak value)
            Ast.BaseAny(memberDefn)

        static member AnyMemberDefn(value: WidgetBuilder<MemberDefnAutoPropertyNode>) =
            let memberDefn = MemberDefn.AutoProperty(Gen.mkOak value)
            Ast.BaseAny(memberDefn)

        static member AnyMemberDefn(value: WidgetBuilder<MemberDefnAbstractSlotNode>) =
            let memberDefn = MemberDefn.AbstractSlot(Gen.mkOak value)
            Ast.BaseAny(memberDefn)

        static member AnyMemberDefn(value: WidgetBuilder<MemberDefnSigMemberNode>) =
            let memberDefn = MemberDefn.SigMember(Gen.mkOak value)
            Ast.BaseAny(memberDefn)

        static member AnyMemberDefn(value: WidgetBuilder<MemberDefnPropertyGetSetNode>) =
            let memberDefn = MemberDefn.PropertyGetSet(Gen.mkOak value)
            Ast.BaseAny(memberDefn)
