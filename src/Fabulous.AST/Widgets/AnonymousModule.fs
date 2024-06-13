namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak

type AnonymousModuleNode(decls) =
    inherit ModuleOrNamespaceNode(None, decls, Range.Zero)

module AnonymousModule =
    let Decls = Attributes.defineWidgetCollection "Decls"

    let WidgetKey =
        Widgets.register "AnonymousModule" (fun widget ->
            let decls = Widgets.getNodesFromWidgetCollection<ModuleDecl> widget Decls
            AnonymousModuleNode(decls))

[<AutoOpen>]
module AnonymousModuleBuilders =
    type Ast with
        static member AnonymousModule() =
            CollectionBuilder<AnonymousModuleNode, ModuleDecl>(AnonymousModule.WidgetKey, AnonymousModule.Decls)

type AnonymousModuleExtensions =
    /// Allows Anonymous Module components to be yielded directly into a Module
    /// Useful since there's no common holder of declarations or generic WidgetBuilder than can be used
    /// when yielding different types of declarations
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<AnonymousModuleNode>) =
        let node = Gen.mkOak x

        let ws =
            node.Declarations
            |> List.map(fun x -> Ast.EscapeHatch(x).Compile())
            |> List.toArray
            |> MutStackArray1.fromArray

        { Widgets = ws }
