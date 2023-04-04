namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

type UnionParameterizedCaseNode(name, fields) =
    inherit UnionCaseNode(None, None, None, name, fields, Range.Zero)

module UnionParameterizedCase =

    let Name = Attributes.defineWidget "SingleTextNode"

    let Fields = Attributes.defineWidgetCollection "Fields"

    let WidgetKey =
        Widgets.register "UnionParameterizedCase" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name
            let fields = Helpers.getNodesFromWidgetCollection<FieldNode> widget Fields
            UnionParameterizedCaseNode(name, fields))

[<AutoOpen>]
module UnionParameterizedCaseBuilders =
    type Fabulous.AST.Ast with

        static member inline UnionParameterizedCase(name: WidgetBuilder<#SingleTextNode>) =
            CollectionBuilder<UnionParameterizedCaseNode, FieldNode>(
                UnionParameterizedCase.WidgetKey,
                UnionParameterizedCase.Fields,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| UnionParameterizedCase.Name.WithValue(name.Compile()) |],
                    ValueNone
                )
            )

        static member inline UnionParameterizedCase(node: SingleTextNode) =
            Ast.UnionParameterizedCase(Ast.EscapeHatch(node))

        static member inline UnionParameterizedCase(name: string) =
            Ast.UnionParameterizedCase(SingleTextNode(name, Range.Zero))

[<Extension>]
type UnionParameterizedCaseYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<TypeDefnUnionNode, UnionCaseNode>,
            x: WidgetBuilder<UnionParameterizedCaseNode>
        ) : CollectionContent =
        { Widgets = MutStackArray1.One(x.Compile()) }
