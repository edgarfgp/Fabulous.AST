namespace Fabulous.AST

open System.ComponentModel
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fabulous.AST.WidgetAttributeDefinitions
open Fabulous.AST.WidgetCollectionAttributeDefinitions

type AttributesBundle =
    (struct (StackList<ScalarAttribute> * WidgetAttribute[] voption * WidgetCollectionAttribute[] voption))

[<Struct; NoComparison; NoEquality>]
type WidgetBuilder<'marker> =
    struct
        val Key: WidgetKey
        val Attributes: AttributesBundle

        new(key: WidgetKey, attributes: AttributesBundle) = { Key = key; Attributes = attributes }

        new(key: WidgetKey, scalar: ScalarAttribute) =
            { Key = key
              Attributes = AttributesBundle(StackList.one scalar, ValueNone, ValueNone) }

        new(key: WidgetKey, scalarA: ScalarAttribute, scalarB: ScalarAttribute) =
            { Key = key
              Attributes = AttributesBundle(StackList.two(scalarA, scalarB), ValueNone, ValueNone) }

        new(key: WidgetKey, scalar1: ScalarAttribute, scalar2: ScalarAttribute, scalar3: ScalarAttribute) =
            { Key = key
              Attributes = AttributesBundle(StackList.three(scalar1, scalar2, scalar3), ValueNone, ValueNone) }

        member x.Compile() : Widget =
            let struct (scalarAttributes, widgetAttributes, widgetCollectionAttributes) =
                x.Attributes

            { Key = x.Key
              ScalarAttributes =
                match StackList.length &scalarAttributes with
                | 0us -> ValueNone
                | _ -> ValueSome(Array.sortInPlace (fun a -> a.Key) (StackList.toArray &scalarAttributes))

              WidgetAttributes = ValueOption.map (Array.sortInPlace(fun a -> a.Key)) widgetAttributes


              WidgetCollectionAttributes =
                widgetCollectionAttributes |> ValueOption.map(Array.sortInPlace(fun a -> a.Key)) }

        [<EditorBrowsable(EditorBrowsableState.Never)>]
        member inline x.AddScalar(attr: ScalarAttribute) =
            let struct (scalarAttributes, widgetAttributes, widgetCollectionAttributes) =
                x.Attributes

            WidgetBuilder<'marker>(
                x.Key,
                struct (StackList.add(&scalarAttributes, attr), widgetAttributes, widgetCollectionAttributes)
            )

        [<EditorBrowsable(EditorBrowsableState.Never)>]
        member inline x.AddOrReplaceScalar
            (
                attrKey: ScalarAttributeKey,
                [<InlineIfLambda>] replaceWith: ScalarAttribute -> ScalarAttribute,
                [<InlineIfLambda>] defaultWith: unit -> ScalarAttribute
            ) =
            let struct (scalarAttributes, widgetAttributes, widgetCollectionAttributes) =
                x.Attributes

            match StackList.tryFind(&scalarAttributes, (fun attr -> attr.Key = attrKey)) with
            | ValueNone ->
                let attr = defaultWith()

                WidgetBuilder<'marker>(
                    x.Key,
                    struct (StackList.add(&scalarAttributes, attr), widgetAttributes, widgetCollectionAttributes)
                )

            | ValueSome attr ->
                let newAttr = replaceWith attr

                let newAttrs =
                    StackList.replace(&scalarAttributes, (fun attr -> attr.Key = attrKey), newAttr)

                WidgetBuilder<'marker>(x.Key, struct (newAttrs, widgetAttributes, widgetCollectionAttributes))

        [<EditorBrowsable(EditorBrowsableState.Never)>]
        member x.AddWidget(attr: WidgetAttribute) =
            let struct (scalarAttributes, widgetAttributes, widgetCollectionAttributes) =
                x.Attributes

            let attribs = widgetAttributes

            let res =
                match attribs with
                | ValueNone -> [| attr |]
                | ValueSome attribs ->
                    let attribs2 = Array.zeroCreate(attribs.Length + 1)
                    Array.blit attribs 0 attribs2 0 attribs.Length
                    attribs2.[attribs.Length] <- attr
                    attribs2

            WidgetBuilder<'marker>(x.Key, struct (scalarAttributes, ValueSome res, widgetCollectionAttributes))

        [<EditorBrowsable(EditorBrowsableState.Never)>]
        member x.AddWidgetCollection(attr: WidgetCollectionAttribute) =
            let struct (scalarAttributes, widgetAttributes, widgetCollectionAttributes) =
                x.Attributes

            let attribs = widgetCollectionAttributes

            let res =
                match attribs with
                | ValueNone -> [| attr |]
                | ValueSome attribs ->
                    let attribs2 = Array.zeroCreate(attribs.Length + 1)
                    Array.blit attribs 0 attribs2 0 attribs.Length
                    attribs2.[attribs.Length] <- attr
                    attribs2

            WidgetBuilder<'marker>(x.Key, struct (scalarAttributes, widgetAttributes, ValueSome res))
    end



[<Struct>]
type SingleChildContent = { Child: Widget }

/// A Computation Expression builder accepting a single child widget.
[<Struct; NoComparison; NoEquality>]
type SingleChildBuilder<'marker, 'childMarker> =
    struct
        val Key: WidgetKey
        val Attributes: AttributesBundle
        val Attr: WidgetAttributeDefinition

        new(key: WidgetKey, attr: WidgetAttributeDefinition, attributes: AttributesBundle) =
            { Key = key
              Attributes = attributes
              Attr = attr }

        new(key: WidgetKey, attr: WidgetAttributeDefinition) =
            { Key = key
              Attributes = AttributesBundle(StackList.empty(), ValueNone, ValueNone)
              Attr = attr }

        /// Starts with a state equals to unit so we can force yield to only be used once.
        member inline _.Zero() = ()

        member inline _.Yield(child: WidgetBuilder<'childMarker>) = { Child = child.Compile() }

        /// Combines only the Zero state with the first child.
        /// Subsequent children won't be allowed thanks to the Unit initial state.
        member inline _.Combine(_: unit, b: SingleChildContent) = b

        member inline _.Delay([<InlineIfLambda>] f: unit -> SingleChildContent) = f()

        /// Creates the current widget using the accumulated attributes and the child.
        member inline x.Run(c: SingleChildContent) =
            let struct (scalars, widgets, widgetCollections) = x.Attributes

            let widgetAttr = x.Attr.WithValue(c.Child)

            let widgets =
                match widgets with
                | ValueNone -> [| widgetAttr |]
                | ValueSome widgets -> Array.append [| widgetAttr |] widgets

            WidgetBuilder<'marker>(x.Key, AttributesBundle(scalars, ValueSome widgets, widgetCollections))

        member inline x.AddScalar(attr: ScalarAttribute) =
            let struct (scalarAttributes, widgetAttributes, widgetCollectionAttributes) =
                x.Attributes

            SingleChildBuilder<'marker, 'childMarker>(
                x.Key,
                x.Attr,
                struct (StackList.add(&scalarAttributes, attr), widgetAttributes, widgetCollectionAttributes)
            )
    end





[<Struct>]
type CollectionContent = { Widgets: MutStackArray1.T<Widget> }

/// A Computation Expression builder accepting a collection of child widgets.
[<Struct; NoComparison; NoEquality>]
type CollectionBuilder<'marker, 'itemMarker> =
    struct
        val Key: WidgetKey
        val Attributes: AttributesBundle
        val Attr: WidgetCollectionAttributeDefinition

        new(key: WidgetKey, attr: WidgetCollectionAttributeDefinition, attributes: AttributesBundle) =
            { Key = key
              Attributes = attributes
              Attr = attr }

        new(key: WidgetKey, attr: WidgetCollectionAttributeDefinition) =
            { Key = key
              Attributes = AttributesBundle(StackList.empty(), ValueNone, ValueNone)
              Attr = attr }

        new(key: WidgetKey, attr: WidgetCollectionAttributeDefinition, scalar: ScalarAttribute) =
            { Key = key
              Attributes = AttributesBundle(StackList.one scalar, ValueNone, ValueNone)
              Attr = attr }

        new(key: WidgetKey,
            attr: WidgetCollectionAttributeDefinition,
            scalarA: ScalarAttribute,
            scalarB: ScalarAttribute) =
            { Key = key
              Attributes = AttributesBundle(StackList.two(scalarA, scalarB), ValueNone, ValueNone)
              Attr = attr }

        member inline x.Run(c: CollectionContent) =
            let struct (scalars, widgets, widgetCollections) = x.Attributes

            let attrValue =
                match MutStackArray1.toArraySlice &c.Widgets with
                | ValueNone -> ArraySlice.emptyWithNull()
                | ValueSome slice -> slice

            let widgetCollAttr = x.Attr.WithValue(attrValue)

            let widgetCollections =
                match widgetCollections with
                | ValueNone -> [| widgetCollAttr |]
                | ValueSome widgetCollections -> Array.append [| widgetCollAttr |] widgetCollections

            WidgetBuilder<'marker>(x.Key, AttributesBundle(scalars, widgets, ValueSome widgetCollections))

        member inline _.Combine(a: CollectionContent, b: CollectionContent) : CollectionContent =
            let res = MutStackArray1.combineMut(&a.Widgets, b.Widgets)

            { Widgets = res }

        member inline _.Zero() : CollectionContent = { Widgets = MutStackArray1.Empty }

        member inline _.Delay([<InlineIfLambda>] f) : CollectionContent = f()

        member inline x.For<'t>(sequence: 't seq, f: 't -> CollectionContent) : CollectionContent =
            let mutable res: CollectionContent = x.Zero()

            // this is essentially Fold, not sure what is more optimal
            // handwritten version of via Seq.Fold
            for t in sequence do
                res <- x.Combine(res, f t)

            res




        member inline x.AddScalar(attr: ScalarAttribute) =
            let struct (scalarAttributes, widgetAttributes, widgetCollectionAttributes) =
                x.Attributes

            CollectionBuilder<'marker, 'childMarker>(
                x.Key,
                x.Attr,
                struct (StackList.add(&scalarAttributes, attr), widgetAttributes, widgetCollectionAttributes)
            )
    end

[<Struct>]
type AttributeCollectionBuilder<'msg, 'marker, 'itemMarker> =
    struct
        val Widget: WidgetBuilder<'marker>
        val Attr: WidgetCollectionAttributeDefinition

        new(widget: WidgetBuilder<'marker>, attr: WidgetCollectionAttributeDefinition) =
            { Widget = widget; Attr = attr }

        member inline x.Run(c: CollectionContent) =
            let attrValue =
                match MutStackArray1.toArraySlice &c.Widgets with
                | ValueNone -> ArraySlice.emptyWithNull()
                | ValueSome slice -> slice

            x.Widget.AddWidgetCollection(x.Attr.WithValue(attrValue))

        member inline _.Combine(a: CollectionContent, b: CollectionContent) : CollectionContent =
            { Widgets = MutStackArray1.combineMut(&a.Widgets, b.Widgets) }

        member inline _.Zero() : CollectionContent = { Widgets = MutStackArray1.Empty }

        member inline _.Delay([<InlineIfLambda>] f) : CollectionContent = f()

        member inline x.For<'t>(sequence: 't seq, [<InlineIfLambda>] f: 't -> CollectionContent) : CollectionContent =
            let mutable res: CollectionContent = x.Zero()

            // this is essentially Fold, not sure what is more optimal
            // handwritten version of via Seq.Fold
            for t in sequence do
                res <- x.Combine(res, f t)

            res
    end
