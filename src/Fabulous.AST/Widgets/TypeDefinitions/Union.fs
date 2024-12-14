namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module Union =

    let UnionCaseNode = Attributes.defineWidgetCollection "UnionCaseNode"

    let Name = Attributes.defineScalar<string> "Name"

    let Members = Attributes.defineWidgetCollection "Members"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let XmlDocs = Attributes.defineWidget "XmlDocs"

    let TypeParams = Attributes.defineWidget "TypeParams"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let IsRecursive = Attributes.defineScalar<bool> "IsRecursive"

    let WidgetKey =
        Widgets.register "Union" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

            let unionCaseNode =
                Widgets.getNodesFromWidgetCollection<UnionCaseNode> widget UnionCaseNode

            let members =
                Widgets.tryGetNodesFromWidgetCollection<MemberDefn> widget Members
                |> ValueOption.defaultValue []

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let typeParams =
                Widgets.tryGetNodeFromWidget widget TypeParams
                |> ValueOption.map Some
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

            let isRecursive =
                Widgets.tryGetScalarValue widget IsRecursive |> ValueOption.defaultValue false

            let leadingKeyword =
                if isRecursive then
                    SingleTextNode.``and``
                else
                    SingleTextNode.``type``

            TypeDefnUnionNode(
                TypeNameNode(
                    xmlDocs,
                    attributes,
                    leadingKeyword,
                    None,
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero),
                    typeParams,
                    [],
                    None,
                    Some(SingleTextNode.equals),
                    None,
                    Range.Zero
                ),
                accessControl,
                unionCaseNode,
                members,
                Range.Zero
            ))

[<AutoOpen>]
module UnionBuilders =
    type Ast with
        static member Union(name: string) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name

            CollectionBuilder<TypeDefnUnionNode, UnionCaseNode>(
                Union.WidgetKey,
                Union.UnionCaseNode,
                Union.Name.WithValue(name)
            )

type UnionModifiers =
    [<Extension>]
    static member inline members(this: WidgetBuilder<TypeDefnUnionNode>) =
        AttributeCollectionBuilder<TypeDefnUnionNode, MemberDefn>(this, Union.Members)

    [<Extension>]
    static member inline typeParams(this: WidgetBuilder<TypeDefnUnionNode>, typeParams: WidgetBuilder<TyparDecls>) =
        this.AddWidget(Union.TypeParams.WithValue(typeParams.Compile()))

    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnUnionNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(Union.XmlDocs.WithValue(xmlDocs.Compile()))

    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnUnionNode>, xmlDocs: string list) =
        UnionModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<TypeDefnUnionNode>, attributes: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            Union.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefnUnionNode>, attribute: WidgetBuilder<AttributeNode>) =
        UnionModifiers.attributes(this, [ attribute ])

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<TypeDefnUnionNode>) =
        this.AddScalar(Union.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<TypeDefnUnionNode>) =
        this.AddScalar(Union.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<TypeDefnUnionNode>) =
        this.AddScalar(Union.Accessibility.WithValue(AccessControl.Internal))

    [<Extension>]
    static member inline toRecursive(this: WidgetBuilder<TypeDefnUnionNode>) =
        this.AddScalar(Union.IsRecursive.WithValue(true))

type UnionYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: TypeDefnUnionNode) : CollectionContent =
        let typeDefn = TypeDefn.Union(x)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefnUnionNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        UnionYieldExtensions.Yield(this, node)
