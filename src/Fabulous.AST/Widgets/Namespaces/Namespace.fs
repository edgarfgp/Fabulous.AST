namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

type NamespaceNode(identList: IdentListNode, decls: ModuleDecl list) =
    inherit
        Oak(
            List.Empty,
            [ ModuleOrNamespaceNode(
                  Some(
                      ModuleOrNamespaceHeaderNode(
                          None,
                          None,
                          MultipleTextsNode([ SingleTextNode("namespace", Range.Zero) ], Range.Zero),
                          None,
                          false,
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

    let WidgetKey =
        Widgets.register "Namespace" (fun widget ->
            let decls = Helpers.getNodesFromWidgetCollection<ModuleDecl> widget Decls
            let identList = Helpers.getNodeFromWidget<IdentListNode> widget IdentList
            NamespaceNode(identList, decls))

[<AutoOpen>]
module ModuleOrNamespaceBuilders =
    type Fabulous.AST.Ast with

        static member inline Namespace(identList: WidgetBuilder<#IdentListNode>) =
            CollectionBuilder<NamespaceNode, ModuleDecl>(
                Namespace.WidgetKey,
                Namespace.Decls,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| Namespace.IdentList.WithValue(identList.Compile()) |],
                    ValueNone
                )
            )

        static member inline Namespace(node: IdentListNode) = Ast.Namespace(Ast.EscapeHatch(node))

        static member inline Namespace(name: string) =
            Ast.Namespace(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode(name, Range.Zero)) ], Range.Zero))
