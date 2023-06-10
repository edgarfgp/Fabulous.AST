namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module Union =

    let UnionCaseNode = Attributes.defineWidgetCollection "UnionCaseNode"

    let Name = Attributes.defineWidget "Name"

    let Members = Attributes.defineWidgetCollection "Members"

    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"

    let TypeParams = Attributes.defineScalar<string list option> "TypeParams"

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
                    match values with
                    | None -> None
                    | Some values ->
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
            ))

[<AutoOpen>]
module UnionBuilders =
    type Ast with

        static member inline Union(name: WidgetBuilder<#SingleTextNode>, typeParams: string list option) =
            CollectionBuilder<TypeDefnUnionNode, UnionCaseNode>(
                Union.WidgetKey,
                Union.UnionCaseNode,
                AttributesBundle(
                    StackList.one(Union.TypeParams.WithValue(typeParams)),
                    ValueSome [| Union.Name.WithValue(name.Compile()) |],
                    ValueNone
                )
            )

        static member inline Union(node: SingleTextNode, typeParams: string list option) =
            Ast.Union(Ast.EscapeHatch(node), typeParams)

        static member inline Union(node: SingleTextNode) = Ast.Union(Ast.EscapeHatch(node), None)

        static member inline Union(name: string, ?typeParams: string list) =
            Ast.Union(SingleTextNode(name, Range.Zero), typeParams)

[<Extension>]
type UnionModifiers =
    [<Extension>]
    static member inline members(this: WidgetBuilder<TypeDefnUnionNode>) =
        AttributeCollectionBuilder<TypeDefnUnionNode, MemberDefn>(this, Union.Members)

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<TypeDefnUnionNode>, attributes: string list) =
        this.AddScalar(Union.MultipleAttributes.WithValue(attributes))

[<Extension>]
type UnionYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<TypeDefnUnionNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let typeDefn = TypeDefn.Union(node)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }
