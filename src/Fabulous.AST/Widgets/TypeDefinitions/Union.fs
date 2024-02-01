namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

type UnionTypeDefnUnionNode
    (
        name: SingleTextNode,
        multipleAttributes: MultipleAttributeListNode option,
        unionCaseNode: UnionCaseNode list,
        members: MemberDefn list,
        typeParams: TyparDecls option
    ) =
    inherit
        TypeDefnUnionNode(
            TypeNameNode(
                None,
                multipleAttributes,
                SingleTextNode.``type``,
                None,
                IdentListNode([ IdentifierOrDot.Ident(name) ], Range.Zero),
                typeParams,
                [],
                None,
                Some(SingleTextNode.equals),
                None,
                Range.Zero
            ),
            None,
            unionCaseNode,
            members,
            Range.Zero
        )

module Union =

    let UnionCaseNode = Attributes.defineWidgetCollection "UnionCaseNode"

    let Name = Attributes.defineWidget "Name"

    let Members = Attributes.defineWidgetCollection "Members"

    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"

    let TypeParams = Attributes.defineScalar<string list> "TypeParams"

    let WidgetKey =
        Widgets.register "Union" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name

            let unionCaseNode =
                Helpers.getNodesFromWidgetCollection<UnionCaseNode> widget UnionCaseNode

            let members = Helpers.tryGetNodesFromWidgetCollection<MemberDefn> widget Members

            let members =
                match members with
                | Some members -> members
                | None -> []

            let attributes = Helpers.tryGetScalarValue widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | ValueSome values -> MultipleAttributeListNode.Create values |> Some
                | ValueNone -> None

            let typeParams = Helpers.tryGetScalarValue widget TypeParams

            let typeParams =
                match typeParams with
                | ValueSome values ->
                    TyparDeclsPostfixListNode(
                        SingleTextNode.lessThan,
                        [ for v in values do
                              // FIXME - Update
                              TyparDeclNode(None, SingleTextNode.Create v, [], Range.Zero) ],
                        [],
                        SingleTextNode.greaterThan,
                        Range.Zero
                    )
                    |> TyparDecls.PostfixList
                    |> Some
                | ValueNone -> None

            UnionTypeDefnUnionNode(name, multipleAttributes, unionCaseNode, members, typeParams))

[<AutoOpen>]
module UnionBuilders =
    type Ast with
        static member inline private BaseUnion(name: WidgetBuilder<#SingleTextNode>, typeParams: string list voption) =
            let scalars =
                match typeParams with
                | ValueNone -> StackList.empty()
                | ValueSome typeParams -> StackList.one(Union.TypeParams.WithValue(typeParams))

            CollectionBuilder<UnionTypeDefnUnionNode, UnionCaseNode>(
                Union.WidgetKey,
                Union.UnionCaseNode,
                AttributesBundle(scalars, ValueSome [| Union.Name.WithValue(name.Compile()) |], ValueNone)
            )

        static member inline Union(name: WidgetBuilder<#SingleTextNode>) = Ast.BaseUnion(name, ValueNone)

        static member inline Union(name: SingleTextNode) = Ast.Union(Ast.EscapeHatch(name))

        static member inline Union(name: string) =
            Ast.Union(SingleTextNode(name, Range.Zero))

        static member inline GenericUnion(name: WidgetBuilder<#SingleTextNode>, typeParams: string list) =
            Ast.BaseUnion(name, ValueSome typeParams)

        static member inline GenericUnion(name: SingleTextNode, typeParams: string list) =
            Ast.GenericUnion(Ast.EscapeHatch(name), typeParams)

        static member inline GenericUnion(name: string, typeParams: string list) =
            Ast.GenericUnion(SingleTextNode(name, Range.Zero), typeParams)

[<Extension>]
type UnionModifiers =
    [<Extension>]
    static member inline members(this: WidgetBuilder<UnionTypeDefnUnionNode>) =
        AttributeCollectionBuilder<UnionTypeDefnUnionNode, MemberDefn>(this, Union.Members)

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<UnionTypeDefnUnionNode>, attributes: string list) =
        this.AddScalar(Union.MultipleAttributes.WithValue(attributes))

[<Extension>]
type UnionYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<UnionTypeDefnUnionNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let typeDefn = TypeDefn.Union(node)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

[<Extension>]
type UnionParameterizedCaseYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<UnionTypeDefnUnionNode, UnionCaseNode>,
            x: WidgetBuilder<UnionParameterizedCaseNode>
        ) : CollectionContent =
        { Widgets = MutStackArray1.One(x.Compile()) }

    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<UnionTypeDefnUnionNode, MemberDefn>,
            x: MethodMemberNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            this: CollectionBuilder<UnionTypeDefnUnionNode, MemberDefn>,
            x: WidgetBuilder<MethodMemberNode>
        ) : CollectionContent =
        let node = Tree.compile x
        UnionParameterizedCaseYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<UnionTypeDefnUnionNode, MemberDefn>,
            x: PropertyMemberNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            this: CollectionBuilder<UnionTypeDefnUnionNode, MemberDefn>,
            x: WidgetBuilder<PropertyMemberNode>
        ) : CollectionContent =
        let node = Tree.compile x
        UnionParameterizedCaseYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (
            _: AttributeCollectionBuilder<UnionTypeDefnUnionNode, MemberDefn>,
            x: WidgetBuilder<PropertyMemberNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let widget = Ast.EscapeHatch(MemberDefn.Member(node)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            _: AttributeCollectionBuilder<UnionTypeDefnUnionNode, MemberDefn>,
            x: MethodMemberNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            this: AttributeCollectionBuilder<UnionTypeDefnUnionNode, MemberDefn>,
            x: WidgetBuilder<MethodMemberNode>
        ) : CollectionContent =
        let node = Tree.compile x
        UnionParameterizedCaseYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (
            _: AttributeCollectionBuilder<UnionTypeDefnUnionNode, MemberDefn>,
            x: InterfaceMemberNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Interface(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            this: AttributeCollectionBuilder<UnionTypeDefnUnionNode, MemberDefn>,
            x: WidgetBuilder<InterfaceMemberNode>
        ) : CollectionContent =
        let node = Tree.compile x
        UnionParameterizedCaseYieldExtensions.Yield(this, node)
