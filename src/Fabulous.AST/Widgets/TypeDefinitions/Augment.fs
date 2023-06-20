namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module Augment =

    let Name = Attributes.defineWidget "Name"
    let Parameters = Attributes.defineScalar<SimplePatNode list option> "Parameters"

    let Members = Attributes.defineWidgetCollection "Members"

    let WidgetKey =
        Widgets.register "Augment" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name
            let parameters = Helpers.tryGetScalarValue widget Parameters
            let members = Helpers.tryGetNodesFromWidgetCollection<MemberDefn> widget Members

            let members =
                match members with
                | Some members -> members
                | None -> []

            TypeDefnAugmentationNode(
                TypeNameNode(
                    None,
                    None,
                    SingleTextNode("type", Range.Zero),
                    Some(name),
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode("with", Range.Zero)) ], Range.Zero),
                    None,
                    [],
                    None,
                    None,
                    None,
                    Range.Zero
                ),
                members,
                Range.Zero
            ))

[<AutoOpen>]
module AugmentBuilders =
    type Ast with

        static member inline Augment(name: WidgetBuilder<#SingleTextNode>, parameters: SimplePatNode list option) =
            CollectionBuilder<TypeDefnAugmentationNode, MemberDefn>(
                Augment.WidgetKey,
                Augment.Members,
                AttributesBundle(
                    StackList.one(Augment.Parameters.WithValue(parameters)),
                    ValueSome [| Augment.Name.WithValue(name.Compile()) |],
                    ValueNone
                )
            )

        static member inline Augment(node: SingleTextNode, parameters: SimplePatNode list option) =
            match parameters with
            | None -> Ast.Augment(Ast.EscapeHatch(node), None)
            | Some parameters -> Ast.Augment(Ast.EscapeHatch(node), Some parameters)

        static member inline Augment(name: string, ?parameters: SimplePatNode list) =
            Ast.Augment(SingleTextNode(name, Range.Zero), parameters)

[<Extension>]
type AugmentYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<TypeDefnAugmentationNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let typeDefn = TypeDefn.Augmentation(node)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<TypeDefnAugmentationNode, MemberDefn>,
            x: MemberDefn
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(x).Compile()
        { Widgets = MutStackArray1.One(widget) }
