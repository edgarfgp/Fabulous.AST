namespace Fabulous.AST

/// It takes the root of the widget tree and create the corresponding Fantomas node, and recursively creating all children nodes
module Gen =
    let ast (root: WidgetBuilder<'node>) : 'node =
        let widget = root.Compile()
        let definition = WidgetDefinitionStore.get widget.Key
        definition.CreateView widget |> unbox
