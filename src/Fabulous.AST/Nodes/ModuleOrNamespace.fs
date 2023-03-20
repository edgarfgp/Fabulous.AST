namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

type IFabModuleOrNamespace = inherit IFabNodeBase

module ModuleOrNamespace =
    
    let ModuleDecls = Attributes.defineWidgetCollection "ModuleDecls"
    
    let WidgetKey = Widgets.register "ModuleOrNamespace" (fun (widget: Widget) ->
        let moduleDecls = Helpers.getNodesFromWidgetCollection<ModuleDecl> widget ModuleDecls
        ModuleOrNamespaceNode(None, moduleDecls, Range.Zero)
    )
    
[<AutoOpen>]
module ModuleOrNamespaceBuilders =
    type Fabulous.AST.Ast with
        static member inline ModuleOrNamespace() =
            CollectionBuilder<IFabModuleOrNamespace, IFabModuleDecl>(ModuleOrNamespace.WidgetKey, ModuleOrNamespace.ModuleDecls) 
            
[<Extension>]
type ModuleOrNamespaceYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<#IFabModuleOrNamespace, IFabModuleDecl>, x: WidgetBuilder<#IFabModuleDecl>) : Content =
        { Widgets = MutStackArray1.One(x.Compile()) }
        
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<#IFabModuleOrNamespace, IFabModuleDecl>, x: WidgetBuilder<IFabBinding>) : Content =
        { Widgets = MutStackArray1.One(TopLevelBinding(x).Compile()) }
        
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<#IFabModuleOrNamespace, IFabModuleDecl>, x: WidgetBuilder<IFabExpr>) : Content =
        { Widgets = MutStackArray1.One(DeclExpr(x).Compile()) }
        
    /// Prefer the signature taking a WidgetBuilder argument as it is more type-safe
    /// This extension is to enable the use of the DSL inside high-level nodes
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<#IFabModuleOrNamespace, IFabModuleDecl>, x: Widget) : Content =
        { Widgets = MutStackArray1.One(x) }
