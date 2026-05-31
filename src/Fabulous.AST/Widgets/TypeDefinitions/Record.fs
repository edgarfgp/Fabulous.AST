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

    let WidgetKey =
        Widgets.register "Record" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

            let fields = Widgets.getNodesFromWidgetCollection<FieldNode> widget RecordCaseNode

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

            let fields =
                fields
                |> List.map(fun field ->
                    match field.Accessibility with
                    | None -> field
                    | Some _ ->
                        FieldNode(
                            field.XmlDoc,
                            field.Attributes,
                            field.LeadingKeyword,
                            field.MutableKeyword,
                            None,
                            field.Name,
                            field.Type,
                            Range.Zero
                        ))

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

            let accessControl =
                Widgets.tryGetScalarValue widget TypeDefn.Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let isRecursive =
                Widgets.tryGetScalarValue widget TypeDefn.IsRecursive
                |> ValueOption.map(fun x ->
                    if x then
                        SingleTextNode.``and``
                    else
                        SingleTextNode.``type``)
                |> ValueOption.defaultValue SingleTextNode.``type``

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> fieldAccessibility

            TypeDefn.Record(
                TypeDefnRecordNode(
                    TypeNameNode(
                        xmlDocs,
                        attributes,
                        isRecursive,
                        None,
                        IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero),
                        typeParams,
                        [],
                        None,
                        Some(SingleTextNode.equals),
                        None,
                        Range.Zero
                    ),
                    accessControl,
                    SingleTextNode.leftCurlyBrace,
                    fields,
                    SingleTextNode.rightCurlyBrace,
                    members,
                    Range.Zero
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
