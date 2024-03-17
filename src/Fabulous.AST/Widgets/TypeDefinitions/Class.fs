namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections

module Class =
    let Name = Attributes.defineScalar<string> "Name"
    let ImplicitConstructor = Attributes.defineWidget "SimplePats"
    let Members = Attributes.defineWidgetCollection "Members"
    let MultipleAttributes = Attributes.defineWidgetCollection "MultipleAttributes"
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"
    let TypeParams = Attributes.defineScalar<string list> "TypeParams"

    let IsClass = Attributes.defineScalar<bool> "IsClass"

    let WidgetKey =
        Widgets.register "Class" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name
                |> Unquoted
                |> StringParsing.normalizeIdentifierBackticks

            let constructor =
                Widgets.tryGetNodeFromWidget<ImplicitConstructorNode> widget ImplicitConstructor

            let members = Widgets.tryGetNodesFromWidgetCollection<MemberDefn> widget Members
            let typeParams = Widgets.tryGetScalarValue widget TypeParams
            let isClass = Widgets.getScalarValue widget IsClass

            let typeParams =
                match typeParams with
                | ValueSome values ->
                    TyparDeclsPostfixListNode(
                        SingleTextNode.lessThan,
                        [ for v in values do
                              TyparDeclNode(None, SingleTextNode.Create v, [], Range.Zero) ],
                        [],
                        SingleTextNode.greaterThan,
                        Range.Zero
                    )
                    |> TyparDecls.PostfixList
                    |> Some
                | ValueNone -> None

            let lines = Widgets.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            let attributes =
                Widgets.tryGetNodesFromWidgetCollection<AttributeNode> widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | Some values ->
                    Some(
                        MultipleAttributeListNode(
                            [ AttributeListNode(
                                  SingleTextNode.leftAttribute,
                                  values,
                                  SingleTextNode.rightAttribute,
                                  Range.Zero
                              ) ],
                            Range.Zero
                        )
                    )
                | None -> None

            let members =
                match members with
                | None -> []
                | Some members -> members

            let constructor =
                match constructor with
                | ValueNone when not isClass -> None
                | ValueNone ->
                    let unitNode =
                        UnitNode(SingleTextNode.leftParenthesis, SingleTextNode.rightParenthesis, Range.Zero)

                    let constructor =
                        ImplicitConstructorNode(None, None, None, Pattern.Unit(unitNode), None, Range.Zero)

                    Some constructor

                | ValueSome constructor -> Some constructor

            TypeDefnRegularNode(
                TypeNameNode(
                    xmlDocs,
                    multipleAttributes,
                    SingleTextNode.``type``,
                    Some(SingleTextNode.Create(name)),
                    IdentListNode([], Range.Zero),
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
module ClassBuilders =
    type Ast with
        static member BaseClass(name: string, constructor: WidgetBuilder<ImplicitConstructorNode> voption) =
            CollectionBuilder<TypeDefnRegularNode, MemberDefn>(
                Class.WidgetKey,
                Class.Members,
                AttributesBundle(
                    StackList.two(Class.Name.WithValue(name), Class.IsClass.WithValue(true)),
                    [| match constructor with
                       | ValueSome constructor -> Class.ImplicitConstructor.WithValue(constructor.Compile())
                       | ValueNone -> () |],
                    Array.empty
                )
            )

        static member Class(name: string) = Ast.BaseClass(name, ValueNone)

        static member Class(name: string, constructor: WidgetBuilder<ImplicitConstructorNode>) =
            Ast.BaseClass(name, ValueSome constructor)

        static member Interface(name: string) =
            CollectionBuilder<TypeDefnRegularNode, MemberDefn>(
                Class.WidgetKey,
                Class.Members,
                AttributesBundle(
                    StackList.two(Class.Name.WithValue(name), Class.IsClass.WithValue(false)),
                    Array.empty,
                    Array.empty
                )
            )

[<Extension>]
type ClassModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnRegularNode>, xmlDocs: string list) =
        this.AddScalar(Class.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline typeParams(this: WidgetBuilder<TypeDefnRegularNode>, typeParams: string list) =
        this.AddScalar(Class.TypeParams.WithValue(typeParams))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<TypeDefnRegularNode>) =
        AttributeCollectionBuilder<TypeDefnRegularNode, AttributeNode>(this, Class.MultipleAttributes)

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<TypeDefnRegularNode>, attributes: string list) =
        AttributeCollectionBuilder<TypeDefnRegularNode, AttributeNode>(this, Class.MultipleAttributes) {
            for attribute in attributes do
                Ast.Attribute(attribute)
        }

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefnRegularNode>, attribute: WidgetBuilder<AttributeNode>) =
        AttributeCollectionBuilder<TypeDefnRegularNode, AttributeNode>(this, Class.MultipleAttributes) { attribute }

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefnRegularNode>, attribute: string) =
        AttributeCollectionBuilder<TypeDefnRegularNode, AttributeNode>(this, Class.MultipleAttributes) {
            Ast.Attribute(attribute)
        }

[<Extension>]
type ClassYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefnRegularNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let typeDefn = TypeDefn.Regular(node)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)

        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<TypeDefnRegularNode, MemberDefn>, x: BindingNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<TypeDefnRegularNode, MemberDefn>, x: WidgetBuilder<BindingNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ClassYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<TypeDefnRegularNode, MemberDefn>, x: MemberDefnAbstractSlotNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.AbstractSlot(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<TypeDefnRegularNode, MemberDefn>, x: WidgetBuilder<MemberDefnAbstractSlotNode>) : CollectionContent =
        let node = Gen.mkOak x
        ClassYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<TypeDefnRegularNode, MemberDefn>, x: MemberDefnInterfaceNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Interface(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<TypeDefnRegularNode, MemberDefn>, x: WidgetBuilder<MemberDefnInterfaceNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ClassYieldExtensions.Yield(this, node)
