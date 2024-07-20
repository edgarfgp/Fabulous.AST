namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module Open =
    let IdentList = Attributes.defineScalar<string list> "IdentList"

    let WidgetModuleOrNamespaceKey =
        Widgets.register "Open" (fun widget ->
            let identList =
                Widgets.getScalarValue widget IdentList
                |> List.map(fun name -> IdentifierOrDot.Ident(SingleTextNode.Create(name)))

            Open.ModuleOrNamespace(OpenModuleOrNamespaceNode(IdentListNode(identList, Range.Zero), Range.Zero)))

    let Target = Attributes.defineWidget "Target"

    let WidgetTargetKey =
        Widgets.register "Target" (fun widget ->
            let target = Widgets.getNodeFromWidget widget Target
            Open.Target(OpenTargetNode(target, Range.Zero)))

    let OpenList = Attributes.defineScalar<Open list> "OpenListNode"

    let WidgetKey =
        Widgets.register "OpenList" (fun widget ->
            let openList = Widgets.getScalarValue widget OpenList
            OpenListNode(openList))

[<AutoOpen>]
module OpenBuilders =
    type Ast with
        static member private OpenNode(name: string list) =
            WidgetBuilder<Open>(
                Open.WidgetModuleOrNamespaceKey,
                AttributesBundle(StackList.one(Open.IdentList.WithValue(name)), Array.empty, Array.empty)
            )

        static member private OpenTypeNode(name: WidgetBuilder<Type>) =
            WidgetBuilder<Open>(
                Open.WidgetTargetKey,
                AttributesBundle(StackList.empty(), [| Open.Target.WithValue(name.Compile()) |], Array.empty)
            )

        static member private OpenListNode(values: WidgetBuilder<Open> list) =
            let values = values |> List.map(Gen.mkOak)

            WidgetBuilder<OpenListNode>(
                Open.WidgetKey,
                AttributesBundle(StackList.one(Open.OpenList.WithValue(values)), Array.empty, Array.empty)
            )

        static member Open(values: string list) =
            Ast.OpenListNode([ Ast.OpenNode(values) ])

        static member Open(value: string) = Ast.Open([ value ])

        static member OpenType(values: WidgetBuilder<Type> list) =
            let values = values |> List.map(Ast.OpenTypeNode)
            Ast.OpenListNode(values)

        static member OpenType(values: string list) =
            let values = values |> List.map(Ast.LongIdent)
            Ast.OpenType(values)

        static member OpenType(value: WidgetBuilder<Type>) =
            Ast.OpenListNode([ Ast.OpenTypeNode(value) ])

        static member OpenType(value: string) = Ast.OpenType(Ast.LongIdent(value))

type OpenYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: OpenListNode) : CollectionContent =
        let moduleDecl = ModuleDecl.OpenList(x)
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<OpenListNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        OpenYieldExtensions.Yield(this, node)
