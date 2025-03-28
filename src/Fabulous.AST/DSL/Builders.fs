namespace Fabulous.AST

open System.ComponentModel
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fabulous.AST.WidgetAttributeDefinitions
open Fabulous.AST.WidgetCollectionAttributeDefinitions

type AttributesBundle = (struct (StackList<ScalarAttribute> * WidgetAttribute array * WidgetCollectionAttribute array))

[<Struct; NoComparison; NoEquality>]
type WidgetBuilder<'marker> =
    struct
        val Key: WidgetKey
        val Attributes: AttributesBundle

        new(key: WidgetKey) =
            { Key = key
              Attributes = AttributesBundle(StackList.empty(), Array.empty, Array.empty) }

        new(key: WidgetKey, attributes: AttributesBundle) =
            { Key = key
              Attributes = attributes }

        new(key: WidgetKey, scalar: ScalarAttribute) =
            { Key = key
              Attributes = AttributesBundle(StackList.one scalar, Array.empty, Array.empty) }

        new(key: WidgetKey, scalarA: ScalarAttribute, scalarB: ScalarAttribute) =
            { Key = key
              Attributes = AttributesBundle(StackList.two(scalarA, scalarB), Array.empty, Array.empty) }

        new(key: WidgetKey, scalar1: ScalarAttribute, scalar2: ScalarAttribute, scalar3: ScalarAttribute) =
            { Key = key
              Attributes = AttributesBundle(StackList.three(scalar1, scalar2, scalar3), Array.empty, Array.empty) }

        new(key: WidgetKey, widget: WidgetAttribute) =
            { Key = key
              Attributes = AttributesBundle(StackList.empty(), [| widget |], Array.empty) }

        member x.Compile() : Widget =
            let struct (scalarAttributes, widgetAttributes, widgetCollectionAttributes) =
                x.Attributes

            { Key = x.Key
              ScalarAttributes =
                match StackList.length &scalarAttributes with
                | 0us -> Array.empty
                | _ -> Array.sortInPlace (fun a -> a.Key) (StackList.toArray &scalarAttributes)

              WidgetAttributes = Array.sortInPlace (fun a -> a.Key) widgetAttributes

              WidgetCollectionAttributes = widgetCollectionAttributes |> Array.sortInPlace(fun a -> a.Key) }

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
                | [||] -> [| attr |]
                | attribs ->
                    let attribs2 = Array.zeroCreate(attribs.Length + 1)
                    Array.blit attribs 0 attribs2 0 attribs.Length
                    attribs2[attribs.Length] <- attr
                    attribs2

            WidgetBuilder<'marker>(x.Key, struct (scalarAttributes, res, widgetCollectionAttributes))

        [<EditorBrowsable(EditorBrowsableState.Never)>]
        member x.AddWidgetCollection(attr: WidgetCollectionAttribute) =
            let struct (scalarAttributes, widgetAttributes, widgetCollectionAttributes) =
                x.Attributes

            let attribs = widgetCollectionAttributes

            let res =
                match attribs with
                | [||] -> [| attr |]
                | attribs ->
                    let attribs2 = Array.zeroCreate(attribs.Length + 1)
                    Array.blit attribs 0 attribs2 0 attribs.Length
                    attribs2[attribs.Length] <- attr
                    attribs2

            WidgetBuilder<'marker>(x.Key, struct (scalarAttributes, widgetAttributes, res))
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

        new(widgetKey: WidgetKey, scalars: StackList<ScalarAttribute>, attr: WidgetCollectionAttributeDefinition) =
            { Key = widgetKey
              Attributes = AttributesBundle(scalars, Array.empty, Array.empty)
              Attr = attr }

        new(key: WidgetKey, attr: WidgetCollectionAttributeDefinition, attributes: AttributesBundle) =
            { Key = key
              Attributes = attributes
              Attr = attr }

        new(key: WidgetKey, attr: WidgetCollectionAttributeDefinition) =
            { Key = key
              Attributes = AttributesBundle(StackList.empty(), Array.empty, Array.empty)
              Attr = attr }

        new(key: WidgetKey, attr: WidgetCollectionAttributeDefinition, scalar: ScalarAttribute) =
            { Key = key
              Attributes = AttributesBundle(StackList.one scalar, Array.empty, Array.empty)
              Attr = attr }

        new(key: WidgetKey, attr: WidgetCollectionAttributeDefinition, scalar: WidgetAttribute) =
            { Key = key
              Attributes = AttributesBundle(StackList.empty(), [| scalar |], Array.empty)
              Attr = attr }

        new
            (
                key: WidgetKey,
                attr: WidgetCollectionAttributeDefinition,
                scalarA: ScalarAttribute,
                scalarB: ScalarAttribute
            ) =
            { Key = key
              Attributes = AttributesBundle(StackList.two(scalarA, scalarB), Array.empty, Array.empty)
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
                | [||] -> [| widgetCollAttr |]
                | widgetCollections -> Array.append [| widgetCollAttr |] widgetCollections

            WidgetBuilder<'marker>(x.Key, AttributesBundle(scalars, widgets, widgetCollections))

        member inline _.Yield(widget: WidgetBuilder<'itemMarker>) : CollectionContent =
            { Widgets = MutStackArray1.One(widget.Compile()) }

        member inline _.YieldFrom(widgets: WidgetBuilder<'itemMarker> seq) : CollectionContent =
            { Widgets =
                widgets
                |> Seq.map(fun widget -> widget.Compile())
                |> Seq.toArray
                |> MutStackArray1.fromArray }

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
type AttributeCollectionBuilder<'marker, 'itemMarker> =
    struct
        val Widget: WidgetBuilder<'marker>
        val Attr: WidgetCollectionAttributeDefinition

        new(widget: WidgetBuilder<'marker>, attr: WidgetCollectionAttributeDefinition) =
            { Widget = widget
              Attr = attr }

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

        member inline _.Yield(widget: WidgetBuilder<'itemMarker>) : CollectionContent =
            { Widgets = MutStackArray1.One(widget.Compile()) }

        member inline _.YieldFrom(widgets: WidgetBuilder<'itemMarker> seq) : CollectionContent =
            { Widgets =
                widgets
                |> Seq.map(fun widget -> widget.Compile())
                |> Seq.toArray
                |> MutStackArray1.fromArray }
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
              Attributes = AttributesBundle(StackList.empty(), Array.empty, Array.empty)
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
                | [||] -> [| widgetAttr |]
                | widgets -> Array.append [| widgetAttr |] widgets

            WidgetBuilder<'marker>(x.Key, AttributesBundle(scalars, widgets, widgetCollections))

        member inline x.AddScalar(attr: ScalarAttribute) =
            let struct (scalarAttributes, widgetAttributes, widgetCollectionAttributes) =
                x.Attributes

            SingleChildBuilder<'marker, 'childMarker>(
                x.Key,
                x.Attr,
                struct (StackList.add(&scalarAttributes, attr), widgetAttributes, widgetCollectionAttributes)
            )
    end
