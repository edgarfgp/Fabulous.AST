namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

type IFabImplicitModule =
    inherit IFabModuleOrNamespace // for children
    inherit IFabOak // for public visibility

module ImplicitModule =
    let WidgetKey = Widgets.registerHighLevel<IFabOak, Oak> "ImplicitModule" (fun widget ->
        let moduleDecls = Helpers.getWidgetsFromWidgetCollection widget ModuleOrNamespace.ModuleDecls
        
        Oak() {
            ModuleOrNamespace() {
                for decl in moduleDecls do
                    decl
            }
        }
    )
    
[<AutoOpen>]
module ImplicitModuleBuilders =
    type Fabulous.AST.Ast with
        static member inline ImplicitModule() =
            CollectionBuilder<IFabImplicitModule, IFabModuleDecl>(
                ImplicitModule.WidgetKey,
                ModuleOrNamespace.ModuleDecls
            )
            
[<Extension>]
type ImplicitModuleYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<#IFabOak, IFabModuleDecl>, x: WidgetBuilder<#IFabModuleDecl>) : Content =
        { Widgets = MutStackArray1.One(x.Compile()) }