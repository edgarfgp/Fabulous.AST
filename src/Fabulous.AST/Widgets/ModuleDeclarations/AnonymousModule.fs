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
            let decls = Helpers.getNodesFromWidgetCollection<ModuleDecl> widget Decls

            let hashDirectives =
                Helpers.tryGetNodesFromWidgetCollection<ParsedHashDirectiveNode> widget ParsedHashDirectives

            let hashDirectives =
                match hashDirectives with
                | Some hashDirectives -> hashDirectives
                | None -> []

            Oak(hashDirectives, [ ModuleOrNamespaceNode(None, decls, Range.Zero) ], Range.Zero))

[<AutoOpen>]
module AnonymousModuleBuilders =
    type Ast with
        static member AnonymousModule() =
            CollectionBuilder<Oak, ModuleDecl>(AnonymousModule.WidgetKey, AnonymousModule.Decls)

[<Extension>]
type AnonymousModuleExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<Expr>) : CollectionContent =
        let node = Tree.compile x
        let moduleDecl = ModuleDecl.DeclExpr(node)
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }