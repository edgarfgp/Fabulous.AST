namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module Oak =
    let ModuleOrNamespaces = Attributes.defineWidgetCollection "ModuleOrNamespaces"
    
    let WidgetKey = Widgets.register "Oak" (fun widget ->
        let moduleOrNamespaces = Helpers.getNodesFromWidgetCollection<ModuleOrNamespaceNode> widget ModuleOrNamespaces
        Oak(List.Empty, moduleOrNamespaces, Range.Zero)
    )
    
[<AutoOpen>]
module OakBuilders =
    type Fabulous.AST.Ast with
        static member inline Oak() =
            CollectionBuilder<Oak, ModuleOrNamespaceNode>(
                Oak.WidgetKey,
                Oak.ModuleOrNamespaces
            )
            
[<Extension>]
type OakYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<Oak, ModuleOrNamespaceNode>, x: WidgetBuilder<ModuleOrNamespaceNode>) : Content =
        { Widgets = MutStackArray1.One(x.Compile()) }