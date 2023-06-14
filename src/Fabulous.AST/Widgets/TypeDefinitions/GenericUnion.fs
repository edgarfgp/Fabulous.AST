namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

type GenericUnionTypeDefnUnionNode
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


module GenericUnion =

    let UnionCaseNode = Attributes.defineWidgetCollection "UnionCaseNode"

    let Name = Attributes.defineWidget "Name"

    let Members = Attributes.defineWidgetCollection "Members"

    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"

    let TypeParams = Attributes.defineScalar<string list> "TypeParams"

    let WidgetKey =
        Widgets.register "GenericUnion" (fun widget ->
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
                              TyparDeclNode(None, SingleTextNode.Create v, Range.Zero) ],
                        [],
                        SingleTextNode.greaterThan,
                        Range.Zero
                    )
                    |> TyparDecls.PostfixList
                    |> Some
                | ValueNone -> None

            GenericUnionTypeDefnUnionNode(name, multipleAttributes, unionCaseNode, members, typeParams))

[<AutoOpen>]
module GenericUnionBuilders =
    type Ast with

        static member inline GenericUnion(name: WidgetBuilder<#SingleTextNode>, typeParams: string list) =
            CollectionBuilder<GenericUnionTypeDefnUnionNode, UnionCaseNode>(
                GenericUnion.WidgetKey,
                GenericUnion.UnionCaseNode,
                AttributesBundle(
                    StackList.one(GenericUnion.TypeParams.WithValue(typeParams)),
                    ValueSome [| GenericUnion.Name.WithValue(name.Compile()) |],
                    ValueNone
                )
            )

        static member inline GenericUnion(node: SingleTextNode, typeParams: string list) =
            Ast.GenericUnion(Ast.EscapeHatch(node), typeParams)

        static member inline GenericUnion(name: string, typeParams: string list) =
            Ast.GenericUnion(SingleTextNode(name, Range.Zero), typeParams)

[<Extension>]
type GenericUnionModifiers =
    [<Extension>]
    static member inline members(this: WidgetBuilder<GenericUnionTypeDefnUnionNode>) =
        AttributeCollectionBuilder<GenericUnionTypeDefnUnionNode, MemberDefn>(this, GenericUnion.Members)

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<GenericUnionTypeDefnUnionNode>, attributes: string list) =
        this.AddScalar(GenericUnion.MultipleAttributes.WithValue(attributes))

    [<Extension>]
    static member inline isStruct(this: WidgetBuilder<GenericUnionTypeDefnUnionNode>) =
        GenericUnionModifiers.attributes(this, [ "Struct" ])

[<Extension>]
type GenericUnionYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<GenericUnionTypeDefnUnionNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let typeDefn = TypeDefn.Union(node)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

[<Extension>]
type GenericUnionParameterizedCaseYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<GenericUnionTypeDefnUnionNode, UnionCaseNode>,
            x: WidgetBuilder<UnionParameterizedCaseNode>
        ) : CollectionContent =
        { Widgets = MutStackArray1.One(x.Compile()) }
