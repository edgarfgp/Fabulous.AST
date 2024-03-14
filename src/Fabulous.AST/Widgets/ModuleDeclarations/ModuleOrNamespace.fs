namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module ModuleOrNamespace =
    let Name = Attributes.defineScalar<string> "IdentList"

    let Decls = Attributes.defineWidgetCollection "Decls"
    let IsRecursive = Attributes.defineScalar<bool> "IsRecursive"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let IsAnonymousModule = Attributes.defineScalar<bool> "IsAnonymous"

    let WidgetKey =
        Widgets.register "Namespace" (fun widget ->
            let decls = Widgets.getNodesFromWidgetCollection<ModuleDecl> widget Decls

            let isAnonymous =
                Widgets.tryGetScalarValue widget IsAnonymousModule
                |> ValueOption.defaultValue false

            let name = Widgets.tryGetScalarValue widget Name

            let textNode =
                match name with
                | ValueNone -> None
                | ValueSome name ->
                    match StringParsing.normalizeModuleOrNamespaceName(name) with
                    | ModuleOrNamespaceDecl.Anonymous -> None
                    | ModuleOrNamespaceDecl.Module _ -> Some SingleTextNode.``module``
                    | ModuleOrNamespaceDecl.Namespace _ -> Some SingleTextNode.``namespace``

            let isNameSpace =
                match name with
                | ValueNone -> false
                | ValueSome name ->
                    match StringParsing.normalizeModuleOrNamespaceName(name) with
                    | ModuleOrNamespaceDecl.Anonymous -> false
                    | ModuleOrNamespaceDecl.Module _ -> false
                    | ModuleOrNamespaceDecl.Namespace _ -> true

            let name =
                match name with
                | ValueNone -> None
                | ValueSome s ->
                    match StringParsing.normalizeModuleOrNamespaceName(s) with
                    | ModuleOrNamespaceDecl.Module name ->
                        Some(
                            IdentListNode(
                                name
                                |> Array.map(fun name -> IdentifierOrDot.Ident(SingleTextNode.Create(name)))
                                |> List.ofArray
                                |> List.intersperse(IdentifierOrDot.KnownDot(SingleTextNode.Create("."))),
                                Range.Zero
                            )
                        )
                    | ModuleOrNamespaceDecl.Namespace name ->
                        Some(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero))
                    | ModuleOrNamespaceDecl.Anonymous -> None

            let isRecursive =
                Widgets.tryGetScalarValue widget IsRecursive |> ValueOption.defaultValue false

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let header =
                if isAnonymous then
                    None
                else
                    Some(
                        ModuleOrNamespaceHeaderNode(
                            None,
                            None,
                            MultipleTextsNode(
                                [ match textNode with
                                  | Some textNode -> textNode
                                  | _ -> () ],
                                Range.Zero
                            ),
                            (if isNameSpace then None else accessControl),
                            isRecursive,
                            name,
                            Range.Zero
                        )
                    )

            ModuleOrNamespaceNode(header, decls, Range.Zero))

[<AutoOpen>]
module ModuleOrNamespaceBuilders =
    type Ast with
        static member ModuleOrNamespace(name: string) =
            CollectionBuilder<ModuleOrNamespaceNode, ModuleDecl>(
                ModuleOrNamespace.WidgetKey,
                ModuleOrNamespace.Decls,
                AttributesBundle(StackList.one(ModuleOrNamespace.Name.WithValue(name)), Array.empty, Array.empty)
            )

        static member AnonymousModule() =
            CollectionBuilder<ModuleOrNamespaceNode, ModuleDecl>(
                ModuleOrNamespace.WidgetKey,
                ModuleOrNamespace.Decls,
                AttributesBundle(
                    StackList.one(ModuleOrNamespace.IsAnonymousModule.WithValue(true)),
                    Array.empty,
                    Array.empty
                )
            )

[<Extension>]
type ModuleOrNamespaceModifiers =
    [<Extension>]
    static member inline toRecursive(this: WidgetBuilder<ModuleOrNamespaceNode>) =
        this.AddScalar(ModuleOrNamespace.IsRecursive.WithValue(true))

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<ModuleOrNamespaceNode>) =
        this.AddScalar(ModuleOrNamespace.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<ModuleOrNamespaceNode>) =
        this.AddScalar(ModuleOrNamespace.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<ModuleOrNamespaceNode>) =
        this.AddScalar(ModuleOrNamespace.Accessibility.WithValue(AccessControl.Internal))

[<Extension>]
type ModuleOrNamespaceExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleOrNamespaceNode>, x: WidgetBuilder<Expr>)
        : CollectionContent =
        let node = Gen.mkOak x
        let moduleDecl = ModuleDecl.DeclExpr(node)
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
