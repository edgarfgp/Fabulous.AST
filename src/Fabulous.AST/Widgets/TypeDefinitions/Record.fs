namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections

module Record =

    let RecordCaseNode = Attributes.defineWidgetCollection "RecordCaseNode"

    let Name = Attributes.defineScalar<string> "Name"

    let Members = Attributes.defineWidgetCollection "Members"

    let MultipleAttributes = Attributes.defineWidgetCollection "MultipleAttributes"

    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let TypeParams = Attributes.defineScalar<string list> "TypeParams"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let WidgetKey =
        Widgets.register "Record" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name
                |> Unquoted
                |> StringParsing.normalizeIdentifierBackticks

            let fields = Widgets.getNodesFromWidgetCollection<FieldNode> widget RecordCaseNode
            let members = Widgets.tryGetNodesFromWidgetCollection<MemberDefn> widget Members

            let members =
                match members with
                | Some members -> members
                | None -> []

            let lines = Widgets.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            let attributes =
                Widgets.tryGetNodesFromWidgetCollection<AttributeNode> widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | Some values ->
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
                | None -> None

            let typeParams = Widgets.tryGetScalarValue widget TypeParams

            let typeParams =
                match typeParams with
                | ValueSome values ->
                    TyparDeclsPostfixListNode(
                        SingleTextNode.lessThan,
                        [ for v in values do
                              TyparDeclNode(None, SingleTextNode.Create v, [], Range.Zero) ],
                        [],
                        SingleTextNode.greaterThan,
                        Range.Zero
                    )
                    |> TyparDecls.PostfixList
                    |> Some
                | ValueNone -> None

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
            CollectionBuilder<TypeDefnRecordNode, FieldNode>(
                Record.WidgetKey,
                Record.RecordCaseNode,
                AttributesBundle(StackList.one(Record.Name.WithValue(name)), Array.empty, Array.empty)
            )

[<Extension>]
type RecordModifiers =
    [<Extension>]
    static member inline members(this: WidgetBuilder<TypeDefnRecordNode>) =
        AttributeCollectionBuilder<TypeDefnRecordNode, MemberDefn>(this, Record.Members)

    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnRecordNode>, xmlDocs: string list) =
        this.AddScalar(Record.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<TypeDefnRecordNode>) =
        AttributeCollectionBuilder<TypeDefnRecordNode, AttributeNode>(this, Record.MultipleAttributes)

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<TypeDefnRecordNode>, attributes: string list) =
        AttributeCollectionBuilder<TypeDefnRecordNode, AttributeNode>(this, Record.MultipleAttributes) {
            for attribute in attributes do
                Ast.Attribute(attribute)
        }

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefnRecordNode>, attribute: WidgetBuilder<AttributeNode>) =
        AttributeCollectionBuilder<TypeDefnRecordNode, AttributeNode>(this, Record.MultipleAttributes) { attribute }

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefnRecordNode>, attribute: string) =
        AttributeCollectionBuilder<TypeDefnRecordNode, AttributeNode>(this, Record.MultipleAttributes) {
            Ast.Attribute(attribute)
        }

    [<Extension>]
    static member inline typeParams(this: WidgetBuilder<TypeDefnRecordNode>, value: string list) =
        this.AddScalar(Record.TypeParams.WithValue(value))

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<TypeDefnRecordNode>) =
        this.AddScalar(Record.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<TypeDefnRecordNode>) =
        this.AddScalar(Record.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<TypeDefnRecordNode>) =
        this.AddScalar(Record.Accessibility.WithValue(AccessControl.Internal))

[<Extension>]
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
        (_: AttributeCollectionBuilder<TypeDefnRecordNode, MemberDefn>, x: BindingNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: AttributeCollectionBuilder<TypeDefnRecordNode, MemberDefn>, x: WidgetBuilder<BindingNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        RecordYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<TypeDefnRecordNode, MemberDefn>, x: MemberDefnInterfaceNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Interface(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<TypeDefnRecordNode, MemberDefn>, x: WidgetBuilder<MemberDefnInterfaceNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        RecordYieldExtensions.Yield(this, node)

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
