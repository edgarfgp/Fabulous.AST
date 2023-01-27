namespace Fabulous.AST

module ScalarAttributeDefinitions =
    /// Attribute definition for boxed scalar properties
    [<Struct>]
    type SimpleScalarAttributeDefinition<'T> =
        { Key: ScalarAttributeKey
          Name: string }

        member inline x.WithValue(value: 'T) : ScalarAttribute =
            { Key = x.Key
#if DEBUG
              DebugName = x.Name
#endif
              NumericValue = 0UL
              Value = value }

module WidgetAttributeDefinitions =
    /// Attribute definition for widget properties
    [<Struct>]
    type WidgetAttributeDefinition =
        { Key: WidgetAttributeKey
          Name: string }

        member inline x.WithValue(value: Widget) : WidgetAttribute =
            { Key = x.Key
#if DEBUG
              DebugName = x.Name
#endif
              Value = value }

module WidgetCollectionAttributeDefinitions =
    /// Attribute definition for collection properties
    [<Struct>]
    type WidgetCollectionAttributeDefinition =
        { Key: WidgetCollectionAttributeKey
          Name: string }

        member inline x.WithValue(value: ArraySlice<Widget>) : WidgetCollectionAttribute =
            { Key = x.Key
#if DEBUG
              DebugName = x.Name
#endif
              Value = value }

module AttributeDefinitionStore =
    let mutable private _scalarsCount = 0
    let mutable private _widgetsCount = 0
    let mutable private _widgetCollectionsCount = 0

    let getNextKeyForScalar () : ScalarAttributeKey =
        let key = _scalarsCount * 1<scalarAttributeKey>
        _scalarsCount <- _scalarsCount + 1
        key

    let getNextKeyForWidget () : WidgetAttributeKey =
        let key = _scalarsCount * 1<widgetAttributeKey>
        _scalarsCount <- _scalarsCount + 1
        key

    let getNextKeyForWidgetCollection () : WidgetCollectionAttributeKey =
        let key = _scalarsCount * 1<widgetCollectionAttributeKey>
        _scalarsCount <- _scalarsCount + 1
        key
