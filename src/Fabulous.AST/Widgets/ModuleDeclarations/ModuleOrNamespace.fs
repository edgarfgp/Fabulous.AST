namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

(*

Top-level module

module A or A.B
let x = 0

// Nested module
module X =
    let x = 0

// Namespace
namespace A.B

*)

module ModuleOrNamespace =
    let Decls = Attributes.defineWidgetCollection "Decls"
    let IsRecursive = Attributes.defineScalar<bool> "IsRecursive"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let ModuleOrNamespace =
        Attributes.defineScalar<ModuleOrNamespaceDecl> "ModuleOrNamespaceDecl"

    let WidgetKey =
        Widgets.register "Namespace" (fun widget ->
            let decls = Widgets.getNodesFromWidgetCollection<ModuleDecl> widget Decls
            let moduleOrNamespaceDecl = Widgets.getScalarValue widget ModuleOrNamespace

            let name =
                match moduleOrNamespaceDecl with
                | ModuleOrNamespaceDecl.AnonymousModule -> None
                | ModuleOrNamespaceDecl.TopLevelModule name
                | ModuleOrNamespaceDecl.Namespace name ->
                    Some(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero))

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
                match moduleOrNamespaceDecl with
                | ModuleOrNamespaceDecl.TopLevelModule _ ->
                    Some(
                        ModuleOrNamespaceHeaderNode(
                            None,
                            None,
                            MultipleTextsNode(
                                [ match moduleOrNamespaceDecl with
                                  | ModuleOrNamespaceDecl.AnonymousModule -> ()
                                  | ModuleOrNamespaceDecl.TopLevelModule _ -> SingleTextNode.``module``
                                  | ModuleOrNamespaceDecl.Namespace _ -> SingleTextNode.``namespace`` ],
                                Range.Zero
                            ),
                            accessControl,
                            isRecursive,
                            name,
                            Range.Zero
                        )
                    )
                | ModuleOrNamespaceDecl.Namespace _ ->
                    Some(
                        ModuleOrNamespaceHeaderNode(
                            None,
                            None,
                            MultipleTextsNode(
                                [ match moduleOrNamespaceDecl with
                                  | ModuleOrNamespaceDecl.AnonymousModule -> ()
                                  | ModuleOrNamespaceDecl.TopLevelModule _ -> SingleTextNode.``module``
                                  | ModuleOrNamespaceDecl.Namespace _ -> SingleTextNode.``namespace`` ],
                                Range.Zero
                            ),
                            None,
                            isRecursive,
                            name,
                            Range.Zero
                        )
                    )
                | ModuleOrNamespaceDecl.AnonymousModule -> None

            ModuleOrNamespaceNode(header, decls, Range.Zero))

[<AutoOpen>]
module ModuleOrNamespaceBuilders =
    type Ast with
        static member Namespace(name: string) =
            CollectionBuilder<ModuleOrNamespaceNode, ModuleDecl>(
                ModuleOrNamespace.WidgetKey,
                ModuleOrNamespace.Decls,
                AttributesBundle(
                    StackList.one(ModuleOrNamespace.ModuleOrNamespace.WithValue(ModuleOrNamespaceDecl.Namespace(name))),
                    Array.empty,
                    Array.empty
                )
            )

        static member TopLevelModule(name: string) =
            CollectionBuilder<ModuleOrNamespaceNode, ModuleDecl>(
                ModuleOrNamespace.WidgetKey,
                ModuleOrNamespace.Decls,
                AttributesBundle(
                    StackList.one(
                        ModuleOrNamespace.ModuleOrNamespace.WithValue(ModuleOrNamespaceDecl.TopLevelModule(name))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member AnonymousModule() =
            CollectionBuilder<ModuleOrNamespaceNode, ModuleDecl>(
                ModuleOrNamespace.WidgetKey,
                ModuleOrNamespace.Decls,
                AttributesBundle(
                    StackList.one(ModuleOrNamespace.ModuleOrNamespace.WithValue(ModuleOrNamespaceDecl.AnonymousModule)),
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
