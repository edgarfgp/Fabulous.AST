namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module Namespace =
    let Decls = Attributes.defineWidgetCollection "Decls"
    let IdentList = Attributes.defineScalar<IdentListNode> "IdentList"
    let IsRecursive = Attributes.defineScalar<bool> "IsRecursive"
    let ParsedHashDirectives = Attributes.defineWidgetCollection "ParsedHashDirectives"

    let WidgetKey =
        Widgets.register "Namespace" (fun widget ->
            let decls = Helpers.getNodesFromWidgetCollection<ModuleDecl> widget Decls
            let identList = Helpers.getScalarValue widget IdentList

            let isRecursive =
                Helpers.tryGetScalarValue widget IsRecursive |> ValueOption.defaultValue false

            let hashDirectives =
                Helpers.tryGetNodesFromWidgetCollection<ParsedHashDirectiveNode> widget ParsedHashDirectives

            let hashDirectives =
                match hashDirectives with
                | Some hashDirectives -> hashDirectives
                | None -> []

            Oak(
                hashDirectives,
                [ ModuleOrNamespaceNode(
                      Some(
                          ModuleOrNamespaceHeaderNode(
                              None,
                              None,
                              MultipleTextsNode([ SingleTextNode.``namespace`` ], Range.Zero),
                              None,
                              isRecursive,
                              Some(identList),
                              Range.Zero
                          )
                      ),
                      decls,
                      Range.Zero
                  ) ],
                Range.Zero
            ))

[<AutoOpen>]
module NamespaceBuilders =
    type Ast with

        static member private BaseNamespace(identList: IdentListNode) =
            CollectionBuilder<Oak, ModuleDecl>(
                Namespace.WidgetKey,
                Namespace.Decls,
                AttributesBundle(StackList.one(Namespace.IdentList.WithValue(identList)), ValueNone, ValueNone)
            )

        static member Namespace(name: IdentListNode) = Ast.BaseNamespace(name)

        static member Namespace(name: string) =
            Ast.Namespace(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode(name, Range.Zero)) ], Range.Zero))

[<Extension>]
type NamespaceModifiers =
    [<Extension>]
    static member inline hashDirectives(this: WidgetBuilder<Oak>) =
        AttributeCollectionBuilder<Oak, ParsedHashDirectiveNode>(this, Namespace.ParsedHashDirectives)

    [<Extension>]
    static member inline toRecursive(this: WidgetBuilder<Oak>) =
        this.AddScalar(Namespace.IsRecursive.WithValue(true))
