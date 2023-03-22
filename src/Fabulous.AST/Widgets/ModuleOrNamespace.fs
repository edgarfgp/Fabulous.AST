namespace Fabulous.AST

open FSharp.Compiler.Text
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module ModuleOrNamespace =
    let ModuleDecls = Attributes.defineWidgetCollection "ModuleDecls"

    let WidgetKey =
        Widgets.register "ModuleOrNamespace" (fun widget ->
            let moduleDecls =
                Helpers.getNodesFromWidgetCollection<ModuleDecl> widget ModuleDecls

            ModuleOrNamespaceNode(None, moduleDecls, Range.Zero))

[<AutoOpen>]
module ModuleOrNamespaceBuilders =
    type Fabulous.AST.Ast with

        static member inline ModuleOrNamespace() =
            CollectionBuilder<ModuleOrNamespaceNode, ModuleDecl>(
                ModuleOrNamespace.WidgetKey,
                ModuleOrNamespace.ModuleDecls
            )
