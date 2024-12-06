namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.Intersection
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

    let IsClass = Attributes.defineScalar<bool> "IsClass"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let WidgetKey =
        Widgets.register "TypeDefnRegular" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

            let constructor =
                Widgets.tryGetNodeFromWidget<ImplicitConstructorNode> widget ImplicitConstructor
                |> ValueOption.defaultValue(
                    ImplicitConstructorNode(
                        None,
                        None,
                        None,
                        Pattern.Unit(
                            UnitNode(SingleTextNode.leftParenthesis, SingleTextNode.rightParenthesis, Range.Zero)
                        ),
                        None,
                        Range.Zero
                    )
                )

            let members =
                Widgets.tryGetNodesFromWidgetCollection<MemberDefn> widget Members
                |> ValueOption.defaultValue []

            let typeParams =
                Widgets.tryGetNodeFromWidget widget TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let isClass = Widgets.getScalarValue widget IsClass

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

            let constructor = if not isClass then None else Some constructor

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
                    [],
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
        static member BaseClass(name: string, parameters: WidgetBuilder<ImplicitConstructorNode> voption) =
            CollectionBuilder<TypeDefnRegularNode, MemberDefn>(
                TypeDefnRegular.WidgetKey,
                TypeDefnRegular.Members,
                AttributesBundle(
                    StackList.two(TypeDefnRegular.Name.WithValue(name), TypeDefnRegular.IsClass.WithValue(true)),
                    [| match parameters with
                       | ValueSome parameters -> TypeDefnRegular.ImplicitConstructor.WithValue(parameters.Compile())
                       | ValueNone -> () |],
                    Array.empty
                )
            )

        static member Class(name: string) = Ast.BaseClass(name, ValueNone)

        static member Class(name: string, parameters: WidgetBuilder<Pattern>) =
            Ast.BaseClass(name, ValueSome(Ast.ImplicitConstructor(Ast.ParenPat(parameters))))

        static member Class(name: string, constructor: WidgetBuilder<ImplicitConstructorNode>) =
            Ast.BaseClass(name, ValueSome constructor)

        static member TypeDefn(name: string) =
            CollectionBuilder<TypeDefnRegularNode, MemberDefn>(
                TypeDefnRegular.WidgetKey,
                TypeDefnRegular.Members,
                AttributesBundle(
                    StackList.two(TypeDefnRegular.Name.WithValue(name), TypeDefnRegular.IsClass.WithValue(false)),
                    Array.empty,
                    Array.empty
                )
            )

type TypeDefnRegularModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnRegularNode>, xmlDocs: string list) =
        this.AddScalar(TypeDefnRegular.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline typeParams(this: WidgetBuilder<TypeDefnRegularNode>, typeParams: WidgetBuilder<TyparDecls>) =
        this.AddWidget(TypeDefnRegular.TypeParams.WithValue(typeParams.Compile()))

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
