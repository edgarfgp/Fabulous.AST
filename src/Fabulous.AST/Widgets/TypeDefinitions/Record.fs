namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections

type RecordTypeDefnRecordNode
    (
        name: SingleTextNode,
        multipleAttributes: MultipleAttributeListNode option,
        fields: FieldNode list,
        members: MemberDefn list
    ) =
    inherit
        TypeDefnRecordNode(
            TypeNameNode(
                None,
                multipleAttributes,
                SingleTextNode.``type``,
                None,
                IdentListNode([ IdentifierOrDot.Ident(name) ], Range.Zero),
                None,
                [],
                None,
                Some(SingleTextNode.equals),
                None,
                Range.Zero
            ),
            None,
            SingleTextNode.leftCurlyBrace,
            fields,
            SingleTextNode.rightCurlyBrace,
            members,
            Range.Zero
        )

module Record =

    let RecordCaseNode = Attributes.defineWidgetCollection "RecordCaseNode"

    let Name = Attributes.defineWidget "Name"

    let Members = Attributes.defineWidgetCollection "Members"

    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"

    let WidgetKey =
        Widgets.register "Record" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name
            let fields = Helpers.getNodesFromWidgetCollection<FieldNode> widget RecordCaseNode
            let members = Helpers.tryGetNodesFromWidgetCollection<MemberDefn> widget Members

            let members =
                match members with
                | Some members -> members
                | None -> []

            let attributes = Helpers.tryGetScalarValue widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | ValueSome values -> MultipleAttributeListNode.Create values |> Some
                | ValueNone -> None

            RecordTypeDefnRecordNode(name, multipleAttributes, fields, members))

[<AutoOpen>]
module RecordBuilders =
    type Ast with

        static member inline Record(name: WidgetBuilder<#SingleTextNode>) =
            CollectionBuilder<RecordTypeDefnRecordNode, FieldNode>(
                Record.WidgetKey,
                Record.RecordCaseNode,
                AttributesBundle(StackList.empty(), ValueSome [| Record.Name.WithValue(name.Compile()) |], ValueNone)
            )

        static member inline Record(name: SingleTextNode) = Ast.Record(Ast.EscapeHatch(name))

        static member inline Record(name: string) =
            Ast.Record(SingleTextNode(name, Range.Zero))

[<Extension>]
type RecordModifiers =
    [<Extension>]
    static member inline members(this: WidgetBuilder<RecordTypeDefnRecordNode>) =
        AttributeCollectionBuilder<RecordTypeDefnRecordNode, MemberDefn>(this, Record.Members)

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<RecordTypeDefnRecordNode>, attributes: string list) =
        this.AddScalar(Record.MultipleAttributes.WithValue(attributes))

    [<Extension>]
    static member inline isStruct(this: WidgetBuilder<RecordTypeDefnRecordNode>) =
        RecordModifiers.attributes(this, [ "Struct" ])

[<Extension>]
type RecordYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<RecordTypeDefnRecordNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let typeDefn = TypeDefn.Record(node)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<RecordTypeDefnRecordNode, FieldNode>,
            x: FieldNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(x).Compile()
        { Widgets = MutStackArray1.One(widget) }


    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<RecordTypeDefnRecordNode, MemberDefn>,
            x: MethodMemberNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            this: CollectionBuilder<RecordTypeDefnRecordNode, MemberDefn>,
            x: WidgetBuilder<MethodMemberNode>
        ) : CollectionContent =
        let node = Tree.compile x
        RecordYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<RecordTypeDefnRecordNode, MemberDefn>,
            x: PropertyMemberNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            this: CollectionBuilder<RecordTypeDefnRecordNode, MemberDefn>,
            x: WidgetBuilder<PropertyMemberNode>
        ) : CollectionContent =
        let node = Tree.compile x
        RecordYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (
            _: AttributeCollectionBuilder<RecordTypeDefnRecordNode, MemberDefn>,
            x: WidgetBuilder<PropertyMemberNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let widget = Ast.EscapeHatch(MemberDefn.Member(node)).Compile()
        { Widgets = MutStackArray1.One(widget) }
