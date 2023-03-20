namespace Fabulous.AST

module Widgets =
    let register name (createFn: Widget -> 'T) =
        let key = WidgetDefinitionStore.getNextKey()
        let definition =
          { Key = key
            Name = name
            CreateView = createFn >> box }
        
        WidgetDefinitionStore.set key definition
        key

[<AbstractClass; Sealed>]
type Node =
    class
    end
