namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module Module =
    let Decls = Attributes.defineWidgetCollection "Decls"
    let IdentList = Attributes.defineScalar<IdentListNode> "IdentList"
    let IsRecursive = Attributes.defineScalar<bool> "IsRecursive"

    let WidgetKey =
        Widgets.register "Module" (fun widget ->
            let decls = Helpers.getNodesFromWidgetCollection<ModuleDecl> widget Decls
            let identList = Helpers.getScalarValue widget IdentList

            let isRecursive =
                Helpers.tryGetScalarValue widget IsRecursive |> ValueOption.defaultValue false

            Oak(
                List.Empty,
                [ ModuleOrNamespaceNode(
                      Some(
                          ModuleOrNamespaceHeaderNode(
                              None,
                              None,
                              MultipleTextsNode([ SingleTextNode.``module`` ], Range.Zero),
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
module ModuleBuilders =
    type Ast with

        static member private BaseModule(identList: IdentListNode) =
            CollectionBuilder<Oak, ModuleDecl>(
                Module.WidgetKey,
                Module.Decls,
                AttributesBundle(StackList.one(Module.IdentList.WithValue(identList)), ValueNone, ValueNone)
            )

        static member Module(name: string) =
            Ast.BaseModule(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero))

[<Extension>]
type ModuleModifiers =
    [<Extension>]
    static member inline hashDirectives(this: WidgetBuilder<Oak>) =
        AttributeCollectionBuilder<Oak, ParsedHashDirectiveNode>(this, Namespace.ParsedHashDirectives)

    [<Extension>]
    static member inline toRecursive(this: WidgetBuilder<Oak>) =
        this.AddScalar(Module.IsRecursive.WithValue(true))
