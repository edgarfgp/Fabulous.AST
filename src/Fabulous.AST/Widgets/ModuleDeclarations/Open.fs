namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Open =
    let OpenList = Attributes.defineScalar<Open> "OpenListNode"

    let WidgetKey =
        Widgets.register "OpenList" (fun widget ->
            let openList = Widgets.getScalarValue widget OpenList
            OpenListNode([ openList ]))

[<AutoOpen>]
module OpenBuilders =
    type Ast with
        /// <summary>Creates an Open widget with the specified values.</summary>
        /// <param name="values">The values to open.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Open([ "System"; "Collections"; "Generic" ])
        ///     }
        /// }
        /// </code>
        static member Open(values: string seq) =
            let values =
                values
                |> Seq.map(fun name -> IdentifierOrDot.Ident(SingleTextNode.Create(name)))
                |> Seq.intersperse(IdentifierOrDot.KnownDot(SingleTextNode.dot))

            let value =
                Open.ModuleOrNamespace(OpenModuleOrNamespaceNode(IdentListNode(values, Range.Zero), Range.Zero))

            WidgetBuilder<OpenListNode>(Open.WidgetKey, Open.OpenList.WithValue(value))

        /// <summary>Creates an Open widget with the specified value.</summary>
        /// <param name="value">The value to open.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Open("System.Collections.Generic")
        ///     }
        /// }
        /// </code>
        static member Open(value: string) = Ast.Open([ value ])

        /// <summary>Creates an OpenGlobal widget with the specified values.</summary>
        /// <param name="values">The values to open.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         OpenGlobal([ "System"; "Collections"; "Generic" ])
        ///     }
        /// }
        /// </code>
        static member OpenGlobal(values: string seq) =
            let values =
                values
                |> Seq.map(fun name -> IdentifierOrDot.Ident(SingleTextNode.Create(name)))
                |> fun gbl -> IdentifierOrDot.Ident(SingleTextNode.``global``) :: List.ofSeq gbl
                |> Seq.intersperse(IdentifierOrDot.KnownDot(SingleTextNode.dot))

            let value =
                Open.ModuleOrNamespace(OpenModuleOrNamespaceNode(IdentListNode(values, Range.Zero), Range.Zero))

            WidgetBuilder<OpenListNode>(Open.WidgetKey, Open.OpenList.WithValue(value))

        /// <summary>Creates an OpenGlobal widget with the specified value.</summary>
        /// <param name="value">The value to open.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         OpenGlobal("System.Collections.Generic")
        ///     }
        /// }
        /// </code>
        static member OpenGlobal(value: string) = Ast.OpenGlobal([ value ])

        /// <summary>Creates an OpenType widget with the specified values.</summary>
        /// <param name="values">The values to open.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         OpenType([ "System"; "Collections"; "Generic" ])
        ///     }
        /// }
        /// </code>
        static member OpenType(values: string seq) =
            WidgetBuilder<OpenListNode>(
                Open.WidgetKey,
                Open.OpenList.WithValue(
                    Open.Target(OpenTargetNode(Gen.mkOak(Ast.LongIdent(List.ofSeq values)), Range.Zero))
                )

            )

        /// <summary>Creates an OpenType widget with the specified value.</summary>
        /// <param name="value">The value to open.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         OpenType("System.Collections.Generic")
        ///     }
        /// }
        /// </code>
        static member OpenType(value: string) = Ast.OpenType([ value ])

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

    [<Extension>]
    static member inline YieldFrom(_: CollectionBuilder<'parent, ModuleDecl>, x: OpenListNode seq) : CollectionContent =
        let widgets =
            x
            |> Seq.map(fun node ->
                let moduleDecl = ModuleDecl.OpenList(node)
                Ast.EscapeHatch(moduleDecl).Compile())
            |> Seq.toArray
            |> MutStackArray1.fromArray

        { Widgets = widgets }

    [<Extension>]
    static member inline YieldFrom
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<OpenListNode> seq)
        : CollectionContent =
        let nodes = x |> Seq.map Gen.mkOak
        OpenYieldExtensions.YieldFrom(this, nodes)
