namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections

type GenericRecordTypeDefnRecordNode
    (
        name: SingleTextNode,
        multipleAttributes: MultipleAttributeListNode option,
        fields: FieldNode list,
        members: MemberDefn list,
        typeParams: TyparDecls option
    ) =
    inherit
        TypeDefnRecordNode(
            TypeNameNode(
                None,
                multipleAttributes,
                SingleTextNode.``type``,
                None,
                IdentListNode([ IdentifierOrDot.Ident(name) ], Range.Zero),
                typeParams,
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

module GenericRecord =

    let RecordCaseNode = Attributes.defineWidgetCollection "RecordCaseNode"

    let Name = Attributes.defineWidget "Name"

    let Members = Attributes.defineWidgetCollection "Members"

    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"

    let TypeParams = Attributes.defineScalar<string list> "TypeParams"

    let WidgetKey =
        Widgets.register "GenericRecord" (fun widget ->
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

            let typeParams = Helpers.tryGetScalarValue widget TypeParams

            let typeParams =
                match typeParams with
                | ValueSome values ->
                    TyparDeclsPostfixListNode(
                        SingleTextNode.lessThan,
                        [ for v in values do
                              TyparDeclNode(None, SingleTextNode.Create v, Range.Zero) ],
                        [],
                        SingleTextNode.greaterThan,
                        Range.Zero
                    )
                    |> TyparDecls.PostfixList
                    |> Some
                | ValueNone -> None

            GenericRecordTypeDefnRecordNode(name, multipleAttributes, fields, members, typeParams))

[<AutoOpen>]
module GenericRecordBuilders =
    type Ast with

        static member inline GenericRecord(name: WidgetBuilder<#SingleTextNode>, typeParams: string list) =
            CollectionBuilder<GenericRecordTypeDefnRecordNode, FieldNode>(
                GenericRecord.WidgetKey,
                GenericRecord.RecordCaseNode,
                AttributesBundle(
                    StackList.one(GenericRecord.TypeParams.WithValue(typeParams)),
                    ValueSome [| GenericRecord.Name.WithValue(name.Compile()) |],
                    ValueNone
                )
            )

        static member inline GenericRecord(name: SingleTextNode, typeParams: string list) =
            Ast.GenericRecord(Ast.EscapeHatch(name), typeParams)

        static member inline GenericRecord(name: string, typeParams: string list) =
            Ast.GenericRecord(SingleTextNode(name, Range.Zero), typeParams)

[<Extension>]
type GenericRecordModifiers =
    [<Extension>]
    static member inline members(this: WidgetBuilder<GenericRecordTypeDefnRecordNode>) =
        AttributeCollectionBuilder<GenericRecordTypeDefnRecordNode, MemberDefn>(this, GenericRecord.Members)

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<GenericRecordTypeDefnRecordNode>, attributes: string list) =
        this.AddScalar(GenericRecord.MultipleAttributes.WithValue(attributes))

    [<Extension>]
    static member inline isStruct(this: WidgetBuilder<GenericRecordTypeDefnRecordNode>) =
        GenericRecordModifiers.attributes(this, [ "Struct" ])

[<Extension>]
type GenericRecordYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<GenericRecordTypeDefnRecordNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let typeDefn = TypeDefn.Record(node)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<GenericRecordTypeDefnRecordNode, FieldNode>,
            x: FieldNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(x).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<GenericRecordTypeDefnRecordNode, MemberDefn>,
            x: MethodMemberNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            this: CollectionBuilder<GenericRecordTypeDefnRecordNode, MemberDefn>,
            x: WidgetBuilder<MethodMemberNode>
        ) : CollectionContent =
        let node = Tree.compile x
        GenericRecordYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<GenericRecordTypeDefnRecordNode, MemberDefn>,
            x: PropertyMemberNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            this: CollectionBuilder<GenericRecordTypeDefnRecordNode, MemberDefn>,
            x: WidgetBuilder<PropertyMemberNode>
        ) : CollectionContent =
        let node = Tree.compile x
        GenericRecordYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (
            _: AttributeCollectionBuilder<GenericRecordTypeDefnRecordNode, MemberDefn>,
            x: WidgetBuilder<PropertyMemberNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let widget = Ast.EscapeHatch(MemberDefn.Member(node)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            _: AttributeCollectionBuilder<GenericRecordTypeDefnRecordNode, MemberDefn>,
            x: MethodMemberNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            this: AttributeCollectionBuilder<GenericRecordTypeDefnRecordNode, MemberDefn>,
            x: WidgetBuilder<MethodMemberNode>
        ) : CollectionContent =
        let node = Tree.compile x
        GenericRecordYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (
            _: AttributeCollectionBuilder<GenericRecordTypeDefnRecordNode, MemberDefn>,
            x: InterfaceMemberNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Interface(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            this: AttributeCollectionBuilder<GenericRecordTypeDefnRecordNode, MemberDefn>,
            x: WidgetBuilder<InterfaceMemberNode>
        ) : CollectionContent =
        let node = Tree.compile x
        GenericRecordYieldExtensions.Yield(this, node)
