namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

type NamespaceNode(header, decls) =
    inherit ModuleOrNamespaceNode(header, decls, Range.Zero)

module Namespace =
    let Decls = Attributes.defineWidgetCollection "Decls"
    let IsRecursive = Attributes.defineScalar<bool> "IsRecursive"
    let HeaderName = Attributes.defineScalar<string> "HeaderName"

    let WidgetKey =
        Widgets.register "Namespace" (fun widget ->
            let decls = Widgets.getNodesFromWidgetCollection<ModuleDecl> widget Decls
            let headerName = Widgets.getScalarValue widget HeaderName

            let isRecursive =
                Widgets.tryGetScalarValue widget IsRecursive |> ValueOption.defaultValue false

            let header =
                Some(
                    ModuleOrNamespaceHeaderNode(
                        None,
                        None,
                        MultipleTextsNode([ SingleTextNode.``namespace`` ], Range.Zero),
                        None,
                        isRecursive,
                        Some(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(headerName)) ], Range.Zero)),
                        Range.Zero
                    )
                )

            NamespaceNode(header, decls))

[<AutoOpen>]
module NamespaceBuilders =
    type Ast with
        static member Namespace(name: string) =
            CollectionBuilder<NamespaceNode, ModuleDecl>(
                Namespace.WidgetKey,
                Namespace.Decls,
                AttributesBundle(StackList.one(Namespace.HeaderName.WithValue(name)), Array.empty, Array.empty)
            )

type NamespaceModifiers =
    [<Extension>]
    static member inline toRecursive(this: WidgetBuilder<NamespaceNode>) =
        this.AddScalar(Namespace.IsRecursive.WithValue(true))
