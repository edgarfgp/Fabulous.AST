namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Oak =
    let Decls = Attributes.defineWidgetCollection "Decls"

    let ParsedHashDirectives =
        Attributes.defineScalar<ParsedHashDirectiveNode list> "ParsedHashDirectives"

    let WidgetKey =
        Widgets.register "AnonymousModule" (fun widget ->
            let decls = Widgets.getNodesFromWidgetCollection<ModuleOrNamespaceNode> widget Decls

            let hashDirectives = Widgets.tryGetScalarValue widget ParsedHashDirectives

            let hashDirectives =
                match hashDirectives with
                | ValueSome hashDirectives -> hashDirectives
                | ValueNone -> []

            Oak(hashDirectives, decls, Range.Zero))

[<AutoOpen>]
module SyntaxOakBuilders =
    type Ast with
        /// Creates an Oak AST node
        static member Oak() =
            CollectionBuilder<Oak, 'marker>(Oak.WidgetKey, Oak.Decls)

type SyntaxOakModifiers =
    [<Extension>]
    static member inline hashDirectives(this: WidgetBuilder<Oak>, values: WidgetBuilder<ParsedHashDirectiveNode> list) =
        this.AddScalar(Oak.ParsedHashDirectives.WithValue([ for value in values -> Gen.mkOak value ]))

    [<Extension>]
    static member inline hashDirective(this: WidgetBuilder<Oak>, value: WidgetBuilder<ParsedHashDirectiveNode>) =
        SyntaxOakModifiers.hashDirectives(this, [ value ])

type SyntaxOakExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, Oak>, x: ModuleOrNamespaceNode) : CollectionContent =
        let widget = Ast.EscapeHatch(x).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, Oak>, x: WidgetBuilder<ModuleOrNamespaceNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        SyntaxOakExtensions.Yield(this, node)

    /// Allows Anonymous Module components to be yielded directly into a Module
    /// Useful since there's no common holder of declarations or generic WidgetBuilder than can be used
    /// when yielding different types of declarations
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ModuleOrNamespaceNode>) =
        let node = Gen.mkOak x

        let ws =
            node.Declarations
            |> List.map(fun x -> Ast.EscapeHatch(x).Compile())
            |> List.toArray
            |> MutStackArray1.fromArray

        { Widgets = ws }
