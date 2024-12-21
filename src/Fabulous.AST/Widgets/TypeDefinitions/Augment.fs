namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module Augmentation =
    let XmlDocs = Attributes.defineWidget "XmlDocs"

    let Name = Attributes.defineScalar<string> "Name"
    let Members = Attributes.defineWidgetCollection "Members"

    let TypeParams = Attributes.defineWidget "TypeParams"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let WidgetKey =
        Widgets.register "Augmentation" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

            let members =
                Widgets.tryGetNodesFromWidgetCollection<MemberDefn> widget Members
                |> ValueOption.defaultValue []

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let typeParams =
                Widgets.tryGetNodeFromWidget widget TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget XmlDocs
                |> ValueOption.map(fun x -> Some(x))
                |> ValueOption.defaultValue None

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            TypeDefnAugmentationNode(
                TypeNameNode(
                    xmlDocs,
                    attributes,
                    SingleTextNode.``type``,
                    accessControl,
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero),
                    typeParams,
                    [],
                    None,
                    None,
                    Some SingleTextNode.``with``,
                    Range.Zero
                ),
                members,
                Range.Zero
            ))

[<AutoOpen>]
module AugmentBuilders =
    type Ast with

        static member Augmentation(name: string) =
            CollectionBuilder<TypeDefnAugmentationNode, MemberDefn>(
                Augmentation.WidgetKey,
                Augmentation.Members,
                Augmentation.Name.WithValue(name)
            )

type AugmentationModifiers =
    [<Extension>]
    static member inline typeParams
        (this: WidgetBuilder<TypeDefnAugmentationNode>, typeParams: WidgetBuilder<TyparDecls>)
        =
        this.AddWidget(Augmentation.TypeParams.WithValue(typeParams.Compile()))

    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnAugmentationNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(Augmentation.XmlDocs.WithValue(xmlDocs.Compile()))

    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnAugmentationNode>, xmlDocs: string list) =
        AugmentationModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<TypeDefnAugmentationNode>) =
        this.AddScalar(Augmentation.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<TypeDefnAugmentationNode>) =
        this.AddScalar(Augmentation.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<TypeDefnAugmentationNode>) =
        this.AddScalar(Augmentation.Accessibility.WithValue(AccessControl.Internal))

    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<TypeDefnAugmentationNode>, attributes: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            Augmentation.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    [<Extension>]
    static member inline attribute
        (this: WidgetBuilder<TypeDefnAugmentationNode>, attribute: WidgetBuilder<AttributeNode>)
        =
        AugmentationModifiers.attributes(this, [ attribute ])

type AugmentYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: TypeDefnAugmentationNode)
        : CollectionContent =
        let typeDefn = TypeDefn.Augmentation(x)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefnAugmentationNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        AugmentYieldExtensions.Yield(this, node)
