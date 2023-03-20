namespace Fabulous.AST

open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

type IFabBinding = IFabNodeBase

module Binding =
    let LeadingKeyword = Attributes.defineWidget "LeadingKeyword"
    let FunctionName = Attributes.defineWidget "FunctionName"
    let Equals = Attributes.defineWidget "Equals"
    let Expr = Attributes.defineWidget "Expr"
    
    let WidgetKey = Widgets.register "Binding" (fun widget ->
        let leadingKeyword = Helpers.getNodeFromWidget<MultipleTextsNode> widget LeadingKeyword
        let functionName = Helpers.getNodeFromWidget<IdentListNode> widget FunctionName
        let equals = Helpers.getNodeFromWidget<SingleTextNode> widget Equals
        let expr = Helpers.getNodeFromWidget<Expr> widget Expr
        
        BindingNode(
            None,
            None,
            leadingKeyword,
            false,
            None,
            None,
            Choice1Of2 functionName,
            None,
            [],
            None,
            equals,
            expr,
            Range.Zero
        )
    )
    
[<AutoOpen>]
module BindingBuilders =
    type Fabulous.AST.Ast with
        static member inline Binding(leadingKeyword: WidgetBuilder<#IFabMultipleTexts>, functionName: WidgetBuilder<#IFabIdentList>, equals: WidgetBuilder<#IFabSingleText>, expr: WidgetBuilder<#IFabExpr>) =
            WidgetBuilder<IFabBinding>(
                Binding.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [|
                        Binding.LeadingKeyword.WithValue(leadingKeyword.Compile())
                        Binding.FunctionName.WithValue(functionName.Compile())
                        Binding.Equals.WithValue(equals.Compile())
                        Binding.Expr.WithValue(expr.Compile())
                    |],
                    ValueNone
                )
            )