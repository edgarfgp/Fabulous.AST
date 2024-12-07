namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module TypeDefnRegular =
    let Name = Attributes.defineScalar<string> "Name"
    let ImplicitConstructor = Attributes.defineWidget "SimplePats"
    let Members = Attributes.defineWidgetCollection "Members"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"
    let TypeParams = Attributes.defineWidget "TypeParams"

    let Constraints = Attributes.defineWidget "Constraints"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let WidgetKey =
        Widgets.register "TypeDefnRegular" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

            let constructor =
                Widgets.tryGetNodeFromWidget<ImplicitConstructorNode> widget ImplicitConstructor

            let members =
                Widgets.tryGetNodesFromWidgetCollection<MemberDefn> widget Members
                |> ValueOption.defaultValue []

            let typeParams =
                Widgets.tryGetNodeFromWidget widget TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let constraints =
                Widgets.tryGetNodeFromWidget<TypeConstraint list> widget Constraints
                |> ValueOption.defaultValue []

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

            let constructor =
                match constructor with
                | ValueNone -> None
                | ValueSome constructor -> Some constructor

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            TypeDefnRegularNode(
                TypeNameNode(
                    xmlDocs,
                    attributes,
                    SingleTextNode.``type``,
                    accessControl,
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero),
                    typeParams,
                    constraints,
                    constructor,
                    Some(SingleTextNode.equals),
                    None,
                    Range.Zero
                ),
                members,
                Range.Zero
            ))

[<AutoOpen>]
module TypeDefnRegularBuilders =
    type Ast with
        static member BaseTypeDefn(name: string, constructor: WidgetBuilder<ImplicitConstructorNode> voption) =
            CollectionBuilder<TypeDefnRegularNode, MemberDefn>(
                TypeDefnRegular.WidgetKey,
                TypeDefnRegular.Members,
                AttributesBundle(
                    StackList.one(TypeDefnRegular.Name.WithValue(name)),
                    [| match constructor with
                       | ValueSome constructor -> TypeDefnRegular.ImplicitConstructor.WithValue(constructor.Compile())
                       | ValueNone -> () |],
                    Array.empty
                )
            )

        static member TypeDefn(name: string, parameters: WidgetBuilder<Pattern>) =
            Ast.BaseTypeDefn(name, ValueSome(Ast.ImplicitConstructor parameters))

        static member TypeDefn(name: string, constructor: WidgetBuilder<ImplicitConstructorNode>) =
            Ast.BaseTypeDefn(name, ValueSome constructor)

        static member TypeDefn(name: string) = Ast.BaseTypeDefn(name, ValueNone)

type TypeDefnRegularModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnRegularNode>, xmlDocs: string list) =
        this.AddScalar(TypeDefnRegular.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline typeParams(this: WidgetBuilder<TypeDefnRegularNode>, typeParams: WidgetBuilder<TyparDecls>) =
        this.AddWidget(TypeDefnRegular.TypeParams.WithValue(typeParams.Compile()))

    [<Extension>]
    static member inline constraints
        (this: WidgetBuilder<TypeDefnRegularNode>, constraints: WidgetBuilder<TypeConstraint list>)
        =
        this.AddWidget(TypeDefnRegular.Constraints.WithValue(constraints.Compile()))

    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<TypeDefnRegularNode>, attributes: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            TypeDefnRegular.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefnRegularNode>, attribute: WidgetBuilder<AttributeNode>) =
        TypeDefnRegularModifiers.attributes(this, [ attribute ])

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<TypeDefnRegularNode>) =
        this.AddScalar(TypeDefnRegular.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<TypeDefnRegularNode>) =
        this.AddScalar(TypeDefnRegular.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<TypeDefnRegularNode>) =
        this.AddScalar(TypeDefnRegular.Accessibility.WithValue(AccessControl.Internal))

type TypeDefnRegularYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: TypeDefnRegularNode) : CollectionContent =
        let typeDefn = TypeDefn.Regular(x)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)

        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefnRegularNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        TypeDefnRegularYieldExtensions.Yield(this, node)
