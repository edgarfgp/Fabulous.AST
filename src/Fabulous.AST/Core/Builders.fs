namespace Fabulous.AST

open System.ComponentModel
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
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
              Attributes = AttributesBundle(StackList.two (scalarA, scalarB), ValueNone, ValueNone) }

        new(key: WidgetKey, scalar1: ScalarAttribute, scalar2: ScalarAttribute, scalar3: ScalarAttribute) =
            { Key = key
              Attributes = AttributesBundle(StackList.three (scalar1, scalar2, scalar3), ValueNone, ValueNone) }

        member x.Compile() : Widget =
            let struct (scalarAttributes, widgetAttributes, widgetCollectionAttributes) =
                x.Attributes

            { Key = x.Key
              ScalarAttributes =
                match StackList.length &scalarAttributes with
                | 0us -> ValueNone
                | _ -> ValueSome(Array.sortInPlace (fun a -> a.Key) (StackList.toArray &scalarAttributes))

              WidgetAttributes = ValueOption.map (Array.sortInPlace (fun a -> a.Key)) widgetAttributes


              WidgetCollectionAttributes =
                  widgetCollectionAttributes
                  |> ValueOption.map (Array.sortInPlace (fun a -> a.Key)) }

        [<EditorBrowsable(EditorBrowsableState.Never)>]
        member inline x.AddScalar(attr: ScalarAttribute) =
            let struct (scalarAttributes, widgetAttributes, widgetCollectionAttributes) =
                x.Attributes

            WidgetBuilder<'marker>(
                x.Key,
                struct (StackList.add (&scalarAttributes, attr), widgetAttributes, widgetCollectionAttributes)
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

            match StackList.tryFind (&scalarAttributes, (fun attr -> attr.Key = attrKey)) with
            | ValueNone ->
                let attr = defaultWith ()

                WidgetBuilder<'marker>(
                    x.Key,
                    struct (StackList.add (&scalarAttributes, attr), widgetAttributes, widgetCollectionAttributes)
                )

            | ValueSome attr ->
                let newAttr = replaceWith attr

                let newAttrs =
                    StackList.replace (&scalarAttributes, (fun attr -> attr.Key = attrKey), newAttr)

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
                    let attribs2 = Array.zeroCreate (attribs.Length + 1)
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
                    let attribs2 = Array.zeroCreate (attribs.Length + 1)
                    Array.blit attribs 0 attribs2 0 attribs.Length
                    attribs2.[attribs.Length] <- attr
                    attribs2

            WidgetBuilder<'marker>(x.Key, struct (scalarAttributes, widgetAttributes, ValueSome res))
    end


[<Struct>]
type Content = { Widgets: MutStackArray1.T<Widget> }

[<Struct; NoComparison; NoEquality>]
type CollectionBuilder<'marker, 'itemMarker> =
    struct
        val WidgetKey: WidgetKey
        val Scalars: StackList<ScalarAttribute>
        val Widgets: WidgetAttribute[] voption
        val Attr: WidgetCollectionAttributeDefinition

        new(widgetKey: WidgetKey,
            scalars: StackList<ScalarAttribute>,
            widgets: WidgetAttribute[] voption,
            attr: WidgetCollectionAttributeDefinition) =
            { WidgetKey = widgetKey
              Scalars = scalars
              Widgets = widgets
              Attr = attr }

        new(widgetKey: WidgetKey, attr: WidgetCollectionAttributeDefinition) =
            { WidgetKey = widgetKey
              Scalars = StackList.empty ()
              Widgets = ValueNone
              Attr = attr }

        new(widgetKey: WidgetKey, attr: WidgetCollectionAttributeDefinition, scalar: ScalarAttribute) =
            { WidgetKey = widgetKey
              Scalars = StackList.one scalar
              Widgets = ValueNone
              Attr = attr }

        new(widgetKey: WidgetKey,
            attr: WidgetCollectionAttributeDefinition,
            scalarA: ScalarAttribute,
            scalarB: ScalarAttribute) =
            { WidgetKey = widgetKey
              Scalars = StackList.two (scalarA, scalarB)
              Widgets = ValueNone
              Attr = attr }

        member inline x.Run(c: Content) =
            let attrValue =
                match MutStackArray1.toArraySlice &c.Widgets with
                | ValueNone -> ArraySlice.emptyWithNull ()
                | ValueSome slice -> slice

            WidgetBuilder<'marker>(
                x.WidgetKey,
                AttributesBundle(x.Scalars, x.Widgets, ValueSome [| x.Attr.WithValue(attrValue) |])
            )

        member inline _.Combine(a: Content, b: Content) : Content =
            let res = MutStackArray1.combineMut (&a.Widgets, b.Widgets)

            { Widgets = res }

        member inline _.Zero() : Content = { Widgets = MutStackArray1.Empty }

        member inline _.Delay([<InlineIfLambda>] f) : Content = f ()

        member inline x.For<'t>(sequence: 't seq, f: 't -> Content) : Content =
            let mutable res: Content = x.Zero()

            // this is essentially Fold, not sure what is more optimal
            // handwritten version of via Seq.Fold
            for t in sequence do
                res <- x.Combine(res, f t)

            res
    end

[<Struct>]
type AttributeCollectionBuilder<'msg, 'marker, 'itemMarker> =
    struct
        val Widget: WidgetBuilder<'marker>
        val Attr: WidgetCollectionAttributeDefinition

        new(widget: WidgetBuilder<'marker>, attr: WidgetCollectionAttributeDefinition) =
            { Widget = widget; Attr = attr }

        member inline x.Run(c: Content) =
            let attrValue =
                match MutStackArray1.toArraySlice &c.Widgets with
                | ValueNone -> ArraySlice.emptyWithNull ()
                | ValueSome slice -> slice

            x.Widget.AddWidgetCollection(x.Attr.WithValue(attrValue))

        member inline _.Combine(a: Content, b: Content) : Content =
            { Widgets = MutStackArray1.combineMut (&a.Widgets, b.Widgets) }

        member inline _.Zero() : Content = { Widgets = MutStackArray1.Empty }

        member inline _.Delay([<InlineIfLambda>] f) : Content = f ()

        member inline x.For<'t>(sequence: 't seq, [<InlineIfLambda>] f: 't -> Content) : Content =
            let mutable res: Content = x.Zero()

            // this is essentially Fold, not sure what is more optimal
            // handwritten version of via Seq.Fold
            for t in sequence do
                res <- x.Combine(res, f t)

            res
    end
