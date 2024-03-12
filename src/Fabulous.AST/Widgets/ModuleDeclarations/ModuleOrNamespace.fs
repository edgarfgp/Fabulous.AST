namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module ModuleOrNamespace =
    let Name = Attributes.defineScalar<string> "IdentList"

    let WidgetKey =
        Widgets.register "Namespace" (fun widget ->
            let decls = Widgets.getNodesFromWidgetCollection<ModuleDecl> widget Oak.Decls
            let name = Widgets.getScalarValue widget Name
            let isNameSpace = Widgets.getScalarValue widget Oak.IsNameSpace

            let textNode =
                if isNameSpace then
                    SingleTextNode.``namespace``
                else
                    SingleTextNode.``module``

            let isRecursive =
                Widgets.tryGetScalarValue widget Oak.IsRecursive
                |> ValueOption.defaultValue false

            let hashDirectives =
                Widgets.tryGetNodesFromWidgetCollection<ParsedHashDirectiveNode> widget Oak.ParsedHashDirectives

            let hashDirectives =
                match hashDirectives with
                | Some hashDirectives -> hashDirectives
                | None -> []

            let accessControl =
                Widgets.tryGetScalarValue widget Oak.Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            Oak(
                hashDirectives,
                [ ModuleOrNamespaceNode(
                      Some(
                          ModuleOrNamespaceHeaderNode(
                              None,
                              None,
                              MultipleTextsNode([ textNode ], Range.Zero),
                              (if isNameSpace then None else accessControl),
                              isRecursive,
                              Some(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero)),
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
        static member Namespace(name: string) =
            CollectionBuilder<Oak, ModuleDecl>(
                ModuleOrNamespace.WidgetKey,
                Oak.Decls,
                AttributesBundle(
                    StackList.two(ModuleOrNamespace.Name.WithValue(name), Oak.IsNameSpace.WithValue(true)),
                    ValueNone,
                    ValueNone
                )
            )

        static member Module(name: string) =
            CollectionBuilder<Oak, ModuleDecl>(
                ModuleOrNamespace.WidgetKey,
                Oak.Decls,
                AttributesBundle(
                    StackList.two(ModuleOrNamespace.Name.WithValue(name), Oak.IsNameSpace.WithValue(false)),
                    ValueNone,
                    ValueNone
                )
            )
