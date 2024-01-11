namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

type OpenTypeNode(target: IdentListNode) =
    inherit OpenTargetNode(Type.LongIdent target, Range.Zero)

module OpenType =
    let Target = Attributes.defineWidget "Target"

    let WidgetKey =
        Widgets.register "OpenType" (fun widget ->
            let target = Helpers.getNodeFromWidget<IdentListNode> widget Target
            OpenTypeNode target)

[<AutoOpen>]
module OpenTypeBuilders =
    type Ast with

        static member inline OpenType(identList: WidgetBuilder<#IdentListNode>) =
            WidgetBuilder<OpenTypeNode>(
                OpenType.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| OpenType.Target.WithValue(identList.Compile()) |],
                    ValueNone
                )
            )

        static member inline OpenType(node: IdentListNode) = Ast.OpenType(Ast.EscapeHatch(node))

        static member inline OpenType(name: string) =
            Ast.OpenType(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode(name, Range.Zero)) ], Range.Zero))

[<Extension>]
type OpenTypeYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<OpenTypeNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let openList = OpenListNode([ Open.Target node ])
        let moduleDecl = ModuleDecl.OpenList openList
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
