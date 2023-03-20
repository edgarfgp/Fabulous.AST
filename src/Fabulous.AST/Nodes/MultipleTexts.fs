namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak

type IFabMultipleTexts = inherit IFabNodeBase

module MultipleTexts =
    let Content = Attributes.defineWidgetCollection "Content"
    
    let WidgetKey = Widgets.register "MultipleTexts" (fun widget ->
        let content = Helpers.getNodesFromWidgetCollection<SingleTextNode> widget Content
        MultipleTextsNode(content, Range.Zero)
    )

[<AutoOpen>]
module MultipleTextsBuilders =
    type Fabulous.AST.Ast with
        static member inline MultipleTexts() =
            CollectionBuilder<IFabMultipleTexts, IFabSingleText>(
                MultipleTexts.WidgetKey,
                MultipleTexts.Content
            )
            
[<Extension>]
type MultipleTextsYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<#IFabMultipleTexts, IFabSingleText>, x: WidgetBuilder<#IFabSingleText>) : Content =
        { Widgets = MutStackArray1.One(x.Compile()) }