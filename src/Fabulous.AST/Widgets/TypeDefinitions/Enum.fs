namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module Enum =

    let EnumCaseNode = Attributes.defineWidgetCollection "UnionCaseNode"

    let Name = Attributes.defineWidget "Name"

    let WidgetKey =
        Widgets.register "Enum" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name

            let enumCaseNodes =
                Helpers.getNodesFromWidgetCollection<EnumCaseNode> widget EnumCaseNode

            TypeDefnEnumNode(
                TypeNameNode(
                    None,
                    None,
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
