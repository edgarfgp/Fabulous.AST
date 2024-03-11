namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module ModuleOrNamespace =
    let Decls = Attributes.defineWidgetCollection "Decls"
    let IdentList = Attributes.defineScalar<IdentListNode> "IdentList"
    let IsRecursive = Attributes.defineScalar<bool> "IsRecursive"
    let ParsedHashDirectives = Attributes.defineWidgetCollection "ParsedHashDirectives"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"
    let IsNameSpace = Attributes.defineScalar<bool> "IsNameSpace"

    let WidgetKey =
        Widgets.register "Namespace" (fun widget ->
            let decls = Widgets.getNodesFromWidgetCollection<ModuleDecl> widget Decls
            let identList = Widgets.getScalarValue widget IdentList
            let isNameSpace = Widgets.getScalarValue widget IsNameSpace

            let textNode =
                if isNameSpace then
                    SingleTextNode.``namespace``
                else
                    SingleTextNode.``module``

            let isRecursive =
                Widgets.tryGetScalarValue widget IsRecursive |> ValueOption.defaultValue false

            let hashDirectives =
                Widgets.tryGetNodesFromWidgetCollection<ParsedHashDirectiveNode> widget ParsedHashDirectives

            let hashDirectives =
                match hashDirectives with
                | Some hashDirectives -> hashDirectives
                | None -> []

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
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
        static member Namespace(name: string) =
            CollectionBuilder<Oak, ModuleDecl>(
                ModuleOrNamespace.WidgetKey,
                ModuleOrNamespace.Decls,
                AttributesBundle(
                    StackList.two(
                        ModuleOrNamespace.IdentList.WithValue(
                            IdentListNode([ IdentifierOrDot.Ident(SingleTextNode(name, Range.Zero)) ], Range.Zero)
                        ),
                        ModuleOrNamespace.IsNameSpace.WithValue(true)
                    ),
                    ValueNone,
                    ValueNone
                )
            )

        static member Module(name: string) =
            CollectionBuilder<Oak, ModuleDecl>(
                ModuleOrNamespace.WidgetKey,
                ModuleOrNamespace.Decls,
                AttributesBundle(
                    StackList.two(
                        ModuleOrNamespace.IdentList.WithValue(
                            IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero)
                        ),
                        ModuleOrNamespace.IsNameSpace.WithValue(false)
                    ),
                    ValueNone,
                    ValueNone
                )
            )

[<Extension>]
type NamespaceModifiers =
    [<Extension>]
    static member inline hashDirectives(this: WidgetBuilder<Oak>) =
        AttributeCollectionBuilder<Oak, ParsedHashDirectiveNode>(this, ModuleOrNamespace.ParsedHashDirectives)

    [<Extension>]
    static member inline toRecursive(this: WidgetBuilder<Oak>) =
        this.AddScalar(ModuleOrNamespace.IsRecursive.WithValue(true))

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<Oak>) =
        this.AddScalar(ModuleOrNamespace.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<Oak>) =
        this.AddScalar(ModuleOrNamespace.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<Oak>) =
        this.AddScalar(ModuleOrNamespace.Accessibility.WithValue(AccessControl.Internal))
