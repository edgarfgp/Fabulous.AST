namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module OpenType =
    let Target = Attributes.defineScalar<string> "Target"

    let WidgetKey =
        Widgets.register "OpenType" (fun widget ->
            let target = Widgets.getScalarValue widget Target

            OpenTargetNode(
                Type.LongIdent(
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode(target, Range.Zero)) ], Range.Zero)
                ),
                Range.Zero
            ))

[<AutoOpen>]
module OpenTypeBuilders =
    type Ast with

        static member OpenType(name: string) =
            WidgetBuilder<OpenTargetNode>(
                OpenType.WidgetKey,
                AttributesBundle(StackList.one(OpenType.Target.WithValue(name)), Array.empty, Array.empty)
            )

[<Extension>]
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
