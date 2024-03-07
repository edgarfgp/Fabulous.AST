namespace Fabulous.AST


module EscapeHatch =
    let Node = Attributes.defineScalar<obj> "Node"

    let WidgetKey =
        Widgets.register "EscapeHatch" (fun widget ->
            let node = Helpers.getScalarValue widget Node
            node
        )

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
