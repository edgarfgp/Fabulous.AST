namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak

type IFabIdentList = inherit IFabNodeBase

module IdentList =
    let Content = Attributes.defineWidgetCollection "Content"
    
    let WidgetKey = Widgets.register "IdentList" (fun widget ->
        let content = Helpers.getNodesFromWidgetCollection<IdentifierOrDot> widget Content
        IdentListNode(content, Range.Zero)
    )

[<AutoOpen>]
module IdentListBuilders =
    type Fabulous.AST.Ast with
        static member inline IdentList() =
            CollectionBuilder<IFabIdentList, IFabIdentifierOrDot>(
                IdentList.WidgetKey,
                IdentList.Content
            )
            
[<Extension>]
type IdentListYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<#IFabIdentList, IFabIdentifierOrDot>, x: WidgetBuilder<#IFabIdentifierOrDot>) : Content =
        { Widgets = MutStackArray1.One(x.Compile()) }