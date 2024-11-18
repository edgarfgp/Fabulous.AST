namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ModuleOrNamespace =
    let Decls = Attributes.defineWidgetCollection "Decls"
    let IsRecursive = Attributes.defineScalar<bool> "IsRecursive"
    let HeaderName = Attributes.defineScalar<string> "HeaderName"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"
    let IsImplicit = Attributes.defineScalar<bool> "IsImplicit"
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"
    let IsGlobal = Attributes.defineScalar<bool> "IsGlobal"

    let IsAnonymousModule = Attributes.defineScalar<bool> "IsAnonymousModule"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let WidgetKey =
        Widgets.register "Namespace" (fun widget ->
            let decls = Widgets.getNodesFromWidgetCollection<ModuleDecl> widget Decls

            let isAnonymousModule =
                Widgets.tryGetScalarValue widget IsAnonymousModule
                |> ValueOption.defaultValue false

            if isAnonymousModule then
                ModuleOrNamespaceNode(None, decls, Range.Zero)
            else
                let isGlobal =
                    Widgets.tryGetScalarValue widget IsGlobal |> ValueOption.defaultValue false

                let isImplicit =
                    Widgets.tryGetScalarValue widget IsImplicit |> ValueOption.defaultValue false

                let headerName =
                    if isGlobal then
                        SingleTextNode.``global``
                    else
                        SingleTextNode.Create(Widgets.getScalarValue widget HeaderName)

                let isRecursive =
                    Widgets.tryGetScalarValue widget IsRecursive |> ValueOption.defaultValue false

                let leadingKeyword =
                    if isImplicit then
                        SingleTextNode.``module``
                    else
                        SingleTextNode.``namespace``

                let accessControl =
                    Widgets.tryGetScalarValue widget Accessibility
                    |> ValueOption.defaultValue AccessControl.Unknown

                let accessControl =
                    match accessControl with
                    | Public -> Some(SingleTextNode.``public``)
                    | Private -> Some(SingleTextNode.``private``)
                    | Internal -> Some(SingleTextNode.``internal``)
                    | Unknown -> None

                let lines = Widgets.tryGetScalarValue widget XmlDocs

                let xmlDocs =
                    match lines with
                    | ValueSome values ->
                        let xmlDocNode = XmlDocNode.Create(values)
                        Some xmlDocNode
                    | ValueNone -> None

                let attributes =
                    Widgets.tryGetScalarValue widget MultipleAttributes
                    |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                    |> ValueOption.defaultValue None

                let header =
                    Some(
                        ModuleOrNamespaceHeaderNode(
                            xmlDocs,
                            attributes,
                            MultipleTextsNode([ leadingKeyword ], Range.Zero),
                            accessControl,
                            isRecursive,
                            Some(IdentListNode([ IdentifierOrDot.Ident(headerName) ], Range.Zero)),
                            Range.Zero
                        )
                    )

                ModuleOrNamespaceNode(header, decls, Range.Zero))

[<AutoOpen>]
module NamespaceBuilders =
    type Ast with
        static member Namespace(name: string) =
            CollectionBuilder<ModuleOrNamespaceNode, ModuleDecl>(
                ModuleOrNamespace.WidgetKey,
                ModuleOrNamespace.Decls,
                AttributesBundle(
                    StackList.two(
                        ModuleOrNamespace.HeaderName.WithValue(name),
                        ModuleOrNamespace.IsAnonymousModule.WithValue(false)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member GlobalNamespace() =
            CollectionBuilder<ModuleOrNamespaceNode, ModuleDecl>(
                ModuleOrNamespace.WidgetKey,
                ModuleOrNamespace.Decls,
                AttributesBundle(
                    StackList.two(
                        ModuleOrNamespace.IsGlobal.WithValue(true),
                        ModuleOrNamespace.IsAnonymousModule.WithValue(false)
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
                    StackList.one(ModuleOrNamespace.IsAnonymousModule.WithValue(true)),
                    Array.empty,
                    Array.empty
                )
            )

type NamespaceModifiers =
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
    static member inline xmlDocs(this: WidgetBuilder<ModuleOrNamespaceNode>, xmlDocs: string list) =
        this.AddScalar(ModuleOrNamespace.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<ModuleOrNamespaceNode>, attributes: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            ModuleOrNamespace.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    [<Extension>]
    static member inline attribute
        (this: WidgetBuilder<ModuleOrNamespaceNode>, attribute: WidgetBuilder<AttributeNode>)
        =
        NamespaceModifiers.attributes(this, [ attribute ])

    [<Extension>]
    static member inline toImplicit(this: WidgetBuilder<ModuleOrNamespaceNode>) =
        this.AddScalar(ModuleOrNamespace.IsImplicit.WithValue(true))
