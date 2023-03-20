namespace Fabulous.AST


open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

type IFabOak =
    inherit IFabNodeBase
    
module Oak =
    // let ParsedHashDirectives = Attributes.defineScalar<ParsedHashDirectiveNode list> "ParsedHashDirectives"
    
    let ModulesOrNamespaces = Attributes.defineWidgetCollection "ModulesOrNamespaces"
     
    let WidgetKey = Widgets.register "Oak" (fun (widget: Widget) ->
        // let parsedHashDirectives = Helpers.getScalarValue widget ParsedHashDirectives
        let modulesOrNamespaces = Helpers.getWidgetCollectionValue widget ModulesOrNamespaces        
        Oak([], modulesOrNamespaces, Range.Zero)
    )
    
[<AutoOpen>]
module OakBuilders =
    type Fabulous.AST.Node with
        static member inline Oak() =
            CollectionBuilder<IFabOak, IFabModuleOrNamespace>(Oak.WidgetKey, Oak.ModulesOrNamespaces) 
            
// [<Extension>]
// type OakModifiers =
//     [<Extension>]
//     static member inline parsedHashDirectives(this: WidgetBuilder<#IFabOak>, value: ParsedHashDirectiveNode list) =
//         this.AddScalar(Oak.ParsedHashDirectives.WithValue(value))

[<Extension>]
type OakYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<#IFabOak, IFabModuleOrNamespace>, x: WidgetBuilder<#IFabModuleOrNamespace>) : Content =
        { Widgets = MutStackArray1.One(x.Compile()) }
