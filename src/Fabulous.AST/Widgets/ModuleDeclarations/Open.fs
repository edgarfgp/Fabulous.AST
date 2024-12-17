namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
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
        static member Open(values: string list) =
            let values =
                values
                |> List.map(fun name -> IdentifierOrDot.Ident(SingleTextNode.Create(name)))
                |> List.intersperse(IdentifierOrDot.KnownDot(SingleTextNode.dot))

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
        static member OpenGlobal(values: string list) =
            let values =
                values
                |> List.map(fun name -> IdentifierOrDot.Ident(SingleTextNode.Create(name)))
                |> fun gbl -> IdentifierOrDot.Ident(SingleTextNode.``global``) :: gbl
                |> List.intersperse(IdentifierOrDot.KnownDot(SingleTextNode.dot))

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
        static member OpenType(values: string list) =
            WidgetBuilder<OpenListNode>(
                Open.WidgetKey,
                Open.OpenList.WithValue(Open.Target(OpenTargetNode(Gen.mkOak(Ast.LongIdent(values)), Range.Zero)))

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
