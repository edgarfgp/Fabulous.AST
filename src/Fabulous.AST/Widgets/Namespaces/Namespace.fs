namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

type NamespaceNode(identList: IdentListNode, decls: ModuleDecl list, isRecursive: bool) =
    inherit
        Oak(
            List.Empty,
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
            let identList = Helpers.getNodeFromWidget<IdentListNode> widget IdentList

            let isRecursive =
                Helpers.tryGetScalarValue widget IsRecursive |> ValueOption.defaultValue false

            NamespaceNode(identList, decls, isRecursive))

[<AutoOpen>]
module NamespaceBuilders =
    type Ast with

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

[<Extension>]
type NamespaceModifiers =
    [<Extension>]
    static member inline isRecursive(this: CollectionBuilder<NamespaceNode, ModuleDecl>) =
        this.AddScalar(Namespace.IsRecursive.WithValue(true))
