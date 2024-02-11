namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module Augmentation =

    let Name = Attributes.defineScalar<string> "Name"
    let Members = Attributes.defineWidgetCollection "Members"

    let WidgetKey =
        Widgets.register "Augmentation" (fun widget ->
            let name = Helpers.getScalarValue widget Name
            let members = Helpers.tryGetNodesFromWidgetCollection<MemberDefn> widget Members

            let members =
                match members with
                | Some members -> members
                | None -> []

            TypeDefnAugmentationNode(
                TypeNameNode(
                    None,
                    None,
                    SingleTextNode.``type``,
                    Some(SingleTextNode.Create(name)),
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.``with``) ], Range.Zero),
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

        static member Augmentation(name: string) =
            CollectionBuilder<TypeDefnAugmentationNode, MemberDefn>(
                Augmentation.WidgetKey,
                Augmentation.Members,
                AttributesBundle(StackList.one(Augmentation.Name.WithValue(name)), ValueNone, ValueNone)
            )

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
            x: MethodMemberNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            this: CollectionBuilder<TypeDefnAugmentationNode, MemberDefn>,
            x: WidgetBuilder<MethodMemberNode>
        ) : CollectionContent =
        let node = Tree.compile x
        AugmentYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<TypeDefnAugmentationNode, MemberDefn>,
            x: PropertyMemberNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            this: CollectionBuilder<TypeDefnAugmentationNode, MemberDefn>,
            x: WidgetBuilder<PropertyMemberNode>
        ) : CollectionContent =
        let node = Tree.compile x
        AugmentYieldExtensions.Yield(this, node)
