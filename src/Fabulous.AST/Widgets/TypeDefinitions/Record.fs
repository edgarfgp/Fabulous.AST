namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module Record =

    let RecordCaseNode = Attributes.defineWidgetCollection "RecordCaseNode"

    let Name = Attributes.defineScalar<string> "Name"

    let SpreadType = Attributes.defineWidget "SpreadType"

    let SpreadWidgetKey =
        Widgets.register "TypeSpread" (fun widget ->
            let t = Widgets.getNodeFromWidget<Type> widget SpreadType
            TypeSpreadNode(SingleTextNode.Create("..."), t, Range.range0))

    let WidgetKey =
        Widgets.register "Record" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

            // Fields and spreads (`...Base`), in source order.
            let items =
                Widgets.getNodesFromWidgetCollection<obj> widget RecordCaseNode
                |> List.map(fun item ->
                    match item with
                    | :? FieldNode as node -> TypeDefnRecordFieldOrSpread.Field node
                    | :? TypeSpreadNode as node -> TypeDefnRecordFieldOrSpread.Spread node
                    | other -> invalidArg "item" $"Records accept fields and spreads. Got: %s{other.GetType().Name}")

            let fields =
                items
                |> List.choose(fun item ->
                    match item with
                    | TypeDefnRecordFieldOrSpread.Field node -> Some node
                    | TypeDefnRecordFieldOrSpread.Spread _ -> None)

            // Accessibility modifiers are not permitted on individual record fields;
            // F# only allows accessibility on the whole representation. Lift any
            // field-level accessibility onto the record (most restrictive wins) and
            // strip it from the fields.
            let fieldAccessibility =
                fields
                |> List.choose(fun field -> field.Accessibility)
                |> List.sortBy(fun node ->
                    match node.Text with
                    | "private" -> 0
                    | "internal" -> 1
                    | _ -> 2)
                |> List.tryHead

            let items =
                items
                |> List.map(fun item ->
                    match item with
                    | TypeDefnRecordFieldOrSpread.Field field when field.Accessibility.IsSome ->
                        TypeDefnRecordFieldOrSpread.Field(
                            FieldNode(
                                field.XmlDoc,
                                field.Attributes,
                                field.LeadingKeyword,
                                field.MutableKeyword,
                                None,
                                field.Name,
                                field.Type,
                                Range.range0
                            )
                        )
                    | item -> item)

            let members =
                Widgets.tryGetNodesFromWidgetCollection widget TypeDefn.Members
                |> ValueOption.defaultValue []

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget TypeDefn.XmlDocs |> ValueOption.toOption

            let attributes =
                Widgets.tryGetScalarValue widget TypeDefn.MultipleAttributes
                |> ValueOption.map MultipleAttributeListNode.Create
                |> ValueOption.toOption

            let typeParams =
                Widgets.tryGetNodeFromWidget widget TypeDefn.TypeParams |> ValueOption.toOption

            let isRecursive =
                Widgets.tryGetScalarValue widget TypeDefn.IsRecursive
                |> ValueOption.map(fun x ->
                    if x then
                        SingleTextNode.``and``
                    else
                        SingleTextNode.``type``)
                |> ValueOption.defaultValue SingleTextNode.``type``

            let representationAccessibility =
                TypeDefn.representationAccessibility widget |> Option.orElse fieldAccessibility

            TypeDefn.Record(
                TypeDefnRecordNode(
                    TypeNameNode(
                        xmlDocs,
                        attributes,
                        isRecursive,
                        TypeDefn.accessibility widget,
                        Type.LongIdent(
                            IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.range0)
                        ),
                        typeParams,
                        [],
                        None,
                        Some(SingleTextNode.equals),
                        Range.range0
                    ),
                    representationAccessibility,
                    SingleTextNode.leftCurlyBrace,
                    items,
                    SingleTextNode.rightCurlyBrace,
                    None,
                    members,
                    Range.range0
                )
            ))

[<AutoOpen>]
module RecordBuilders =
    type Ast with
        /// <summary>Create a record type with the given name.</summary>
        /// <param name="name">The name of the record type.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Record("Point") {
        ///             Field("X", Float())
        ///             Field("Y", Float())
        ///             Field("Z", Float())
        ///         }
        ///     }
        /// }
        /// </code>
        static member Record(name: string) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name

            CollectionBuilder<TypeDefn, FieldNode>(Record.WidgetKey, Record.RecordCaseNode, Record.Name.WithValue(name))

        /// <summary>
        /// Create a spread (<c>...Base</c>) that copies the fields of an existing record type into a record
        /// definition. Yield it in the body of <c>Record(...)</c>. Record spreads need F# 11 or later.
        /// </summary>
        /// <param name="value">The record type whose fields are copied.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Record("Point3D") {
        ///             Spread(LongIdent("Point"))
        ///             Field("Z", Float())
        ///         }
        ///     }
        /// }
        /// </code>
        static member Spread(value: WidgetBuilder<Type>) =
            WidgetBuilder<TypeSpreadNode>(Record.SpreadWidgetKey, Record.SpreadType.WithValue(value.Compile()))

        /// <summary>
        /// Create a spread (<c>...Base</c>) of the named record type. Record spreads need F# 11 or later.
        /// </summary>
        /// <param name="value">The record type whose fields are copied.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Record("Point3D") {
        ///             Spread("Point")
        ///             Field("Z", Float())
        ///         }
        ///     }
        /// }
        /// </code>
        static member Spread(value: string) = Ast.Spread(Ast.LongIdent(value))

type RecordYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<TypeDefn, FieldNode>, x: WidgetBuilder<TypeSpreadNode>)
        : CollectionContent =
        { Widgets = MutStackArray1.One(x.Compile()) }

type TypeDefnYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefn>)
        : CollectionContent =
        let node = Gen.mkOak x
        let moduleDecl = ModuleDecl.TypeDefn(node)
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline YieldFrom
        (_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefn> seq)
        : CollectionContent =
        let widgets =
            x
            |> Seq.map(fun wb ->
                let node = Gen.mkOak wb
                let moduleDecl = ModuleDecl.TypeDefn(node)
                Ast.EscapeHatch(moduleDecl).Compile())
            |> Seq.toArray
            |> MutStackArray1.fromArray

        { Widgets = widgets }
