namespace Fabulous.AST

open FSharp.Compiler.Text
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module ImplicitModule =
    let ModuleDecls = Attributes.defineWidgetCollection "ModuleDecls"
    
    let WidgetKey = Widgets.register "ImplicitModule" (fun widget ->
        let moduleDecls = Helpers.getWidgetsFromWidgetCollection widget ModuleDecls
        
        let moduleNode =
            ModuleOrNamespaceNode(
                None,
                [ for decl in moduleDecls -> Helpers.createValueForWidget<ModuleDecl> decl ],
                Range.Zero
            )
        
        Oak(List.Empty, [ moduleNode ], Range.Zero)
    )
    
[<AutoOpen>]
module ImplicitModuleBuilders =
    type Fabulous.AST.Ast with
        static member inline ImplicitModule() =
            CollectionBuilder<Oak, ModuleDecl>(
                ImplicitModule.WidgetKey,
                ImplicitModule.ModuleDecls
            )