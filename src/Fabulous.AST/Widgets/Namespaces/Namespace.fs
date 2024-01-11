namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

type NamespaceNode
    (identList: IdentListNode, decls: ModuleDecl list, isRecursive: bool, hashDirectives: ParsedHashDirectiveNode list)
    =
    inherit
        Oak(
            hashDirectives,
            [ ModuleOrNamespaceNode(
                  Some(
                      ModuleOrNamespaceHeaderNode(
                          None,
                          None,
                          MultipleTextsNode([ SingleTextNode.``namespace`` ], Range.Zero),
                          None,
                          isRecursive,
                          Some(identList),
                          Range.Zero
                      )
                  ),
                  decls,
                  Range.Zero
              ) ],
            Range.Zero
        )

module Namespace =
    let Decls = Attributes.defineWidgetCollection "Decls"
    let IdentList = Attributes.defineWidget "IdentList"

    let IsRecursive = Attributes.defineScalar<bool> "IsRecursive"

    let ParsedHashDirectives = Attributes.defineWidgetCollection "ParsedHashDirectives"

    let WidgetKey =
        Widgets.register "Namespace" (fun widget ->
            let decls = Helpers.getNodesFromWidgetCollection<ModuleDecl> widget Decls
            let identList = Helpers.getNodeFromWidget widget IdentList
            let isRecursive = Helpers.getScalarValue widget IsRecursive

            let parsedHashDirectives =
                Helpers.tryGetNodesFromWidgetCollection widget ParsedHashDirectives
                |> Option.defaultValue []

            NamespaceNode(identList, decls, isRecursive, parsedHashDirectives))

[<AutoOpen>]
module NamespaceBuilders =
    type Ast with

        static member inline Namespace(identList: WidgetBuilder<#IdentListNode>) =
            CollectionBuilder<NamespaceNode, ModuleDecl>(
                Namespace.WidgetKey,
                Namespace.Decls,
                AttributesBundle(
                    StackList.one(Namespace.IsRecursive.WithValue(false)),
                    ValueSome [| Namespace.IdentList.WithValue(identList.Compile()) |],
                    ValueNone
                )
            )

        static member inline Namespace(node: IdentListNode) = Ast.Namespace(Ast.EscapeHatch(node))

        static member inline Namespace(name: string) =
            Ast.Namespace(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode(name, Range.Zero)) ], Range.Zero))

        static member inline RecNamespace(identList: WidgetBuilder<#IdentListNode>) =
            CollectionBuilder<NamespaceNode, ModuleDecl>(
                Namespace.WidgetKey,
                Namespace.Decls,
                AttributesBundle(
                    StackList.one(Namespace.IsRecursive.WithValue(true)),
                    ValueSome [| Namespace.IdentList.WithValue(identList.Compile()) |],
                    ValueNone
                )
            )

        static member inline RecNamespace(node: IdentListNode) = Ast.RecNamespace(Ast.EscapeHatch(node))

        static member inline RecNamespace(name: string) =
            Ast.RecNamespace(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode(name, Range.Zero)) ], Range.Zero))

[<Extension>]
type NamespaceModifiers =
    [<Extension>]
    static member inline hashDirectives(this: WidgetBuilder<NamespaceNode>) =
        AttributeCollectionBuilder<NamespaceNode, HashDirectiveNode>(this, Namespace.ParsedHashDirectives)

    [<Extension>]
    static member inline hashDirective(this: WidgetBuilder<NamespaceNode>, value: WidgetBuilder<HashDirectiveNode>) =
        AttributeCollectionBuilder<NamespaceNode, HashDirectiveNode>(this, Namespace.ParsedHashDirectives) { value }

[<Extension>]
type NameSpaceExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: AttributeCollectionBuilder<NamespaceNode, HashDirectiveNode>,
            x: WidgetBuilder<HashDirectiveNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let widget = Ast.EscapeHatch(node).Compile()
        { Widgets = MutStackArray1.One(widget) }
