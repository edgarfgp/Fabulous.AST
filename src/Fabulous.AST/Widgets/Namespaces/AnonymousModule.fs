namespace Fabulous.AST

open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak

type AnonymousModuleNode(decls) =
    inherit Oak(List.Empty, [ ModuleOrNamespaceNode(None, decls, Range.Zero) ], Range.Zero)

module AnonymousModule =
    let Decls = Attributes.defineWidgetCollection "Decls"

    let WidgetKey =
        Widgets.register "AnonymousModule" (fun widget ->
            let decls = Helpers.getNodesFromWidgetCollection<ModuleDecl> widget Decls
            AnonymousModuleNode(decls))

[<AutoOpen>]
module AnonymousModuleBuilders =
    type Ast with

        static member inline AnonymousModule() =
            CollectionBuilder<AnonymousModuleNode, ModuleDecl>(AnonymousModule.WidgetKey, AnonymousModule.Decls)
