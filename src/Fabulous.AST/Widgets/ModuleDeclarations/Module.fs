namespace Fabulous.AST

open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

type ModuleNode(identList: IdentListNode, decls: ModuleDecl list) =
    inherit
        Oak(
            List.Empty,
            [ ModuleOrNamespaceNode(
                  Some(
                      ModuleOrNamespaceHeaderNode(
                          None,
                          None,
                          MultipleTextsNode([ SingleTextNode.``module`` ], Range.Zero),
                          None,
                          false,
                          Some(identList),
                          Range.Zero
                      )
                  ),
                  decls,
                  Range.Zero
              ) ],
            Range.Zero
        )

module Module =
    let Decls = Attributes.defineWidgetCollection "Decls"
    let IdentList = Attributes.defineScalar<IdentListNode> "IdentList"

    let WidgetKey =
        Widgets.register "Module" (fun widget ->
            let decls = Helpers.getNodesFromWidgetCollection<ModuleDecl> widget Decls
            let identList = Helpers.getScalarValue widget IdentList
            ModuleNode(identList, decls))

[<AutoOpen>]
module ModuleBuilders =
    type Ast with

        static member private BaseModule(identList: IdentListNode) =
            CollectionBuilder<ModuleNode, ModuleDecl>(
                Module.WidgetKey,
                Module.Decls,
                AttributesBundle(StackList.one(Module.IdentList.WithValue(identList)), ValueSome [||], ValueNone)
            )

        static member Module(name: string) =
            Ast.BaseModule(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero))
