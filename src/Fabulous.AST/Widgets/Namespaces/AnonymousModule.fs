namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak

type AnonymousModuleNode(decls: ModuleDecl list, hashDirectives: ParsedHashDirectiveNode list) =
    inherit Oak(hashDirectives, [ ModuleOrNamespaceNode(None, decls, Range.Zero) ], Range.Zero)

module AnonymousModule =
    let Decls = Attributes.defineWidgetCollection "Decls"

    let ParsedHashDirectives = Attributes.defineWidgetCollection "ParsedHashDirectives"

    let WidgetKey =
        Widgets.register "AnonymousModule" (fun widget ->
            let decls = Helpers.getNodesFromWidgetCollection<ModuleDecl> widget Decls

            let parsedHashDirectives =
                Helpers.tryGetNodesFromWidgetCollection widget ParsedHashDirectives
                |> Option.defaultValue []

            AnonymousModuleNode(decls, parsedHashDirectives))

[<AutoOpen>]
module AnonymousModuleBuilders =
    type Ast with
        static member inline AnonymousModule() =
            CollectionBuilder<AnonymousModuleNode, ModuleDecl>(AnonymousModule.WidgetKey, AnonymousModule.Decls)

[<Extension>]
type AnonymousModuleModifiers =
    [<Extension>]
    static member inline hashDirective
        (
            this: WidgetBuilder<AnonymousModuleNode>,
            value: WidgetBuilder<HashDirectiveNode>
        ) =
        AttributeCollectionBuilder<AnonymousModuleNode, HashDirectiveNode>(this, AnonymousModule.ParsedHashDirectives) {
            value
        }

    [<Extension>]
    static member inline hashDirectives(this: WidgetBuilder<AnonymousModuleNode>) =
        AttributeCollectionBuilder<AnonymousModuleNode, HashDirectiveNode>(this, AnonymousModule.ParsedHashDirectives)

[<Extension>]
type AnonymousModuleExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: AttributeCollectionBuilder<AnonymousModuleNode, HashDirectiveNode>,
            x: WidgetBuilder<HashDirectiveNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let widget = Ast.EscapeHatch(node).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<Expr>) : CollectionContent =
        let node = Tree.compile x
        let moduleDecl = ModuleDecl.DeclExpr(node)
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
