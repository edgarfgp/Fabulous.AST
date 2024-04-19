namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak

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
module AnonymousModuleBuilders =
    type Ast with
        static member Oak() =
            CollectionBuilder<Oak, ModuleOrNamespaceNode>(Oak.WidgetKey, Oak.Decls)

type AnonymousModuleModifiers =
    [<Extension>]
    static member inline hashDirectives(this: WidgetBuilder<Oak>, values: WidgetBuilder<ParsedHashDirectiveNode> list) =
        this.AddScalar(Oak.ParsedHashDirectives.WithValue([ for value in values -> Gen.mkOak value ]))

    [<Extension>]
    static member inline hashDirective(this: WidgetBuilder<Oak>, value: WidgetBuilder<ParsedHashDirectiveNode>) =
        AnonymousModuleModifiers.hashDirectives(this, [ value ])

type AnonymousModuleExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, Oak>, x: WidgetBuilder<ModuleOrNamespaceNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let widget = Ast.EscapeHatch(node).Compile()
        { Widgets = MutStackArray1.One(widget) }
