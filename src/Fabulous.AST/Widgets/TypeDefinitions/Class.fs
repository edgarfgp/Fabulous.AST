namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections

module Class =
    let Name = Attributes.defineScalar<string> "Name"
    let ImplicitConstructor = Attributes.defineWidget "SimplePats"
    let Members = Attributes.defineWidgetCollection "Members"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"
    let TypeParams = Attributes.defineScalar<string list> "TypeParams"

    let IsClass = Attributes.defineScalar<bool> "IsClass"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let WidgetKey =
        Widgets.register "Class" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name
                |> Unquoted
                |> StringParsing.normalizeIdentifierBackticks

            let constructor =
                Widgets.tryGetNodeFromWidget<ImplicitConstructorNode> widget ImplicitConstructor

            let members = Widgets.tryGetNodesFromWidgetCollection<MemberDefn> widget Members
            let typeParams = Widgets.tryGetScalarValue widget TypeParams
            let isClass = Widgets.getScalarValue widget IsClass

            let typeParams =
                match typeParams with
                | ValueSome values ->
                    TyparDeclsPostfixListNode(
                        SingleTextNode.lessThan,
                        [ for v in values do
                              TyparDeclNode(None, SingleTextNode.Create v, [], Range.Zero) ],
                        [],
                        SingleTextNode.greaterThan,
                        Range.Zero
                    )
                    |> TyparDecls.PostfixList
                    |> Some
                | ValueNone -> None

            let lines = Widgets.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            let attributes = Widgets.tryGetScalarValue widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | ValueSome values ->
                    Some(
                        MultipleAttributeListNode(
                            [ AttributeListNode(
                                  SingleTextNode.leftAttribute,
                                  values,
                                  SingleTextNode.rightAttribute,
                                  Range.Zero
                              ) ],
                            Range.Zero
                        )
                    )
                | ValueNone -> None

            let members =
                match members with
                | None -> []
                | Some members -> members

            let constructor =
                match constructor with
                | ValueNone when not isClass -> None
                | ValueNone ->
                    let unitNode =
                        UnitNode(SingleTextNode.leftParenthesis, SingleTextNode.rightParenthesis, Range.Zero)

                    let constructor =
                        ImplicitConstructorNode(None, None, None, Pattern.Unit(unitNode), None, Range.Zero)

                    Some constructor

                | ValueSome constructor -> Some constructor

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            TypeDefnRegularNode(
                TypeNameNode(
                    xmlDocs,
                    multipleAttributes,
                    SingleTextNode.``type``,
                    accessControl,
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero),
                    typeParams,
                    [],
                    constructor,
                    Some(SingleTextNode.equals),
                    None,
                    Range.Zero
                ),
                members,
                Range.Zero
            ))

[<AutoOpen>]
module ClassBuilders =
    type Ast with
        static member BaseClass(name: string, constructor: WidgetBuilder<ImplicitConstructorNode> voption) =
            CollectionBuilder<TypeDefnRegularNode, MemberDefn>(
                Class.WidgetKey,
                Class.Members,
                AttributesBundle(
                    StackList.two(Class.Name.WithValue(name), Class.IsClass.WithValue(true)),
                    [| match constructor with
                       | ValueSome constructor -> Class.ImplicitConstructor.WithValue(constructor.Compile())
                       | ValueNone -> () |],
                    Array.empty
                )
            )

        static member Class(name: string) = Ast.BaseClass(name, ValueNone)

        static member Class(name: string, constructor: WidgetBuilder<ImplicitConstructorNode>) =
            Ast.BaseClass(name, ValueSome constructor)

        static member Interface(name: string) =
            CollectionBuilder<TypeDefnRegularNode, MemberDefn>(
                Class.WidgetKey,
                Class.Members,
                AttributesBundle(
                    StackList.two(Class.Name.WithValue(name), Class.IsClass.WithValue(false)),
                    Array.empty,
                    Array.empty
                )
            )

type ClassModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnRegularNode>, xmlDocs: string list) =
        this.AddScalar(Class.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline typeParams(this: WidgetBuilder<TypeDefnRegularNode>, typeParams: string list) =
        this.AddScalar(Class.TypeParams.WithValue(typeParams))

    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<TypeDefnRegularNode>, attributes: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            Class.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<TypeDefnRegularNode>, attributes: string list) =
        ClassModifiers.attributes(
            this,
            [ for attribute in attributes do
                  Ast.Attribute(attribute) ]
        )

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefnRegularNode>, attribute: WidgetBuilder<AttributeNode>) =
        ClassModifiers.attributes(this, [ attribute ])

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefnRegularNode>, attribute: string) =
        ClassModifiers.attributes(this, [ Ast.Attribute(attribute) ])

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<TypeDefnRegularNode>) =
        this.AddScalar(Class.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<TypeDefnRegularNode>) =
        this.AddScalar(Class.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<TypeDefnRegularNode>) =
        this.AddScalar(Class.Accessibility.WithValue(AccessControl.Internal))

type ClassYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefnRegularNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let typeDefn = TypeDefn.Regular(node)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)

        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<TypeDefnRegularNode, MemberDefn>, x: BindingNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<TypeDefnRegularNode, MemberDefn>, x: WidgetBuilder<BindingNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ClassYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<TypeDefnRegularNode, MemberDefn>, x: MemberDefnAbstractSlotNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.AbstractSlot(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<TypeDefnRegularNode, MemberDefn>, x: WidgetBuilder<MemberDefnAbstractSlotNode>) : CollectionContent =
        let node = Gen.mkOak x
        ClassYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<TypeDefnRegularNode, MemberDefn>, x: MemberDefnInterfaceNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Interface(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<TypeDefnRegularNode, MemberDefn>, x: WidgetBuilder<MemberDefnInterfaceNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ClassYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<TypeDefnRegularNode, MemberDefn>, x: MemberDefnAutoPropertyNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.AutoProperty(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<TypeDefnRegularNode, MemberDefn>, x: WidgetBuilder<MemberDefnAutoPropertyNode>) : CollectionContent =
        let node = Gen.mkOak x
        ClassYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<TypeDefnRegularNode, MemberDefn>, x: MemberDefnPropertyGetSetNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.PropertyGetSet(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<TypeDefnRegularNode, MemberDefn>, x: WidgetBuilder<MemberDefnPropertyGetSetNode>) : CollectionContent =
        let node = Gen.mkOak x
        ClassYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<TypeDefnRegularNode, MemberDefn>, x: InheritConstructor)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.ImplicitInherit(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<TypeDefnRegularNode, MemberDefn>, x: WidgetBuilder<InheritConstructor>)
        : CollectionContent =
        let node = Gen.mkOak x
        ClassYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<TypeDefnRegularNode, MemberDefn>, x: MemberDefnInheritNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Inherit(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<TypeDefnRegularNode, MemberDefn>, x: WidgetBuilder<MemberDefnInheritNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ClassYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<TypeDefnRegularNode, MemberDefn>, x: BindingListNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.LetBinding(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<TypeDefnRegularNode, MemberDefn>, x: WidgetBuilder<BindingListNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ClassYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<TypeDefnRegularNode, MemberDefn>, x: ExternBindingNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.ExternBinding(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<TypeDefnRegularNode, MemberDefn>, x: WidgetBuilder<ExternBindingNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ClassYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<TypeDefnRegularNode, MemberDefn>, x: ExprSingleNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.DoExpr(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<TypeDefnRegularNode, MemberDefn>, x: WidgetBuilder<ExprSingleNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ClassYieldExtensions.Yield(this, node)
