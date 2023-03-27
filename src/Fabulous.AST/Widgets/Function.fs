namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module Function =
    let Name = Attributes.defineScalar "Name"
    let Parameters = Attributes.defineScalar<string[]> "Value"
    let BodyExpr = Attributes.defineWidget "BodyExpr"
    let IsInlined = Attributes.defineScalar<bool> "IsInlined"

    let WidgetKey =
        Widgets.register "Function" (fun widget ->
            let name = Helpers.getScalarValue widget Name
            let parameters = Helpers.getScalarValue widget Parameters
            let bodyExpr = Helpers.getNodeFromWidget<Expr> widget BodyExpr
            let isInlined = Helpers.tryGetScalarValue widget IsInlined

            BindingNode(
                None,
                None,
                MultipleTextsNode([ SingleTextNode("let", Range.Zero) ], Range.Zero),
                false,
                (match isInlined with
                 | ValueNone
                 | ValueSome false -> None
                 | ValueSome true -> Some(SingleTextNode("inline", Range.Zero))),
                None,
                Choice1Of2(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode(name, Range.Zero)) ], Range.Zero)),
                None,
                [ for param in parameters do
                      Pattern.Named(PatNamedNode(None, SingleTextNode(param, Range.Zero), Range.Zero)) ],
                None,
                SingleTextNode("=", Range.Zero),
                bodyExpr,
                Range.Zero
            ))

[<AutoOpen>]
module FunctionBuilders =
    type Fabulous.AST.Ast with

        static member inline Function(name: string, parameters: string[]) =
            SingleChildBuilder<BindingNode, Expr>(
                Function.WidgetKey,
                Function.BodyExpr,
                AttributesBundle(
                    StackList.two(Function.Name.WithValue(name), Function.Parameters.WithValue(parameters)),
                    ValueNone,
                    ValueNone
                )
            )

[<Extension>]
type FunctionModifiers =
    [<Extension>]
    static member inline isInlined(this: SingleChildBuilder<BindingNode, Expr>) =
        this.AddScalar(Function.IsInlined.WithValue(true))
