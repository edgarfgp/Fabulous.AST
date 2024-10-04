namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.Builders
open Fantomas.Core.SyntaxOak

module EscapeHatch =
    let Node = Attributes.defineScalar<obj> "Node"

    let WidgetKey =
        Widgets.register "EscapeHatch" (fun widget ->
            let node = Widgets.getScalarValue widget Node
            node)

[<AutoOpen>]
module EscapeHatchBuilders =
    type Ast with

        /// <summary>
        /// Creates a widget that holds a reference to a SyntaxTree node so it can be composed in the widget tree.
        /// </summary>
        /// <param name="node">The node to hold a reference to</param>
        /// <code language="fsharp">
        /// EscapeHatch(BindingNode(...))
        /// </code>
        static member inline EscapeHatch(node: 'T) =
            WidgetBuilder<'T>(EscapeHatch.WidgetKey, EscapeHatch.Node.WithValue(node))

type EscapeHatchModifiers =

    [<Extension>]
    static member inline commentsBefore(this: WidgetBuilder<#NodeBase>, value: WidgetBuilder<TriviaNode>) =
        let node = Gen.mkOak this
        let value = Gen.mkOak value
        node.AddBefore(value)
        Ast.EscapeHatch(node)
