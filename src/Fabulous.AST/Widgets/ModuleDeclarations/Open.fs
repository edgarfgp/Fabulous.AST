namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module Open =
    let Name = Attributes.defineScalar<string> "IdentListNode"

    let WidgetKey =
        Widgets.register "Open" (fun widget ->
            let name = Widgets.getScalarValue widget Name

            OpenModuleOrNamespaceNode(
                IdentListNode([ IdentifierOrDot.Ident(SingleTextNode(name, Range.Zero)) ], Range.Zero),
                Range.Zero
            ))

[<AutoOpen>]
module OpenBuilders =
    type Ast with
        static member Open(name: string) =
            WidgetBuilder<OpenModuleOrNamespaceNode>(
                Open.WidgetKey,
                AttributesBundle(StackList.one(Open.Name.WithValue(name)), Array.empty, Array.empty)
            )

[<Extension>]
type OpenYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<OpenModuleOrNamespaceNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let openList = OpenListNode([ Open.ModuleOrNamespace node ])
        let moduleDecl = ModuleDecl.OpenList openList
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
