namespace Fabulous.AST

module Widgets =
    let register<'T> (createFn: Widget -> 'T) =
        let key = WidgetDefinitionStore.getNextKey()
        let definition =
          { Key = key
            Name = typeof<'T>.Name
            CreateView = createFn >> box }
        
        WidgetDefinitionStore.set key definition
        key

[<AbstractClass; Sealed>]
type Node =
    class
    end
