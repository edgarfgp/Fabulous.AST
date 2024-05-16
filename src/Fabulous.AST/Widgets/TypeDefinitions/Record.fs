namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections

module Record =

    let RecordCaseNode = Attributes.defineWidgetCollection "RecordCaseNode"

    let Name = Attributes.defineScalar<string> "Name"

    let Members = Attributes.defineWidgetCollection "Members"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let TypeParams = Attributes.defineWidget "TypeParams"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let WidgetKey =
        Widgets.register "Record" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

            let fields = Widgets.getNodesFromWidgetCollection<FieldNode> widget RecordCaseNode

            let members =
                Widgets.tryGetNodesFromWidgetCollection widget Members |> Option.defaultValue []

            let lines = Widgets.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            let attributes = Widgets.tryGetScalarValue widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | ValueSome values ->
                    Some(
                        MultipleAttributeListNode(
                            [ AttributeListNode(
                                  SingleTextNode.leftAttribute,
                                  values,
                                  SingleTextNode.rightAttribute,
                                  Range.Zero
                              ) ],
                            Range.Zero
                        )
                    )
                | ValueNone -> None

            let typeParams =
                Widgets.tryGetNodeFromWidget widget TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            TypeDefnRecordNode(
                TypeNameNode(
                    xmlDocs,
                    multipleAttributes,
                    SingleTextNode.``type``,
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
            ))

[<AutoOpen>]
module RecordBuilders =
    type Ast with
        static member Record(name: string) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name

            CollectionBuilder<TypeDefnRecordNode, FieldNode>(
                Record.WidgetKey,
                Record.RecordCaseNode,
                AttributesBundle(StackList.one(Record.Name.WithValue(name)), Array.empty, Array.empty)
            )

type RecordModifiers =
    [<Extension>]
    static member inline members(this: WidgetBuilder<TypeDefnRecordNode>) =
        AttributeCollectionBuilder<TypeDefnRecordNode, MemberDefn>(this, Record.Members)

    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnRecordNode>, xmlDocs: string list) =
        this.AddScalar(Record.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<TypeDefnRecordNode>, attributes: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            Record.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefnRecordNode>, attribute: WidgetBuilder<AttributeNode>) =
        RecordModifiers.attributes(this, [ attribute ])

    [<Extension>]
    static member inline typeParams(this: WidgetBuilder<TypeDefnRecordNode>, typeParams: WidgetBuilder<TyparDecls>) =
        this.AddWidget(Record.TypeParams.WithValue(typeParams.Compile()))

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<TypeDefnRecordNode>) =
        this.AddScalar(Record.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<TypeDefnRecordNode>) =
        this.AddScalar(Record.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<TypeDefnRecordNode>) =
        this.AddScalar(Record.Accessibility.WithValue(AccessControl.Internal))

type RecordYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefnRecordNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let typeDefn = TypeDefn.Record(node)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield(_: CollectionBuilder<TypeDefnRecordNode, FieldNode>, x: FieldNode) : CollectionContent =
        let widget = Ast.EscapeHatch(x).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (_: AttributeCollectionBuilder<TypeDefnRecordNode, MemberDefn>, x: WidgetBuilder<BindingNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let widget = Ast.EscapeHatch(MemberDefn.Member(node)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (_: AttributeCollectionBuilder<TypeDefnRecordNode, MemberDefn>, x: BindingNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (_: AttributeCollectionBuilder<TypeDefnRecordNode, MemberDefn>, x: MemberDefnInterfaceNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Interface(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: AttributeCollectionBuilder<TypeDefnRecordNode, MemberDefn>, x: WidgetBuilder<MemberDefnInterfaceNode>) : CollectionContent =
        let node = Gen.mkOak x
        RecordYieldExtensions.Yield(this, node)
