namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

open type Fabulous.AST.Ast
open type Fantomas.Core.SyntaxOak.Oak

module NestedModule =
    let Name = Attributes.defineScalar<string> "Name"
    let IsRecursive = Attributes.defineScalar<bool> "IsRecursive"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"
    let Decls = Attributes.defineWidgetCollection "Decls"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let IsTopLevel = Attributes.defineScalar<bool> "IsTopLevel"

    let WidgetKey =
        Widgets.register "NestedModule" (fun widget ->
            let name = Widgets.getScalarValue widget Name

            let name =
                name |> PrettyNaming.NormalizeIdentifierBackticks |> SingleTextNode.Create

            let moduleDecls = Widgets.getNodesFromWidgetCollection<ModuleDecl> widget Decls

            let isRecursive =
                Widgets.tryGetScalarValue widget IsRecursive |> ValueOption.defaultValue false

            let isTopLevel =
                Widgets.tryGetScalarValue widget IsTopLevel |> ValueOption.defaultValue false

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

            NestedModuleNode(
                xmlDocs,
                attributes,
                SingleTextNode.``module``,
                accessControl,
                isRecursive,
                IdentListNode([ IdentifierOrDot.Ident(name) ], Range.Zero),
                SingleTextNode.equals,
                moduleDecls,
                Range.Zero
            ))

[<AutoOpen>]
module NestedModuleBuilders =
    type Ast with
        static member Module(name: string) =
            CollectionBuilder<NestedModuleNode, ModuleDecl>(
                NestedModule.WidgetKey,
                NestedModule.Decls,
                AttributesBundle(StackList.one(NestedModule.Name.WithValue(name)), Array.empty, Array.empty)
            )

type NestedModuleModifiers =
    [<Extension>]
    static member inline toRecursive(this: WidgetBuilder<NestedModuleNode>) =
        this.AddScalar(NestedModule.IsRecursive.WithValue(true))

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<NestedModuleNode>) =
        this.AddScalar(NestedModule.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<NestedModuleNode>) =
        this.AddScalar(NestedModule.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<NestedModuleNode>) =
        this.AddScalar(NestedModule.Accessibility.WithValue(AccessControl.Internal))

    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<NestedModuleNode>, xmlDocs: string list) =
        this.AddScalar(NestedModule.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<NestedModuleNode>, attributes: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            NestedModule.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<NestedModuleNode>, attribute: WidgetBuilder<AttributeNode>) =
        NestedModuleModifiers.attributes(this, [ attribute ])

type NestedModuleYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: NestedModuleNode) : CollectionContent =
        let moduleDecl = ModuleDecl.NestedModule x
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<NestedModuleNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        NestedModuleYieldExtensions.Yield(this, node)
