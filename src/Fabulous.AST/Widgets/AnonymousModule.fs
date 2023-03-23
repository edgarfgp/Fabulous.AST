namespace Fabulous.AST

open FSharp.Compiler.Text
open Fantomas.Core.SyntaxOak

module AnonymousModule =
    let ModuleDecls = Attributes.defineWidgetCollection "ModuleDecls"

    let WidgetKey =
        Widgets.register "AnonymousModule" (fun widget ->
            let moduleDecls =
                Helpers.getNodesFromWidgetCollection<ModuleDecl> widget ModuleDecls

            Oak(List.Empty, [ ModuleOrNamespaceNode(None, moduleDecls, Range.Zero) ], Range.Zero))

[<AutoOpen>]
module AnonymousModuleBuilders =
    type Fabulous.AST.Ast with

        static member inline AnonymousModule() =
            CollectionBuilder<Oak, ModuleDecl>(AnonymousModule.WidgetKey, AnonymousModule.ModuleDecls)
