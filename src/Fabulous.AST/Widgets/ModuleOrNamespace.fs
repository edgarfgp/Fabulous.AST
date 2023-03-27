namespace Fabulous.AST

open FSharp.Compiler.Text
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module ModuleOrNamespace =
    let ModuleDecls = Attributes.defineWidgetCollection "ModuleDecls"
   
    let WidgetKey =
        Widgets.register "ModuleOrNamespace" (fun widget ->
            let name = ""
            
            let moduleDecls =
                Helpers.getNodesFromWidgetCollection<ModuleDecl> widget ModuleDecls
                
            let header =
                ModuleOrNamespaceHeaderNode(
                None,
                None,
                MultipleTextsNode([SingleTextNode("namespace", Range.Zero)], Range.Zero),
                None,
                false,
                Some (IdentListNode([ IdentifierOrDot.Ident(SingleTextNode(name, Range.Zero)) ], Range.Zero)),
                Range.Zero
            )

            ModuleOrNamespaceNode(None, moduleDecls, Range.Zero)
            
        )

[<AutoOpen>]
module ModuleOrNamespaceBuilders =
    type Fabulous.AST.Ast with

        static member inline ModuleOrNamespace() =
            CollectionBuilder<ModuleOrNamespaceNode, ModuleDecl>(
                ModuleOrNamespace.WidgetKey,
                ModuleOrNamespace.ModuleDecls
            )
