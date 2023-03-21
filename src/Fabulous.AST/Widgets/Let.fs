namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module Let =
    let Name = Attributes.defineScalar "Name"
    let Value = Attributes.defineScalar "Value"
    
    let WidgetKey = Widgets.register "Let" (fun widget ->
        let name = Helpers.getScalarValue widget Name
        let value = Helpers.getScalarValue widget Value
        
        BindingNode(
            None,
            None,
            MultipleTextsNode([SingleTextNode("let", Range.Zero)], Range.Zero),
            false,
            None,
            None,
            Choice1Of2 (IdentListNode([IdentifierOrDot.Ident(SingleTextNode(name, Range.Zero))], Range.Zero)),
            None,
            List.Empty,
            None,
            SingleTextNode("=", Range.Zero),
            Expr.Constant(Constant.FromText(SingleTextNode(value, Range.Zero))),
            Range.Zero
        )
    )

[<AutoOpen>]
module LetBuilders =
    type Fabulous.AST.Ast with
        static member inline Let(name: string, value: string) =
            WidgetBuilder<BindingNode>(
                Let.WidgetKey,
                Let.Name.WithValue(name),
                Let.Value.WithValue(value)
            )
            
[<Extension>]
type LetYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<Oak, ModuleDecl>, x: WidgetBuilder<BindingNode>) : Content =
        { Widgets = MutStackArray1.One(TopLevelBinding(x).Compile()) }
