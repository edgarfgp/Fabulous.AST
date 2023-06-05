namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections

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
                | ValueSome values -> TypeHelpers.createAttributes values |> Some
                | ValueNone -> None

            TypeDefnRecordNode(
                TypeNameNode(
                    None,
                    multipleAttributes,
                    SingleTextNode("type", Range.Zero),
                    Some(name),
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode("=", Range.Zero)) ], Range.Zero),
                    None,
                    [],
                    None,
                    None,
                    None,
                    Range.Zero
                ),
                None,
                SingleTextNode("{", Range.Zero),
                fields,
                SingleTextNode("}", Range.Zero),
                members,
                Range.Zero
            ))

[<AutoOpen>]
module RecordBuilders =
    type Ast with

        static member inline Record(name: WidgetBuilder<#SingleTextNode>) =
            CollectionBuilder<TypeDefnRecordNode, FieldNode>(
                Record.WidgetKey,
                Record.RecordCaseNode,
                AttributesBundle(StackList.empty(), ValueSome [| Record.Name.WithValue(name.Compile()) |], ValueNone)
            )

        static member inline Record(node: SingleTextNode) = Ast.Record(Ast.EscapeHatch(node))

        static member inline Record(name: string) =
            Ast.Record(SingleTextNode(name, Range.Zero))

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
