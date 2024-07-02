namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
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

type MemberDefnCollectionBuilderExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, MemberDefn>, x: FieldNode) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.ValField(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<FieldNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        MemberDefnCollectionBuilderExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, MemberDefn>, x: BindingNode) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<BindingNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        MemberDefnCollectionBuilderExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, MemberDefn>, x: MemberDefnAbstractSlotNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.AbstractSlot(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<MemberDefnAbstractSlotNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        MemberDefnCollectionBuilderExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, MemberDefn>, x: MemberDefnInterfaceNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Interface(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<MemberDefnInterfaceNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        MemberDefnCollectionBuilderExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, MemberDefn>, x: MemberDefnAutoPropertyNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.AutoProperty(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<MemberDefnAutoPropertyNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        MemberDefnCollectionBuilderExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, MemberDefn>, x: MemberDefnPropertyGetSetNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.PropertyGetSet(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<MemberDefnPropertyGetSetNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        MemberDefnCollectionBuilderExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, MemberDefn>, x: InheritConstructor) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.ImplicitInherit(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<InheritConstructor>)
        : CollectionContent =
        let node = Gen.mkOak x
        MemberDefnCollectionBuilderExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, MemberDefn>, x: MemberDefnInheritNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Inherit(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<MemberDefnInheritNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        MemberDefnCollectionBuilderExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, MemberDefn>, x: BindingListNode) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.LetBinding(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<BindingListNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        MemberDefnCollectionBuilderExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, MemberDefn>, x: ExternBindingNode) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.ExternBinding(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<ExternBindingNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        MemberDefnCollectionBuilderExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, MemberDefn>, x: ExprSingleNode) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.DoExpr(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<ExprSingleNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        MemberDefnCollectionBuilderExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, MemberDefn>, x: MemberDefnExplicitCtorNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.ExplicitCtor(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<MemberDefnExplicitCtorNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        MemberDefnCollectionBuilderExtensions.Yield(this, node)

type ModuleDeclAttributeCollectionBuilderExtensions =

    [<Extension>]
    static member inline Yield(_: AttributeCollectionBuilder<'parent, MemberDefn>, x: MemberDefn) : CollectionContent =
        let widget = Ast.EscapeHatch(x).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: AttributeCollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<MemberDefn>)
        : CollectionContent =
        let node = Gen.mkOak x
        ModuleDeclAttributeCollectionBuilderExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield(_: AttributeCollectionBuilder<'parent, MemberDefn>, x: BindingNode) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: AttributeCollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<BindingNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ModuleDeclAttributeCollectionBuilderExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: AttributeCollectionBuilder<'parent, MemberDefn>, x: MemberDefnAbstractSlotNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.AbstractSlot(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: AttributeCollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<MemberDefnAbstractSlotNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ModuleDeclAttributeCollectionBuilderExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: AttributeCollectionBuilder<'parent, MemberDefn>, x: MemberDefnInterfaceNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Interface(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: AttributeCollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<MemberDefnInterfaceNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ModuleDeclAttributeCollectionBuilderExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: AttributeCollectionBuilder<'parent, MemberDefn>, x: MemberDefnAutoPropertyNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.AutoProperty(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: AttributeCollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<MemberDefnAutoPropertyNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ModuleDeclAttributeCollectionBuilderExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: AttributeCollectionBuilder<'parent, MemberDefn>, x: MemberDefnPropertyGetSetNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.PropertyGetSet(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: AttributeCollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<MemberDefnPropertyGetSetNode>) : CollectionContent =
        let node = Gen.mkOak x
        ModuleDeclAttributeCollectionBuilderExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: AttributeCollectionBuilder<'parent, MemberDefn>, x: InheritConstructor)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.ImplicitInherit(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: AttributeCollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<InheritConstructor>)
        : CollectionContent =
        let node = Gen.mkOak x
        ModuleDeclAttributeCollectionBuilderExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: AttributeCollectionBuilder<'parent, MemberDefn>, x: MemberDefnInheritNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Inherit(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: AttributeCollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<MemberDefnInheritNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ModuleDeclAttributeCollectionBuilderExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: AttributeCollectionBuilder<'parent, MemberDefn>, x: BindingListNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.LetBinding(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: AttributeCollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<BindingListNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ModuleDeclAttributeCollectionBuilderExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: AttributeCollectionBuilder<'parent, MemberDefn>, x: ExternBindingNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.ExternBinding(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: AttributeCollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<ExternBindingNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ModuleDeclAttributeCollectionBuilderExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: AttributeCollectionBuilder<'parent, MemberDefn>, x: ExprSingleNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.DoExpr(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: AttributeCollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<ExprSingleNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ModuleDeclAttributeCollectionBuilderExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: AttributeCollectionBuilder<'parent, MemberDefn>, x: MemberDefnExplicitCtorNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.ExplicitCtor(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: AttributeCollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<MemberDefnExplicitCtorNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ModuleDeclAttributeCollectionBuilderExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield(_: AttributeCollectionBuilder<'parent, MemberDefn>, x: FieldNode) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.ValField(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: AttributeCollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<FieldNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ModuleDeclAttributeCollectionBuilderExtensions.Yield(this, node)
