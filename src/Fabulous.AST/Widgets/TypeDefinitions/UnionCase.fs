namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module UnionCase =

    let Name = Attributes.defineWidget "SingleTextNode"

    let WidgetKey =
        Widgets.register "UnionCase" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name
            UnionCaseNode(None, None, None, name, [], Range.Zero))

[<AutoOpen>]
module UnionCaseBuilders =
    type Fabulous.AST.Ast with

        static member inline UnionCase(name: WidgetBuilder<#SingleTextNode>) =
            WidgetBuilder<UnionCaseNode>(
                UnionCase.WidgetKey,
                AttributesBundle(StackList.empty(), ValueSome [| UnionCase.Name.WithValue(name.Compile()) |], ValueNone)
            )

        static member inline UnionCase(node: SingleTextNode) = Ast.UnionCase(Ast.EscapeHatch(node))

        static member inline UnionCase(name: string) =
            Ast.UnionCase(SingleTextNode(name, Range.Zero))

[<Extension>]
type UnionCaseYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, UnionCaseNode>,
            x: WidgetBuilder<TypeDefnUnionNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let typeDefn = ModuleDecl.TypeDefn(TypeDefn.Union(node))
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }
