namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak

// This widget holds a direct reference to a manually created node so it can be composed in the widget tree
module EscapeHatch =
    let Node = Attributes.defineScalar<obj> "Node"

    let WidgetKey =
        Widgets.register "EscapeHatch" (fun widget ->
            let node = Helpers.getScalarValue widget Node
            node // return the node immediately
        )

[<AutoOpen>]
module EscapeHatchBuilders =
    type Fabulous.AST.Ast with

        static member inline EscapeHatch(node: 'T) =
            WidgetBuilder<'T>(EscapeHatch.WidgetKey, EscapeHatch.Node.WithValue(node))
