namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

type Namespace(hashDirectives, name, decls, isRecursive) =
    inherit
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
                          Some(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero)),
                          Range.Zero
                      )
                  ),
                  decls,
                  Range.Zero
              ) ],
            Range.Zero
        )

module Namespace =
    let Name = Attributes.defineScalar<string> "IdentList"

    let Decls = Attributes.defineWidgetCollection "Decls"
    let ParsedHashDirectives = Attributes.defineWidgetCollection "ParsedHashDirectives"
    let IsRecursive = Attributes.defineScalar<bool> "IsRecursive"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let WidgetKey =
        Widgets.register "Namespace" (fun widget ->
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

            Namespace(hashDirectives, name, decls, isRecursive))

[<AutoOpen>]
module NamespaceBuilders =
    type Ast with
        static member Namespace(name: string) =
            CollectionBuilder<Namespace, ModuleDecl>(
                Namespace.WidgetKey,
                Namespace.Decls,
                AttributesBundle(StackList.one(Namespace.Name.WithValue(name)), ValueNone, ValueNone)
            )

[<Extension>]
type NamespaceModifiers =
    [<Extension>]
    static member inline hashDirectives(this: WidgetBuilder<Namespace>) =
        AttributeCollectionBuilder<Namespace, ParsedHashDirectiveNode>(this, Namespace.ParsedHashDirectives)

    [<Extension>]
    static member inline toRecursive(this: WidgetBuilder<Namespace>) =
        this.AddScalar(Namespace.IsRecursive.WithValue(true))

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<Namespace>) =
        this.AddScalar(Namespace.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<Namespace>) =
        this.AddScalar(Namespace.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<Namespace>) =
        this.AddScalar(Namespace.Accessibility.WithValue(AccessControl.Internal))
