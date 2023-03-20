namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

type IFabModuleDecl = inherit IFabNodeBase

module ModuleOrNamespace =
    
    let ModuleDecls = Attributes.defineWidgetCollection "ModuleDecls"
    
    let WidgetKey = Widgets.register(fun (widget: Widget) ->
        let struct (numberOfElements, moduleDecls) = Helpers.getWidgetCollectionValue widget ModuleDecls
        let moduleDecls =
            moduleDecls
            |> Array.take(int numberOfElements)
            |> Array.map(Helpers.createValueForWidget)
            |> List.ofArray
        ModuleOrNamespaceNode(None, moduleDecls, Range.Zero)
    )