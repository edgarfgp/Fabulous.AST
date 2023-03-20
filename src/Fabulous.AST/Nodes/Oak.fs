namespace Fabulous.AST


open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak

type IFabOak =
    inherit IFabNodeBase
    
module Oak =    
    let ModulesOrNamespaces = Attributes.defineWidgetCollection "ModulesOrNamespaces"
     
    let WidgetKey = Widgets.register "Oak" (fun (widget: Widget) ->
        let modulesOrNamespaces = Helpers.getNodesFromWidgetCollection<ModuleOrNamespaceNode> widget ModulesOrNamespaces        
        Oak([], modulesOrNamespaces, Range.Zero)
    )
    
[<AutoOpen>]
module OakBuilders =
    type Fabulous.AST.Ast with
        static member inline Oak() =
            CollectionBuilder<IFabOak, IFabModuleOrNamespace>(Oak.WidgetKey, Oak.ModulesOrNamespaces) 

[<Extension>]
type OakYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<#IFabOak, IFabModuleOrNamespace>, x: WidgetBuilder<#IFabModuleOrNamespace>) : Content =
        { Widgets = MutStackArray1.One(x.Compile()) }
