namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak

module AnonymousModule =
    let Decls = Attributes.defineWidgetCollection "Decls"
    let ParsedHashDirectives = Attributes.defineWidgetCollection "ParsedHashDirectives"

    let WidgetKey =
        Widgets.register "AnonymousModule" (fun widget ->
            let decls = Widgets.getNodesFromWidgetCollection<ModuleOrNamespaceNode> widget Decls

            let hashDirectives =
                Widgets.tryGetNodesFromWidgetCollection<ParsedHashDirectiveNode> widget ParsedHashDirectives

            let hashDirectives =
                match hashDirectives with
                | Some hashDirectives -> hashDirectives
                | None -> []

            Oak(hashDirectives, decls, Range.Zero))

[<AutoOpen>]
module AnonymousModuleBuilders =
    type Ast with
        static member Oak() =
            CollectionBuilder<Oak, ModuleOrNamespaceNode>(AnonymousModule.WidgetKey, AnonymousModule.Decls)

[<Extension>]
type AnonymousModuleModifiers =
    [<Extension>]
    static member inline hashDirectives(this: WidgetBuilder<Oak>) =
        AttributeCollectionBuilder<Oak, ParsedHashDirectiveNode>(this, AnonymousModule.ParsedHashDirectives)

[<Extension>]
type AnonymousModuleExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, Oak>, x: WidgetBuilder<ModuleOrNamespaceNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let widget = Ast.EscapeHatch(node).Compile()
        { Widgets = MutStackArray1.One(widget) }
