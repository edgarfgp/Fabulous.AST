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

            let members =
                Widgets.tryGetNodesFromWidgetCollection widget TypeDefn.Members
                |> ValueOption.defaultValue []

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget TypeDefn.XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget TypeDefn.MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let typeParams =
                Widgets.tryGetNodeFromWidget widget TypeDefn.TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

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
                | Unknown -> None

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
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: TypeDefn) : CollectionContent =
        let moduleDecl = ModuleDecl.TypeDefn(x)
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefn>)
        : CollectionContent =
        let node = Gen.mkOak x
        TypeDefnYieldExtensions.Yield(this, node)
