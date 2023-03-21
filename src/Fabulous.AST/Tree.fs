namespace Fabulous.AST

/// This is the equivalent of Program in Fabulous
/// It takes the root of the widget tree and create the corresponding Fantomas node, and recursively creating all children nodes
module Tree =
    let compile (root: WidgetBuilder<'T>) : 'T =
        let widget = root.Compile()
        let definition = WidgetDefinitionStore.get widget.Key
        definition.CreateView widget |> unbox