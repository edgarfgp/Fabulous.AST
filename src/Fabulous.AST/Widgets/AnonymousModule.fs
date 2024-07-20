namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module AnonymousModule =
    let Decls = Attributes.defineWidgetCollection "Decls"

    let WidgetKey =
        Widgets.register "AnonymousModule" (fun widget ->
            let decls = Widgets.getNodesFromWidgetCollection<ModuleDecl> widget Decls
            ModuleOrNamespaceNode(None, decls, Range.Zero))

[<AutoOpen>]
module AnonymousModuleBuilders =
    type Ast with
        static member AnonymousModule() =
            CollectionBuilder<ModuleOrNamespaceNode, ModuleDecl>(AnonymousModule.WidgetKey, AnonymousModule.Decls)

type AnonymousModuleExtensions =
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
