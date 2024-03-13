namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

type Module(hashDirectives, name, decls, accessControl, isRecursive) =
    inherit
        Oak(
            hashDirectives,
            [ ModuleOrNamespaceNode(
                  Some(
                      ModuleOrNamespaceHeaderNode(
                          None,
                          None,
                          MultipleTextsNode([ SingleTextNode.``module`` ], Range.Zero),
                          accessControl,
                          isRecursive,
                          Some(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero)),
                          Range.Zero
                      )
                  ),
                  decls,
                  Range.Zero
              ) ],
            Range.Zero
        )

module Module =
    let Name = Attributes.defineScalar<string> "IdentList"
    let Decls = Attributes.defineWidgetCollection "Decls"
    let ParsedHashDirectives = Attributes.defineWidgetCollection "ParsedHashDirectives"
    let IsRecursive = Attributes.defineScalar<bool> "IsRecursive"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let WidgetKey =
        Widgets.register "Module" (fun widget ->
            let decls = Widgets.getNodesFromWidgetCollection<ModuleDecl> widget Decls
            let name = Widgets.getScalarValue widget Name

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

            Module(hashDirectives, name, decls, accessControl, isRecursive))

[<AutoOpen>]
module ModuleBuilders =
    type Ast with
        static member Module(name: string) =
            CollectionBuilder<Module, ModuleDecl>(
                Module.WidgetKey,
                Module.Decls,
                AttributesBundle(StackList.one(Module.Name.WithValue(name)), Array.empty, Array.empty)
            )

[<Extension>]
type ModuleModifiers =
    [<Extension>]
    static member inline hashDirectives(this: WidgetBuilder<Module>) =
        AttributeCollectionBuilder<Module, ParsedHashDirectiveNode>(this, Module.ParsedHashDirectives)

    [<Extension>]
    static member inline toRecursive(this: WidgetBuilder<Module>) =
        this.AddScalar(Module.IsRecursive.WithValue(true))

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<Module>) =
        this.AddScalar(Module.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<Module>) =
        this.AddScalar(Module.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<Module>) =
        this.AddScalar(Module.Accessibility.WithValue(AccessControl.Internal))
