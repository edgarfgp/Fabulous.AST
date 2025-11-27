namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ParsedHashDirectives =
    let Ident = Attributes.defineScalar<string> "Ident"
    let Arguments = Attributes.defineScalar<string seq> "Args"

    let WidgetKey =
        Widgets.register "HashDirective" (fun widget ->
            let ident = Widgets.getScalarValue widget Ident

            let arguments =
                Widgets.getScalarValue widget Arguments
                |> Seq.map(fun arg ->
                    Choice2Of2(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create arg) ], Range.Zero)))

            ParsedHashDirectiveNode(ident, List.ofSeq arguments, Range.Zero))

[<AutoOpen>]
module HashDirectiveBuilders =
    type Ast with

        static member private BaseHashDirective(ident: string, arguments: WidgetBuilder<Constant> seq) =
            let arguments =
                arguments
                |> Seq.choose(fun arg ->
                    match Gen.mkOak arg with
                    | Constant.FromText node -> Some node.Text
                    | Constant.Unit _ -> None
                    | Constant.Measure _ -> None)

            WidgetBuilder<ParsedHashDirectiveNode>(
                ParsedHashDirectives.WidgetKey,
                ParsedHashDirectives.Ident.WithValue(ident),
                ParsedHashDirectives.Arguments.WithValue(arguments)
            )

        /// <summary>Creates a NoWarn hash directive.</summary>
        /// <param name="args">The arguments of the NoWarn hash directive.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         NoWarn([ String "0044"; String "0045" ])
        ///     }
        /// }
        /// </code>
        static member NoWarn(args: WidgetBuilder<Constant> seq) = Ast.BaseHashDirective("nowarn", args)

        /// <summary>Creates a NoWarn hash directive.</summary>
        /// <param name="args">The arguments of the NoWarn hash directive.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         NoWarn([ "0044"; "0045" ])
        ///     }
        /// }
        /// </code>
        static member NoWarn(args: string seq) =
            Ast.NoWarn(args |> Seq.map(Ast.Constant))

        /// <summary>Creates a NoWarn hash directive.</summary>
        /// <param name="value">The argument of the NoWarn hash directive.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         NoWarn(String "0044")
        ///     }
        /// }
        /// </code>
        static member NoWarn(value: WidgetBuilder<Constant>) = Ast.NoWarn([ value ])

        /// <summary>Creates a NoWarn hash directive.</summary>
        /// <param name="value">The argument of the NoWarn hash directive.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         NoWarn("0044")
        ///     }
        /// }
        /// </code>
        static member NoWarn(value: string) = Ast.NoWarn(Ast.Constant(value))

        /// <summary>Creates a Help hash directive.</summary>
        /// <param name="value">The arguments of the Help hash directive.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Help(String("List.map"))
        ///     }
        /// }
        /// </code>
        static member Help(value: WidgetBuilder<Constant>) =
            Ast.BaseHashDirective("help", [ value ])

        /// <summary>Creates a Help hash directive.</summary>
        /// <param name="value">The arguments of the Help hash directive.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Help("List.map")
        ///     }
        /// }
        /// </code>
        static member Help(value: string) = Ast.Help(Ast.Constant(value))

        /// <summary>Creates a HashDirective widget</summary>
        /// <param name="ident">The identifier of the hash directive.</param>
        /// <param name="args">The arguments of the hash directive.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         HashDirective("help", [ String "List.map" ])
        ///     }
        /// }
        /// </code>
        static member HashDirective(ident: string, args: WidgetBuilder<Constant> seq) =
            Ast.BaseHashDirective(ident, args)

        /// <summary>Creates a HashDirective widget</summary>
        /// <param name="ident">The identifier of the hash directive.</param>
        /// <param name="args">The arguments of the hash directive.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         HashDirective("help", [ "List.map" ])
        ///     }
        /// }
        /// </code>
        static member HashDirective(ident: string, args: string seq) =
            Ast.HashDirective(ident, args |> Seq.map(Ast.Constant))

        /// <summary>Creates a HashDirective widget</summary>
        /// <param name="ident">The identifier of the hash directive.</param>
        /// <param name="value">The argument of the hash directive.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         HashDirective("help", String "List.map")
        ///     }
        /// }
        /// </code>
        static member HashDirective(ident: string, value: WidgetBuilder<Constant>) = Ast.HashDirective(ident, [ value ])

        /// <summary>Creates a HashDirective widget</summary>
        /// <param name="ident">The identifier of the hash directive.</param>
        /// <param name="value">The argument of the hash directive.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         HashDirective("help", "List.map")
        ///     }
        /// }
        /// </code>
        static member HashDirective(ident: string, value: string) =
            Ast.HashDirective(ident, Ast.Constant(value))

        /// <summary>Creates a HashDirective widget</summary>
        /// <param name="ident">The identifier of the hash directive.</param>
        /// <code language="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         HashDirective("help")
        ///     }
        /// }
        /// </code>
        static member HashDirective(ident: string) = Ast.HashDirective(ident, [])

type HashDirectiveNodeExtensions =

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: ParsedHashDirectiveNode)
        : CollectionContent =
        let moduleDecl = ModuleDecl.HashDirectiveList(HashDirectiveListNode([ x ]))
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ParsedHashDirectiveNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        HashDirectiveNodeExtensions.Yield(this, node)
