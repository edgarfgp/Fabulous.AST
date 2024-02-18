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

    let WidgetKey =
        Widgets.register "Namespace" (fun widget ->
            let decls = Helpers.getNodesFromWidgetCollection<ModuleDecl> widget Decls
            let identList = Helpers.getNodeFromWidget widget IdentList
            let isRecursive = Helpers.getScalarValue widget IsRecursive

            NamespaceNode(identList, decls, isRecursive, []))

[<AutoOpen>]
module NamespaceBuilders =
    type Ast with

        static member Namespace(identList: WidgetBuilder<#IdentListNode>) =
            CollectionBuilder<NamespaceNode, ModuleDecl>(
                Namespace.WidgetKey,
                Namespace.Decls,
                AttributesBundle(
                    StackList.one(Namespace.IsRecursive.WithValue(false)),
                    ValueSome [| Namespace.IdentList.WithValue(identList.Compile()) |],
                    ValueNone
                )
            )

        static member Namespace(node: IdentListNode) = Ast.Namespace(Ast.EscapeHatch(node))

        static member Namespace(name: string) =
            Ast.Namespace(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode(name, Range.Zero)) ], Range.Zero))

        static member RecNamespace(identList: WidgetBuilder<#IdentListNode>) =
            CollectionBuilder<NamespaceNode, ModuleDecl>(
                Namespace.WidgetKey,
                Namespace.Decls,
                AttributesBundle(
                    StackList.one(Namespace.IsRecursive.WithValue(true)),
                    ValueSome [| Namespace.IdentList.WithValue(identList.Compile()) |],
                    ValueNone
                )
            )

        static member RecNamespace(node: IdentListNode) = Ast.RecNamespace(Ast.EscapeHatch(node))

        static member RecNamespace(name: string) =
            Ast.RecNamespace(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero))
