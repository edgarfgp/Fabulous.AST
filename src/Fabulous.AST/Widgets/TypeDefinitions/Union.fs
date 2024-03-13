namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module Union =

    let UnionCaseNode = Attributes.defineWidgetCollection "UnionCaseNode"

    let Name = Attributes.defineScalar<string> "Name"

    let Members = Attributes.defineWidgetCollection "Members"

    let MultipleAttributes = Attributes.defineWidgetCollection "MultipleAttributes"

    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let TypeParams = Attributes.defineScalar<string list> "TypeParams"

    let WidgetKey =
        Widgets.register "Union" (fun widget ->
            let name = Widgets.getScalarValue widget Name

            let unionCaseNode =
                Widgets.getNodesFromWidgetCollection<UnionCaseNode> widget UnionCaseNode

            let members = Widgets.tryGetNodesFromWidgetCollection<MemberDefn> widget Members

            let members =
                match members with
                | Some members -> members
                | None -> []

            let lines = Widgets.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            let attributes =
                Widgets.tryGetNodesFromWidgetCollection<AttributeNode> widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | Some values ->
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
                | None -> None

            let typeParams = Widgets.tryGetScalarValue widget TypeParams

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

            TypeDefnUnionNode(
                TypeNameNode(
                    None,
                    multipleAttributes,
                    SingleTextNode.``type``,
                    None,
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero),
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
            ))

[<AutoOpen>]
module UnionBuilders =
    type Ast with
        static member private BaseUnion(name: string, typeParams: string list voption) =
            let scalars =
                match typeParams with
                | ValueNone -> StackList.one(Union.Name.WithValue(name))
                | ValueSome typeParams ->
                    StackList.two(Union.Name.WithValue(name), Union.TypeParams.WithValue(typeParams))

            CollectionBuilder<TypeDefnUnionNode, UnionCaseNode>(
                Union.WidgetKey,
                Union.UnionCaseNode,
                AttributesBundle(scalars, Array.empty, Array.empty)
            )

        static member Union(name: string) = Ast.BaseUnion(name, ValueNone)

        static member GenericUnion(name: string, typeParams: string list) =
            Ast.BaseUnion(name, ValueSome typeParams)

[<Extension>]
type UnionModifiers =
    [<Extension>]
    static member inline members(this: WidgetBuilder<TypeDefnUnionNode>) =
        AttributeCollectionBuilder<TypeDefnUnionNode, MemberDefn>(this, Union.Members)

    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnUnionNode>, xmlDocs: string list) =
        this.AddScalar(Union.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<TypeDefnUnionNode>) =
        AttributeCollectionBuilder<TypeDefnUnionNode, AttributeNode>(this, Union.MultipleAttributes)

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<TypeDefnUnionNode>, attributes: string list) =
        AttributeCollectionBuilder<TypeDefnUnionNode, AttributeNode>(this, Union.MultipleAttributes) {
            for attribute in attributes do
                Ast.Attribute(attribute)
        }

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefnUnionNode>, attribute: WidgetBuilder<AttributeNode>) =
        AttributeCollectionBuilder<TypeDefnUnionNode, AttributeNode>(this, Union.MultipleAttributes) { attribute }

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefnUnionNode>, attribute: string) =
        AttributeCollectionBuilder<TypeDefnUnionNode, AttributeNode>(this, Union.MultipleAttributes) {
            Ast.Attribute(attribute)
        }

[<Extension>]
type UnionYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefnUnionNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let typeDefn = TypeDefn.Union(node)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

[<Extension>]
type UnionParameterizedCaseYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<TypeDefnUnionNode, UnionCaseNode>, x: WidgetBuilder<UnionCaseNode>)
        : CollectionContent =
        { Widgets = MutStackArray1.One(x.Compile()) }

    [<Extension>]
    static member inline Yield
        (_: AttributeCollectionBuilder<TypeDefnUnionNode, MemberDefn>, x: WidgetBuilder<BindingNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let widget = Ast.EscapeHatch(MemberDefn.Member(node)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (_: AttributeCollectionBuilder<TypeDefnUnionNode, MemberDefn>, x: BindingNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (_: AttributeCollectionBuilder<TypeDefnUnionNode, MemberDefn>, x: MemberDefnInterfaceNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Interface(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: AttributeCollectionBuilder<TypeDefnUnionNode, MemberDefn>, x: WidgetBuilder<MemberDefnInterfaceNode>) : CollectionContent =
        let node = Gen.mkOak x
        UnionParameterizedCaseYieldExtensions.Yield(this, node)
