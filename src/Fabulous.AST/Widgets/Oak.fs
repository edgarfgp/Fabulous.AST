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

    let WidgetKey =
        Widgets.register "AnonymousModule" (fun widget ->
            let decls = Widgets.getNodesFromWidgetCollection<ModuleOrNamespaceNode> widget Decls

            let hashDirectives =
                Widgets.tryGetScalarValue widget ParsedHashDirectives
                |> ValueOption.map id
                |> ValueOption.defaultValue []

            Oak(List.ofSeq hashDirectives, decls, Range.Zero))

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
    /// <summary>Sets the hash directives for the current Oak widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="values">The hash directives to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Module("MyModule") {
    ///             Value("x", "1")
    ///         }
    ///     }
    /// }
    /// |> _.hashDirectives([ NoWarn("FS0028") ])
    /// </code>
    [<Extension>]
    static member inline hashDirectives(this: WidgetBuilder<Oak>, values: WidgetBuilder<ParsedHashDirectiveNode> seq) =
        this.AddScalar(Oak.ParsedHashDirectives.WithValue([ for value in values -> Gen.mkOak value ]))

    /// <summary>Sets the hash directive for the current Oak widget.</summary>
    /// <param name="this">Current widget.</param>
    /// <param name="value">The hash directive to set.</param>
    /// <code lang="fsharp">
    /// Oak() {
    ///     AnonymousModule() {
    ///         Module("MyModule") {
    ///             Value("x", "1")
    ///         }
    ///     }
    /// }
    /// |> _.hashDirective(NoWarn("FS0028"))
    /// </code>
    [<Extension>]
    static member inline hashDirective(this: WidgetBuilder<Oak>, value: WidgetBuilder<ParsedHashDirectiveNode>) =
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
