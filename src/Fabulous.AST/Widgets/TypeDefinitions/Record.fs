namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections

module Record =

    let RecordCaseNode = Attributes.defineWidgetCollection "RecordCaseNode"

    let Name = Attributes.defineWidget "Name"

    let Members = Attributes.defineWidgetCollection "Members"

    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"

    let TypeParams = Attributes.defineScalar<string list> "TypeParams"

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

            let typeParams = Helpers.tryGetScalarValue widget TypeParams

            let typeParams =
                match typeParams with
                | ValueSome values ->
                    TyparDeclsPostfixListNode(
                        SingleTextNode.lessThan,
                        [ for v in values do
                              // FIXME - Update
                              TyparDeclNode(None, SingleTextNode.Create v, [], Range.Zero) ],
                        [],
                        SingleTextNode.greaterThan,
                        Range.Zero
                    )
                    |> TyparDecls.PostfixList
                    |> Some
                | ValueNone -> None

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
            ))

[<AutoOpen>]
module RecordBuilders =
    type Ast with

        static member inline private BaseRecord(name: WidgetBuilder<#SingleTextNode>, typeParams: string list voption) =
            let scalars =
                match typeParams with
                | ValueNone -> StackList.empty()
                | ValueSome typeParams -> StackList.one(Record.TypeParams.WithValue(typeParams))

            CollectionBuilder<TypeDefnRecordNode, FieldNode>(
                Record.WidgetKey,
                Record.RecordCaseNode,
                AttributesBundle(scalars, ValueSome [| Record.Name.WithValue(name.Compile()) |], ValueNone)
            )

        static member inline Record(name: WidgetBuilder<#SingleTextNode>) = Ast.BaseRecord(name, ValueNone)

        static member inline Record(name: SingleTextNode) = Ast.Record(Ast.EscapeHatch(name))

        static member inline Record(name: string) = Ast.Record(SingleTextNode.Create(name))

        static member inline GenericRecord(name: WidgetBuilder<#SingleTextNode>, typeParams: string list) =
            Ast.BaseRecord(name, ValueSome typeParams)

        static member inline GenericRecord(name: SingleTextNode, typeParams: string list) =
            Ast.GenericRecord(Ast.EscapeHatch(name), typeParams)

        static member inline GenericRecord(name: string, typeParams: string list) =
            Ast.GenericRecord(SingleTextNode(name, Range.Zero), typeParams)

[<Extension>]
type RecordModifiers =
    [<Extension>]
    static member inline members(this: WidgetBuilder<TypeDefnRecordNode>) =
        AttributeCollectionBuilder<TypeDefnRecordNode, MemberDefn>(this, Record.Members)

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<TypeDefnRecordNode>, attributes: string list) =
        this.AddScalar(Record.MultipleAttributes.WithValue(attributes))

[<Extension>]
type RecordYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<TypeDefnRecordNode>
        ) : CollectionContent =
        let node = Tree.compile x
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
        (
            _: CollectionBuilder<TypeDefnRecordNode, MemberDefn>,
            x: PropertyMemberNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            this: CollectionBuilder<TypeDefnRecordNode, MemberDefn>,
            x: WidgetBuilder<PropertyMemberNode>
        ) : CollectionContent =
        let node = Tree.compile x
        RecordYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (
            _: AttributeCollectionBuilder<TypeDefnRecordNode, MemberDefn>,
            x: PropertyMemberNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            this: AttributeCollectionBuilder<TypeDefnRecordNode, MemberDefn>,
            x: WidgetBuilder<PropertyMemberNode>
        ) : CollectionContent =
        let node = Tree.compile x
        RecordYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<TypeDefnRecordNode, MemberDefn>,
            x: MethodMemberNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            this: CollectionBuilder<TypeDefnRecordNode, MemberDefn>,
            x: WidgetBuilder<MethodMemberNode>
        ) : CollectionContent =
        let node = Tree.compile x
        RecordYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (
            _: AttributeCollectionBuilder<TypeDefnRecordNode, MemberDefn>,
            x: MethodMemberNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            this: AttributeCollectionBuilder<TypeDefnRecordNode, MemberDefn>,
            x: WidgetBuilder<MethodMemberNode>
        ) : CollectionContent =
        let node = Tree.compile x
        RecordYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<TypeDefnRecordNode, MemberDefn>,
            x: MemberDefnInterfaceNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Interface(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            this: CollectionBuilder<TypeDefnRecordNode, MemberDefn>,
            x: WidgetBuilder<MemberDefnInterfaceNode>
        ) : CollectionContent =
        let node = Tree.compile x
        RecordYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (
            _: AttributeCollectionBuilder<TypeDefnRecordNode, MemberDefn>,
            x: MemberDefnInterfaceNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Interface(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            this: AttributeCollectionBuilder<TypeDefnRecordNode, MemberDefn>,
            x: WidgetBuilder<MemberDefnInterfaceNode>
        ) : CollectionContent =
        let node = Tree.compile x
        RecordYieldExtensions.Yield(this, node)
