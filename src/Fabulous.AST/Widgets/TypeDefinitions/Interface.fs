namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections

module Interface =
    let Name = Attributes.defineWidget "Name"
    let Parameters = Attributes.defineScalar<SimplePatNode list option> "Parameters"
    let Members = Attributes.defineWidgetCollection "Members"
    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"

    let WidgetKey =
        Widgets.register "Interface" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name

            let members =
                Helpers.tryGetNodesFromWidgetCollection<MemberDefnAbstractSlotNode> widget Members

            let attributes = Helpers.tryGetScalarValue widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | ValueSome values ->
                    MultipleAttributeListNode(
                        [ AttributeListNode(
                              SingleTextNode("[<", Range.Zero),
                              [ for v in values do
                                    AttributeNode(
                                        IdentListNode(
                                            [ IdentifierOrDot.Ident(SingleTextNode(v, Range.Zero)) ],
                                            Range.Zero
                                        ),
                                        None,
                                        None,
                                        Range.Zero
                                    ) ],
                              SingleTextNode(">]", Range.Zero),
                              Range.Zero
                          ) ],
                        Range.Zero
                    )
                    |> Some
                | ValueNone -> None

            let members =
                match members with
                | Some members ->
                    [ for item in members do
                          MemberDefn.AbstractSlot(item) ]
                | None -> []

            TypeDefnRegularNode(
                TypeNameNode(
                    None,
                    multipleAttributes,
                    SingleTextNode("type", Range.Zero),
                    Some(name),
                    IdentListNode([], Range.Zero),
                    None,
                    [],
                    None,
                    Some(SingleTextNode("=", Range.Zero)),
                    None,
                    Range.Zero
                ),
                members,
                Range.Zero
            ))

[<AutoOpen>]
module InterfaceBuilders =
    type Ast with

        static member inline Interface(name: WidgetBuilder<#SingleTextNode>) =
            CollectionBuilder<TypeDefnRegularNode, MemberDefnAbstractSlotNode>(
                Interface.WidgetKey,
                Interface.Members,
                AttributesBundle(StackList.empty(), ValueSome [| Interface.Name.WithValue(name.Compile()) |], ValueNone)
            )

        static member inline Interface(node: SingleTextNode) = Ast.Interface(Ast.EscapeHatch(node))

        static member inline Interface(name: string, ?parameters: SimplePatNode list) =
            Ast.Interface(SingleTextNode(name, Range.Zero))

[<Extension>]
type InterfaceModifiers =
    [<Extension>]
    static member inline attributes(this: WidgetBuilder<TypeDefnRegularNode>, attributes) =
        this.AddScalar(Class.MultipleAttributes.WithValue(attributes))

[<Extension>]
type InterfaceYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<TypeDefnRegularNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let typeDefn = TypeDefn.Regular(node)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<TypeDefnRegularNode, MemberDefnAbstractSlotNode>,
            x: MemberDefnAbstractSlotNode
        ) : CollectionContent =
        let x = MemberDefn.AbstractSlot(x)
        let widget = Ast.EscapeHatch(x).Compile()
        { Widgets = MutStackArray1.One(widget) }
