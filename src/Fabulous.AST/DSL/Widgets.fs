namespace Fabulous.AST

open Fabulous.AST.ScalarAttributeDefinitions
open Fabulous.AST.WidgetAttributeDefinitions
open Fabulous.AST.WidgetCollectionAttributeDefinitions

module Widgets =
    let register name (createFn: Widget -> 'T) =
        let key = WidgetDefinitionStore.getNextKey()

        let definition =
            { Key = key
              Name = name
              CreateView = createFn >> box }

        WidgetDefinitionStore.set key definition
        key

    let createValueForWidget<'T>(widget: Widget) =
        let definition = WidgetDefinitionStore.get widget.Key
        let value = definition.CreateView widget
        unbox<'T> value

    let registerHighLevel<'T, 'U> name (createFn: Widget -> WidgetBuilder<'T>) =
        let createFn widget : 'U =
            let builder = createFn widget
            let shadowWidget = builder.Compile()
            createValueForWidget<'U> shadowWidget

        register name createFn

    let tryGetScalarValue (widget: Widget) (def: SimpleScalarAttributeDefinition<'data>) =
        match widget.ScalarAttributes with
        | [||] -> ValueNone
        | scalarAttrs ->
            match Array.tryFind (fun (attr: ScalarAttribute) -> attr.Key = def.Key) scalarAttrs with
            | None -> ValueNone
            | Some attr -> ValueSome(unbox<'data> attr.Value)

    let getScalarValue (widget: Widget) (def: SimpleScalarAttributeDefinition<'data>) =
        match tryGetScalarValue widget def with
        | ValueNone -> failwith $"Could not find scalar attribute {def.Name} on widget {widget}"
        | ValueSome value -> value

    let tryGetWidgetValue (widget: Widget) (def: WidgetAttributeDefinition) =
        match widget.WidgetAttributes with
        | [||] -> ValueNone
        | widgetAttrs ->
            match Array.tryFind (fun (attr: WidgetAttribute) -> attr.Key = def.Key) widgetAttrs with
            | None -> ValueNone
            | Some attr -> ValueSome attr.Value

    let tryGetNodeFromWidget<'T> (widget: Widget) (def: WidgetAttributeDefinition) =
        match tryGetWidgetValue widget def with
        | ValueNone -> ValueNone
        | ValueSome value -> ValueSome(createValueForWidget<'T> value)

    let getNodeFromWidget<'T> (widget: Widget) (def: WidgetAttributeDefinition) =
        match tryGetNodeFromWidget<'T> widget def with
        | ValueNone -> failwith $"Could not find widget attribute {def.Name} on widget {widget}"
        | ValueSome value -> value

    let tryGetWidgetCollectionValue (widget: Widget) (def: WidgetCollectionAttributeDefinition) =
        match widget.WidgetCollectionAttributes with
        | [||] -> ValueNone
        | widgetCollectionAttrs ->
            match Array.tryFind (fun (attr: WidgetCollectionAttribute) -> attr.Key = def.Key) widgetCollectionAttrs with
            | None -> ValueNone
            | Some attr -> ValueSome attr.Value

    let getWidgetsFromWidgetCollection (widget: Widget) (def: WidgetCollectionAttributeDefinition) =
        match tryGetWidgetCollectionValue widget def with
        | ValueNone -> failwith $"Could not find widget collection attribute {def.Name} on widget {widget}"
        | ValueSome value ->
            let struct (count, elements) = value
            elements |> Array.take(int count) |> List.ofArray

    let tryGetWidgetsFromWidgetCollection (widget: Widget) (def: WidgetCollectionAttributeDefinition) =
        match tryGetWidgetCollectionValue widget def with
        | ValueNone -> None
        | ValueSome value ->
            let struct (count, elements) = value
            elements |> Array.take(int count) |> List.ofArray |> Some

    let getNodesFromWidgetCollection<'T> (widget: Widget) (def: WidgetCollectionAttributeDefinition) =
        getWidgetsFromWidgetCollection widget def |> List.map createValueForWidget<'T>

    let tryGetNodesFromWidgetCollection<'T> (widget: Widget) (def: WidgetCollectionAttributeDefinition) =
        match tryGetWidgetsFromWidgetCollection widget def with
        | None -> None
        | Some widgets -> Some(widgets |> List.map createValueForWidget<'T>)

    let createNodeFromBuilder(builder: WidgetBuilder<'T>) : 'U =
        builder.Compile() |> createValueForWidget<'U>

[<AbstractClass; Sealed>]
type Ast = class end
