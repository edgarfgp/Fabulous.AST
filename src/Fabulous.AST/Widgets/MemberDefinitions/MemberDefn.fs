namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak

module MemberDefn =
    let XmlDocs = Attributes.defineWidget "SharedXmlDocs"
    let IsMutable = Attributes.defineScalar<bool> "SharedIsMutable"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode seq> "SharedMultipleAttributes"

    let Accessibility = Attributes.defineScalar<AccessControl> "SharedAccessibility"
    let TypeParams = Attributes.defineWidget "SharedTypeParams"

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
    static member inline YieldFrom(_: CollectionBuilder<'parent, MemberDefn>, x: FieldNode seq) : CollectionContent =
        let widgets =
            x
            |> Seq.map(fun node -> Ast.EscapeHatch(MemberDefn.ValField(node)).Compile())
            |> Seq.toArray
            |> MutStackArray1.fromArray

        { Widgets = widgets }

    [<Extension>]
    static member inline YieldFrom
        (this: CollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<FieldNode> seq)
        : CollectionContent =
        let nodes = x |> Seq.map Gen.mkOak
        MemberDefnCollectionBuilderExtensions.YieldFrom(this, nodes)

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
    static member inline YieldFrom(_: CollectionBuilder<'parent, MemberDefn>, x: BindingNode seq) : CollectionContent =
        let widgets =
            x
            |> Seq.map(fun node -> Ast.EscapeHatch(MemberDefn.Member(node)).Compile())
            |> Seq.toArray
            |> MutStackArray1.fromArray

        { Widgets = widgets }

    [<Extension>]
    static member inline YieldFrom
        (this: CollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<BindingNode> seq)
        : CollectionContent =
        let nodes = x |> Seq.map Gen.mkOak
        MemberDefnCollectionBuilderExtensions.YieldFrom(this, nodes)

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
    static member inline YieldFrom
        (_: CollectionBuilder<'parent, MemberDefn>, x: InheritConstructor seq)
        : CollectionContent =
        let widgets =
            x
            |> Seq.map(fun node -> Ast.EscapeHatch(MemberDefn.ImplicitInherit(node)).Compile())
            |> Seq.toArray
            |> MutStackArray1.fromArray

        { Widgets = widgets }

    [<Extension>]
    static member inline YieldFrom
        (this: CollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<InheritConstructor> seq)
        : CollectionContent =
        let nodes = x |> Seq.map Gen.mkOak
        MemberDefnCollectionBuilderExtensions.YieldFrom(this, nodes)

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
    static member inline YieldFrom
        (_: CollectionBuilder<'parent, MemberDefn>, x: ExprSingleNode seq)
        : CollectionContent =
        let widgets =
            x
            |> Seq.map(fun node -> Ast.EscapeHatch(MemberDefn.DoExpr(node)).Compile())
            |> Seq.toArray
            |> MutStackArray1.fromArray

        { Widgets = widgets }

    [<Extension>]
    static member inline YieldFrom
        (this: CollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<ExprSingleNode> seq)
        : CollectionContent =
        let nodes = x |> Seq.map Gen.mkOak
        MemberDefnCollectionBuilderExtensions.YieldFrom(this, nodes)

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
    static member inline YieldFrom
        (_: AttributeCollectionBuilder<'parent, MemberDefn>, xs: MemberDefn seq)
        : CollectionContent =
        let widgets =
            xs
            |> Seq.map(fun md -> Ast.EscapeHatch(md).Compile())
            |> Seq.toArray
            |> MutStackArray1.fromArray

        { Widgets = widgets }

    [<Extension>]
    static member inline YieldFrom
        (this: AttributeCollectionBuilder<'parent, MemberDefn>, xs: WidgetBuilder<MemberDefn> seq)
        : CollectionContent =
        let nodes = xs |> Seq.map Gen.mkOak
        ModuleDeclAttributeCollectionBuilderExtensions.YieldFrom(this, nodes)

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

    [<Extension>]
    static member inline YieldFrom
        (_: AttributeCollectionBuilder<'parent, MemberDefn>, x: FieldNode seq)
        : CollectionContent =
        let widgets =
            x
            |> Seq.map(fun node -> Ast.EscapeHatch(MemberDefn.ValField(node)).Compile())
            |> Seq.toArray
            |> MutStackArray1.fromArray

        { Widgets = widgets }

    [<Extension>]
    static member inline YieldFrom
        (this: AttributeCollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<FieldNode> seq)
        : CollectionContent =
        let nodes = x |> Seq.map Gen.mkOak
        ModuleDeclAttributeCollectionBuilderExtensions.YieldFrom(this, nodes)

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
    static member inline YieldFrom
        (_: AttributeCollectionBuilder<'parent, MemberDefn>, x: BindingNode seq)
        : CollectionContent =
        let widgets =
            x
            |> Seq.map(fun node -> Ast.EscapeHatch(MemberDefn.Member(node)).Compile())
            |> Seq.toArray
            |> MutStackArray1.fromArray

        { Widgets = widgets }

    [<Extension>]
    static member inline YieldFrom
        (this: AttributeCollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<BindingNode> seq)
        : CollectionContent =
        let nodes = x |> Seq.map Gen.mkOak
        ModuleDeclAttributeCollectionBuilderExtensions.YieldFrom(this, nodes)

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
    static member inline YieldFrom
        (_: AttributeCollectionBuilder<'parent, MemberDefn>, x: InheritConstructor seq)
        : CollectionContent =
        let widgets =
            x
            |> Seq.map(fun node -> Ast.EscapeHatch(MemberDefn.ImplicitInherit(node)).Compile())
            |> Seq.toArray
            |> MutStackArray1.fromArray

        { Widgets = widgets }

    [<Extension>]
    static member inline YieldFrom
        (this: AttributeCollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<InheritConstructor> seq)
        : CollectionContent =
        let nodes = x |> Seq.map Gen.mkOak
        ModuleDeclAttributeCollectionBuilderExtensions.YieldFrom(this, nodes)

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
    static member inline YieldFrom
        (_: AttributeCollectionBuilder<'parent, MemberDefn>, x: ExprSingleNode seq)
        : CollectionContent =
        let widgets =
            x
            |> Seq.map(fun node -> Ast.EscapeHatch(MemberDefn.DoExpr(node)).Compile())
            |> Seq.toArray
            |> MutStackArray1.fromArray

        { Widgets = widgets }

    [<Extension>]
    static member inline YieldFrom
        (this: AttributeCollectionBuilder<'parent, MemberDefn>, x: WidgetBuilder<ExprSingleNode> seq)
        : CollectionContent =
        let nodes = x |> Seq.map Gen.mkOak
        ModuleDeclAttributeCollectionBuilderExtensions.YieldFrom(this, nodes)
