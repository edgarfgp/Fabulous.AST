namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module OpenType =
    let Target = Attributes.defineWidget "Target"

    let WidgetKey =
        Widgets.register "OpenType" (fun widget ->
            let target = Widgets.getNodeFromWidget widget Target
            OpenTargetNode(target, Range.Zero))

[<AutoOpen>]
module OpenTypeBuilders =
    type Ast with
        static member OpenType(name: WidgetBuilder<Type>) =
            WidgetBuilder<OpenTargetNode>(
                OpenType.WidgetKey,
                AttributesBundle(StackList.empty(), [| OpenType.Target.WithValue(name.Compile()) |], Array.empty)
            )

        static member OpenType(name: string) = Ast.OpenType(Ast.LongIdent(name))

type OpenTypeYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<OpenTargetNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let openList = OpenListNode([ Open.Target node ])
        let moduleDecl = ModuleDecl.OpenList openList
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
