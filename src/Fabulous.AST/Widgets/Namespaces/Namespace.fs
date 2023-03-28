namespace Fabulous.AST

open FSharp.Compiler.Text
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

type NamespaceNode(name: string, decls: ModuleDecl list) =
    inherit ModuleOrNamespaceNode(
        Some (
            ModuleOrNamespaceHeaderNode(
                None,
                None,
                MultipleTextsNode([ SingleTextNode("namespace", Range.Zero) ], Range.Zero),
                None,
                false,
                Some(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode(name, Range.Zero)) ], Range.Zero)),
                Range.Zero
            )
        ),
        decls,
        Range.Zero
    )

module Namespace =
    let Name = Attributes.defineScalar "Name"
    let Decls = Attributes.defineWidgetCollection "Decls"

    let WidgetKey =
        Widgets.register "Namespace" (fun widget ->
            let name = Helpers.getScalarValue widget Name
            let decls = Helpers.getNodesFromWidgetCollection<ModuleDecl> widget Decls
            NamespaceNode(name, decls)
        )

[<AutoOpen>]
module ModuleOrNamespaceBuilders =
    type Fabulous.AST.Ast with

        static member inline Namespace() =
            CollectionBuilder<NamespaceNode, ModuleDecl>(
                Namespace.WidgetKey,
                Namespace.Decls
            )
