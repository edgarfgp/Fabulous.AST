namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

type IFabModuleOrNamespace = inherit IFabNodeBase

module ModuleOrNamespace =
    
    let ModuleDecls = Attributes.defineWidgetCollection "ModuleDecls"
    
    let WidgetKey = Widgets.register "ModuleOrNamespace" (fun (widget: Widget) ->
        let moduleDecls = Helpers.getWidgetCollectionValue widget ModuleDecls
        ModuleOrNamespaceNode(None, moduleDecls, Range.Zero)
    )
    
[<AutoOpen>]
module ModuleOrNamespaceBuilders =
    type Fabulous.AST.Node with
        static member inline ModuleOrNamespace() =
            CollectionBuilder<IFabModuleOrNamespace, IFabNodeBase>(ModuleOrNamespace.WidgetKey, ModuleOrNamespace.ModuleDecls) 
            

[<Extension>]
type ModuleOrNamespaceYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<#IFabModuleOrNamespace, IFabNodeBase>, x: WidgetBuilder<#IFabNodeBase>) : Content =
        { Widgets = MutStackArray1.One(x.Compile()) }
