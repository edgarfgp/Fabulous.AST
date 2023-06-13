namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

type ModuleNode(identList: IdentListNode, decls: ModuleDecl list) =
    inherit
        Oak(
            List.Empty,
            [ ModuleOrNamespaceNode(
                  Some(
                      ModuleOrNamespaceHeaderNode(
                          None,
                          None,
                          MultipleTextsNode([ SingleTextNode.``module`` ], Range.Zero),
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

module Module =
    let Decls = Attributes.defineWidgetCollection "Decls"
    let IdentList = Attributes.defineWidget "IdentList"

    let WidgetKey =
        Widgets.register "Module" (fun widget ->
            let decls = Helpers.getNodesFromWidgetCollection<ModuleDecl> widget Decls
            let identList = Helpers.getNodeFromWidget<IdentListNode> widget IdentList
            ModuleNode(identList, decls))

[<AutoOpen>]
module ModuleBuilders =
    type Ast with

        static member inline Module(identList: WidgetBuilder<#IdentListNode>) =
            CollectionBuilder<ModuleNode, ModuleDecl>(
                Module.WidgetKey,
                Module.Decls,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| Module.IdentList.WithValue(identList.Compile()) |],
                    ValueNone
                )
            )

        static member inline Module(node: IdentListNode) = Ast.Module(Ast.EscapeHatch(node))

        static member inline Module(name: string) =
            Ast.Module(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode(name, Range.Zero)) ], Range.Zero))
