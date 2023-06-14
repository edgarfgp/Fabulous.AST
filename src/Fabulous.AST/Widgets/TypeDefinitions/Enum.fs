namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module Enum =

    let EnumCaseNode = Attributes.defineWidgetCollection "UnionCaseNode"

    let Name = Attributes.defineWidget "Name"

    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"

    let WidgetKey =
        Widgets.register "Enum" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name

            let enumCaseNodes =
                Helpers.getNodesFromWidgetCollection<EnumCaseNode> widget EnumCaseNode

            let attributes = Helpers.tryGetScalarValue widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | ValueSome values -> MultipleAttributeListNode.Create values |> Some
                | ValueNone -> None

            TypeDefnEnumNode(
                TypeNameNode(
                    None,
                    multipleAttributes,
                    SingleTextNode.``type``,
                    Some(name),
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode("=", Range.Zero)) ], Range.Zero),
                    None,
                    [],
                    None,
                    None,
                    None,
                    Range.Zero
                ),
                enumCaseNodes,
                [],
                Range.Zero
            ))

[<AutoOpen>]
module EnumBuilders =
    type Fabulous.AST.Ast with

        static member inline Enum(name: WidgetBuilder<#SingleTextNode>) =
            CollectionBuilder<TypeDefnEnumNode, EnumCaseNode>(
                Enum.WidgetKey,
                Enum.EnumCaseNode,
                AttributesBundle(StackList.empty(), ValueSome [| Enum.Name.WithValue(name.Compile()) |], ValueNone)
            )

        static member inline Enum(node: SingleTextNode) = Ast.Enum(Ast.EscapeHatch(node))

        static member inline Enum(name: string) =
            Ast.Enum(SingleTextNode(name, Range.Zero))

[<Extension>]
type EnumModifiers =
    [<Extension>]
    static member inline attributes(this: WidgetBuilder<TypeDefnEnumNode>, attributes: string list) =
        this.AddScalar(Enum.MultipleAttributes.WithValue(attributes))

[<Extension>]
type EnumYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<TypeDefnEnumNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let typeDefn = TypeDefn.Enum(node)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }
