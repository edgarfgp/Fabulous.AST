namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fantomas.Core

module MemberDefn =
    let MemberDefn = Attributes.defineWidget "MemberDefn"

    let WidgetKey =
        Widgets.register "MemberDefn" (fun widget ->
            let modeDecl = Widgets.getNodeFromWidget<NodeBase> widget MemberDefn

            match modeDecl with
            | :? MemberDefnInterfaceNode as x -> SyntaxOak.MemberDefn.Interface(x)
            | :? MemberDefnInheritNode as x -> SyntaxOak.MemberDefn.Inherit(x)
            | :? FieldNode as x -> SyntaxOak.MemberDefn.ValField(x)
            | :? BindingNode as x -> SyntaxOak.MemberDefn.Member(x)
            | :? ExternBindingNode as x -> SyntaxOak.MemberDefn.ExternBinding(x)
            | :? ExprSingleNode as x -> SyntaxOak.MemberDefn.DoExpr(x)
            | :? BindingListNode as x -> SyntaxOak.MemberDefn.LetBinding(x)
            | :? MemberDefnExplicitCtorNode as x -> SyntaxOak.MemberDefn.ExplicitCtor(x)
            | :? MemberDefnAutoPropertyNode as x -> SyntaxOak.MemberDefn.AutoProperty(x)
            | :? MemberDefnAbstractSlotNode as x -> SyntaxOak.MemberDefn.AbstractSlot(x)
            | :? MemberDefnSigMemberNode as x -> SyntaxOak.MemberDefn.SigMember(x)
            | :? MemberDefnPropertyGetSetNode as x -> SyntaxOak.MemberDefn.PropertyGetSet(x)
            | x -> failwith $"Unexpected node type: {x}")

[<AutoOpen>]
module MemberDefnBuilders =
    type Ast with

        /// <summary>Create a MemberDefn widget which will accept any MemberDefn node.</summary>
        /// <param name="value">The MemberDefn node to add to the widget.</param>
        static member AnyMemberDefn(value: WidgetBuilder<#NodeBase>) =
            WidgetBuilder<MemberDefn>(MemberDefn.WidgetKey, MemberDefn.MemberDefn.WithValue(value.Compile()))

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
        (this: AttributeCollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<MemberDefnPropertyGetSetNode>)
        : CollectionContent =
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
