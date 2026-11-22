namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Oak =
    let Decls = Attributes.defineWidgetCollection "Decls"

    let ParsedHashDirectives =
        Attributes.defineScalar<ParsedHashDirectiveNode seq> "ParsedHashDirectives"

    /// The directives of a hash directive declaration (`NoWarn`, `HashDirective`). Trivia attached to the
    /// declaration, such as an `#if DEBUG` / `#endif` guard, moves to the first and last directive.
    let hashDirectivesOf(decl: ModuleDecl) : ParsedHashDirectiveNode list =
        match decl with
        | ModuleDecl.HashDirectiveList node ->
            // Copy the directives: trivia modifiers keep returning the same declaration node, so
            // adding trivia to the originals would repeat it every time the builder is reused.
            let directives =
                node.HashDirectives
                |> List.map(fun directive ->
                    let copy = ParsedHashDirectiveNode(directive.Ident, directive.Args, directive.Range)

                    for trivia in directive.ContentBefore do
                        copy.AddBefore(trivia)

                    for trivia in directive.ContentAfter do
                        copy.AddAfter(trivia)

                    copy)

            match directives with
            | [] -> []
            | first :: _ ->
                let last = List.last directives

                for trivia in node.ContentBefore do
                    first.AddBefore(trivia)

                for trivia in node.ContentAfter do
                    last.AddAfter(trivia)

                directives
        | _ ->
            raise(
                System.ArgumentException(
                    "Oak hash directives must be hash directive declarations such as NoWarn or HashDirective.",
                    "values"
                )
            )

    let WidgetKey =
        Widgets.register "AnonymousModule" (fun widget ->
            let decls = Widgets.getNodesFromWidgetCollection<ModuleOrNamespaceNode> widget Decls

            let hashDirectives =
                Widgets.tryGetScalarValue widget ParsedHashDirectives
                |> ValueOption.map id
                |> ValueOption.defaultValue []

            Oak(List.ofSeq hashDirectives, decls, Range.range0))

[<AutoOpen>]
module SyntaxOakBuilders =
    type Ast with
        /// <summary>
        /// Creates an Oak widget. This is the root widget for the Oak DSL. It is used to define Oak nodes e.g. Modules, Namespaces, and Declarations.
        /// </summary>
        /// <code lang="fsharp">
        /// Oak() {
        ///     AnonymousModule() {
        ///         Module("MyModule") {
        ///             Value("x", "1")
        ///         }
        ///     }
        /// }
        /// </code>
        static member Oak() =
            CollectionBuilder<Oak, 'marker>(Oak.WidgetKey, Oak.Decls)

type SyntaxOakModifiers =
    /// <summary>Sets the hash directives for the current Oak widget from raw Fantomas nodes
    /// (for example <c>EscapeHatch(ParsedHashDirectiveNode(...))</c>). To use <c>NoWarn</c> or
    /// <c>HashDirective</c>, use the overload that takes module declarations.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="values">The hash directives to set.</param>
    [<Extension>]
    static member inline hashDirectives(this: WidgetBuilder<Oak>, values: WidgetBuilder<ParsedHashDirectiveNode> seq) =
        this.AddScalar(Oak.ParsedHashDirectives.WithValue([ for value in values -> Gen.mkOak value ]))

    /// <summary>Sets the hash directive for the current Oak widget from a raw Fantomas node
    /// (for example <c>EscapeHatch(ParsedHashDirectiveNode(...))</c>). To use <c>NoWarn</c> or
    /// <c>HashDirective</c>, use the overload that takes a module declaration.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="value">The hash directive to set.</param>
    [<Extension>]
    static member inline hashDirective(this: WidgetBuilder<Oak>, value: WidgetBuilder<ParsedHashDirectiveNode>) =
        SyntaxOakModifiers.hashDirectives(this, [ value ])

    /// <summary>Sets the hash directives for the current Oak widget, from directive declarations such as
    /// <c>NoWarn</c> or <c>HashDirective</c>.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="values">The hash directives to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Value("x", "1")
    ///     }
    /// }
    /// |> _.hashDirectives([ NoWarn("FS0028"); HashDirective("r", String("nuget: Fantomas.Core")) ])
    /// </code>
    /// <exception cref="T:System.ArgumentException">A value is not a hash directive.</exception>
    [<Extension>]
    static member inline hashDirectives(this: WidgetBuilder<Oak>, values: WidgetBuilder<ModuleDecl> seq) =
        let directives =
            [ for value in values do
                  yield! Oak.hashDirectivesOf(Gen.mkOak value) ]

        this.AddScalar(Oak.ParsedHashDirectives.WithValue(directives))

    /// <summary>Sets the hash directive for the current Oak widget, from a directive declaration such as
    /// <c>NoWarn</c> or <c>HashDirective</c>.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="value">The hash directive to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Value("x", "1")
    ///     }
    /// }
    /// |> _.hashDirective(NoWarn("FS0028"))
    /// </code>
    /// <exception cref="T:System.ArgumentException">The value is not a hash directive.</exception>
    [<Extension>]
    static member inline hashDirective(this: WidgetBuilder<Oak>, value: WidgetBuilder<ModuleDecl>) =
        SyntaxOakModifiers.hashDirectives(this, [ value ])

type SyntaxOakExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, Oak>, x: WidgetBuilder<ModuleOrNamespaceNode>)
        : CollectionContent =
        let widget = Ast.EscapeHatch(Gen.mkOak x).Compile()
        { Widgets = MutStackArray1.One(widget) }

    ///<summary>Allows Anonymous Module components to be yielded directly into a Module
    /// Useful since there's no common holder of declarations or generic WidgetBuilder than can be used
    /// when yielding different types of declarations.</summary>
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ModuleOrNamespaceNode>) =
        let node = Gen.mkOak x

        let ws =
            node.Declarations
            |> List.map(fun x -> Ast.EscapeHatch(x).Compile())
            |> List.toArray
            |> MutStackArray1.fromArray

        { Widgets = ws }
