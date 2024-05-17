namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak

type AnonymousModuleNode(decls) =
    inherit ModuleOrNamespaceNode(None, decls, Range.Zero)

module AnonymousModule =
    let Decls = Attributes.defineWidgetCollection "Decls"

    let WidgetKey =
        Widgets.register "AnonymousModule" (fun widget ->
            let decls = Widgets.getNodesFromWidgetCollection<ModuleDecl> widget Decls
            AnonymousModuleNode(decls))

[<AutoOpen>]
module AnonymousModuleBuilders =
    type Ast with
        static member AnonymousModule() =
            CollectionBuilder<AnonymousModuleNode, ModuleDecl>(AnonymousModule.WidgetKey, AnonymousModule.Decls)
