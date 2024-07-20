namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module Augmentation =

    let Name = Attributes.defineScalar<string> "Name"
    let Members = Attributes.defineWidgetCollection "Members"

    let WidgetKey =
        Widgets.register "Augmentation" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

            let members =
                Widgets.tryGetNodesFromWidgetCollection<MemberDefn> widget Members
                |> ValueOption.defaultValue []

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
                AttributesBundle(StackList.one(Augmentation.Name.WithValue(name)), Array.empty, Array.empty)
            )

type AugmentYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: TypeDefnAugmentationNode)
        : CollectionContent =
        let typeDefn = TypeDefn.Augmentation(x)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefnAugmentationNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        AugmentYieldExtensions.Yield(this, node)
