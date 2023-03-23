namespace Fabulous.AST

module Widgets =
    let register name (createFn: Widget -> 'T) =
        let key = WidgetDefinitionStore.getNextKey ()

        let definition =
            { Key = key
              Name = name
              CreateView = createFn >> box }

        WidgetDefinitionStore.set key definition
        key

    let registerHighLevel<'T, 'U> name (createFn: Widget -> WidgetBuilder<'T>) =
        let createFn widget : 'U =
            let builder = createFn widget
            let shadowWidget = builder.Compile()
            Helpers.createValueForWidget<'U> shadowWidget

        register name createFn


[<AbstractClass; Sealed>]
type Ast =
    class
    end
