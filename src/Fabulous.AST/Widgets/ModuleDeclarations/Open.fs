namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

type OpenNode(identListNode: IdentListNode) =
    inherit OpenModuleOrNamespaceNode(identListNode, Range.Zero)

module Open =
    let IdentList = Attributes.defineWidget "IdentList"

    let WidgetKey =
        Widgets.register "Open" (fun widget ->
            let identList = Helpers.getNodeFromWidget<IdentListNode> widget IdentList
            OpenNode identList)

[<AutoOpen>]
module OpenBuilders =
    type Ast with

        static member Open(identList: WidgetBuilder<#IdentListNode>) =
            WidgetBuilder<OpenNode>(
                Open.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| Open.IdentList.WithValue(identList.Compile()) |],
                    ValueNone
                )
            )

        static member Open(node: IdentListNode) = Ast.Open(Ast.EscapeHatch(node))

        static member Open(name: string) =
            Ast.Open(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode(name, Range.Zero)) ], Range.Zero))

[<Extension>]
type OpenYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<OpenNode>
        ) : CollectionContent =
        let node = Gen.ast x
        let openList = OpenListNode([ Open.ModuleOrNamespace node ])
        let moduleDecl = ModuleDecl.OpenList openList
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
