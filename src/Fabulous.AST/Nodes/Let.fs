namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core
open Fantomas.Core.SyntaxOak
open Fabulous

open type Fabulous.AST.Node

type IFabLet = inherit IFabBindingNode

module Let =
    let Name = Attributes.defineScalar<string> "Name"
    let Value = Attributes.defineScalar<string> "Value"
    
    let WidgetKey = Widgets.register "Let" (fun (widget: Widget) ->
        let name = Helpers.getScalarValue widget Name
        let value = Helpers.getScalarValue widget Value
        
        let ``let`` =
            MultipleTextsNode(
                [ SingleTextNode("let", Range.Zero) ],
                Range.Zero
            )
            
        let nameNode =
            IdentListNode([
                IdentifierOrDot.Ident(SingleTextNode(name, Range.Zero))
            ], Range.Zero)
            
        let equals = SingleTextNode("=", Range.Zero)
        
        let valueExpr =
            Expr.Constant(
                Constant.FromText(
                    SingleTextNode(value, Range.Zero)
                )
            )
        
        BindingNode(
            None,
            None,
            ``let``,
            false,
            None,
            None,
            Choice1Of2 nameNode,
            None,
            [],
            None,
            equals,
            valueExpr,
            Range.Zero
        )
    )

[<AutoOpen>]
module LetBuilders =
    type Fabulous.AST.Node with
        static member inline Let(name: string, value: string) =
            WidgetBuilder<IFabLet>(
                Let.WidgetKey,
                Let.Name.WithValue(name),
                Let.Value.WithValue(value)
            )
            

[<Extension>]
type LetYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<#IFabModuleOrNamespace, IFabNodeBase>, x: WidgetBuilder<IFabLet>) : Content =
        { Widgets = MutStackArray1.One(TopLevelBinding(x).Compile()) }