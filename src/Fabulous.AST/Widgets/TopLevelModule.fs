namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

type TopLevelModuleNode(header, decls) =
    inherit ModuleOrNamespaceNode(header, decls, Range.Zero)

module TopLevelModule =
    let Decls = Attributes.defineWidgetCollection "Decls"
    let IsRecursive = Attributes.defineScalar<bool> "IsRecursive"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"
    let HeaderName = Attributes.defineScalar<string> "HeaderName"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let WidgetKey =
        Widgets.register "Namespace" (fun widget ->
            let decls = Widgets.getNodesFromWidgetCollection<ModuleDecl> widget Decls

            let headerName =
                Widgets.tryGetScalarValue widget HeaderName
                |> ValueOption.map(fun name ->
                    Some(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero)))
                |> ValueOption.defaultValue None

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
                        MultipleTextsNode([ SingleTextNode.``module`` ], Range.Zero),
                        accessControl,
                        isRecursive,
                        headerName,
                        Range.Zero
                    )
                )

            TopLevelModuleNode(header, decls))

[<AutoOpen>]
module TopLevelModuleBuilders =
    type Ast with
        static member TopLevelModule(name: string) =
            CollectionBuilder<TopLevelModuleNode, ModuleDecl>(
                TopLevelModule.WidgetKey,
                TopLevelModule.Decls,
                AttributesBundle(StackList.one(TopLevelModule.HeaderName.WithValue(name)), Array.empty, Array.empty)
            )

type TopLevelModuleModifiers =
    [<Extension>]
    static member inline toRecursive(this: WidgetBuilder<TopLevelModuleNode>) =
        this.AddScalar(TopLevelModule.IsRecursive.WithValue(true))

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<TopLevelModuleNode>) =
        this.AddScalar(TopLevelModule.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<TopLevelModuleNode>) =
        this.AddScalar(TopLevelModule.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<TopLevelModuleNode>) =
        this.AddScalar(TopLevelModule.Accessibility.WithValue(AccessControl.Internal))

    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TopLevelModuleNode>, xmlDocs: string list) =
        this.AddScalar(TopLevelModule.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<TopLevelModuleNode>, attributes: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            TopLevelModule.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TopLevelModuleNode>, attribute: WidgetBuilder<AttributeNode>) =
        TopLevelModuleModifiers.attributes(this, [ attribute ])
