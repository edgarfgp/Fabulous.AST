namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

type IFabIfKeyword = inherit IFabNodeBase

module IfKeyword =
    let SingleWord = Attributes.defineWidget "SingleWord"
    
    let SingleWordWidgetKey = Widgets.register "IfKeyword" (fun widget ->
        let singleWord = Helpers.getNodeFromWidget<SingleTextNode> widget SingleWord
        IfKeywordNode.SingleWord singleWord
    )
    
[<AutoOpen>]
module IfKeywordBuilders =
    type Fabulous.AST.Ast with
        static member inline IfKeyword(singleWord: WidgetBuilder<#IFabSingleText>) =
            WidgetBuilder<IFabIfKeyword>(
                IfKeyword.SingleWordWidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| IfKeyword.SingleWord.WithValue(singleWord.Compile()) |],
                    ValueNone
                )
            )