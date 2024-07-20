namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module TypeDefnExplicit =
    let Name = Attributes.defineScalar<string> "Name"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let TypeParams = Attributes.defineWidget "TypeParams"

    let Constructor = Attributes.defineWidget "Constructor"
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let Kind = Attributes.defineScalar<SingleTextNode> "Kind"

    let Members = Attributes.defineWidgetCollection "Members"

    let WidgetKey =
        Widgets.register "TypeDefnExplicit" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let lines = Widgets.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            let typeParams =
                Widgets.tryGetNodeFromWidget widget TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let implicitConstructor =
                Widgets.tryGetNodeFromWidget<ImplicitConstructorNode> widget Constructor

            let implicitConstructor =
                match implicitConstructor with
                | ValueNone -> None
                | ValueSome implicitConstructor -> Some implicitConstructor

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let kind = Widgets.getScalarValue widget Kind

            let memDefns =
                Widgets.tryGetNodesFromWidgetCollection widget Members
                |> ValueOption.defaultValue []

            TypeDefnExplicitNode(
                TypeNameNode(
                    xmlDocs,
                    attributes,
                    SingleTextNode.``type``,
                    accessControl,
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero),
                    typeParams,
                    [],
                    implicitConstructor,
                    Some(SingleTextNode.equals),
                    None,
                    Range.Zero
                ),
                TypeDefnExplicitBodyNode(kind, memDefns, SingleTextNode.``end``, Range.Zero),
                [],
                Range.Zero
            ))

[<AutoOpen>]
module TypeDefnExplicitBuilders =
    type Ast with

        static member private BaseClassEnd
            (name: string, constructor: WidgetBuilder<ImplicitConstructorNode> voption, kind: SingleTextNode)
            =
            WidgetBuilder<TypeDefnExplicitNode>(
                TypeDefnExplicit.WidgetKey,
                AttributesBundle(
                    StackList.two(TypeDefnExplicit.Name.WithValue(name), TypeDefnExplicit.Kind.WithValue(kind)),
                    [| match constructor with
                       | ValueNone -> ()
                       | ValueSome value -> TypeDefnExplicit.Constructor.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member ClassEnd(name: string) =
            Ast.BaseClassEnd(name, ValueNone, SingleTextNode.``class``)

        static member ClassEnd(name: string, constructor: WidgetBuilder<ImplicitConstructorNode>) =
            Ast.BaseClassEnd(name, ValueSome constructor, SingleTextNode.``class``)

        static member StructEnd(name: string) =
            Ast.BaseClassEnd(name, ValueNone, SingleTextNode.``struct``)

        static member StructEnd(name: string, constructor: WidgetBuilder<ImplicitConstructorNode>) =
            Ast.BaseClassEnd(name, ValueSome constructor, SingleTextNode.``struct``)

        static member InterfaceEnd(name: string) =
            Ast.BaseClassEnd(name, ValueNone, SingleTextNode.``interface``)

type TypeDefnExplicitModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnExplicitNode>, xmlDocs: string list) =
        this.AddScalar(TypeDefnExplicit.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<TypeDefnExplicitNode>, values: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            TypeDefnExplicit.MultipleAttributes.WithValue(
                [ for vals in values do
                      Gen.mkOak vals ]
            )
        )

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefnExplicitNode>, attribute: WidgetBuilder<AttributeNode>) =
        TypeDefnExplicitModifiers.attributes(this, [ attribute ])

    [<Extension>]
    static member inline typeParams(this: WidgetBuilder<TypeDefnExplicitNode>, typeParams: WidgetBuilder<TyparDecls>) =
        this.AddWidget(TypeDefnExplicit.TypeParams.WithValue(typeParams.Compile()))

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<TypeDefnExplicitNode>) =
        this.AddScalar(TypeDefnExplicit.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<TypeDefnExplicitNode>) =
        this.AddScalar(TypeDefnExplicit.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<TypeDefnExplicitNode>) =
        this.AddScalar(TypeDefnExplicit.Accessibility.WithValue(AccessControl.Internal))

    [<Extension>]
    static member inline members(this: WidgetBuilder<TypeDefnExplicitNode>) =
        AttributeCollectionBuilder<TypeDefnExplicitNode, MemberDefn>(this, TypeDefnExplicit.Members)

type TypeDefnExplicitYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: TypeDefnExplicitNode) : CollectionContent =
        let typeDefn = TypeDefn.Explicit(x)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefnExplicitNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        TypeDefnExplicitYieldExtensions.Yield(this, node)
