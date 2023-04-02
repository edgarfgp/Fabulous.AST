namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module Union =

    let UnionCaseNode = Attributes.defineWidgetCollection "UnionCaseNode"

    let Name = Attributes.defineWidget "SingleTextNode"

    let WidgetKey =
        Widgets.register "Union" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name

            let unionCaseNode =
                Helpers.getNodesFromWidgetCollection<UnionCaseNode> widget UnionCaseNode

            TypeDefnUnionNode(
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
                None,
                unionCaseNode,
                [],
                Range.Zero
            ))

[<AutoOpen>]
module UnionBuilders =
    type Fabulous.AST.Ast with

        static member inline Union(name: WidgetBuilder<#SingleTextNode>) =
            CollectionBuilder<TypeDefnUnionNode, UnionCaseNode>(
                Union.WidgetKey,
                Union.UnionCaseNode,
                AttributesBundle(StackList.empty(), ValueSome [| Union.Name.WithValue(name.Compile()) |], ValueNone)
            )

        static member inline Union(node: SingleTextNode) = Ast.Union(Ast.EscapeHatch(node))

        static member inline Union(name: string) =
            Ast.Union(SingleTextNode(name, Range.Zero))

[<Extension>]
type UnionYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<TypeDefnUnionNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let typeDefn = TypeDefn.Union(node)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }
