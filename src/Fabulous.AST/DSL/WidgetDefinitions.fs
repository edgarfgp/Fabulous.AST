namespace Fabulous.AST

/// Widget definition to create a control
type WidgetDefinition =
    { Key: WidgetKey
      Name: string
      CreateView: Widget -> obj }

module WidgetDefinitionStore =
    let private _lock = obj()
    let private _widgets = ResizeArray<WidgetDefinition>()
    let mutable private _nextKey = 0

    let get key = lock _lock (fun () -> _widgets[key])

    let set key value =
        lock _lock (fun () -> _widgets[key] <- value)

    let getNextKey() : WidgetKey =
        lock _lock (fun () ->
            _widgets.Add(Unchecked.defaultof<WidgetDefinition>)
            let key = _nextKey
            _nextKey <- _nextKey + 1
            key)
