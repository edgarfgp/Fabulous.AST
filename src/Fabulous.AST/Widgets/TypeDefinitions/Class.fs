namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections

module Class =
    let Name = Attributes.defineScalar<string> "Name"
    let SimplePats = Attributes.defineWidget "SimplePats"
    let Members = Attributes.defineWidgetCollection "Members"
    let MultipleAttributes = Attributes.defineWidgetCollection "MultipleAttributes"

    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"
    let TypeParams = Attributes.defineScalar<string list> "TypeParams"
    let IsClass = Attributes.defineScalar<bool> "IsClass"

    let WidgetKey =
        Widgets.register "TypeDefnRegularNode" (fun widget ->
            let name = Helpers.getScalarValue widget Name

            let implicitConstructor =
                Helpers.tryGetNodeFromWidget<ImplicitConstructorNode> widget SimplePats

            let members = Helpers.tryGetNodesFromWidgetCollection<MemberDefn> widget Members
            let typeParams = Helpers.tryGetScalarValue widget TypeParams
            let isClass = Helpers.getScalarValue widget IsClass

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

            let lines = Helpers.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            let attributes =
                Helpers.tryGetNodesFromWidgetCollection<AttributeNode> widget MultipleAttributes

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

            let implicitConstructor =
                match implicitConstructor with
                | ValueNone when not isClass -> None
                | ValueNone ->
                    let implicitConstructorNode =
                        ImplicitConstructorNode(
                            None,
                            None,
                            None,
                            SingleTextNode.leftParenthesis,
                            [],
                            SingleTextNode.rightParenthesis,
                            None,
                            Range.Zero
                        )

                    Some implicitConstructorNode
                | ValueSome implicitConstructor -> Some implicitConstructor

            TypeDefnRegularNode(
                TypeNameNode(
                    xmlDocs,
                    multipleAttributes,
                    SingleTextNode.``type``,
                    Some(SingleTextNode.Create(name)),
                    IdentListNode([], Range.Zero),
                    typeParams,
                    [],
                    implicitConstructor,
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
        static member BaseClass
            (
                name: string,
                typeParams: string list voption,
                constructor: WidgetBuilder<ImplicitConstructorNode> voption,
                isClass: bool
            ) =
            let scalars =
                match typeParams with
                | ValueNone -> StackList.two(Class.Name.WithValue(name), Class.IsClass.WithValue(isClass))
                | ValueSome typeParams ->
                    StackList.three(
                        Class.Name.WithValue(name),
                        Class.TypeParams.WithValue(typeParams),
                        Class.IsClass.WithValue(isClass)
                    )

            CollectionBuilder<TypeDefnRegularNode, MemberDefn>(
                Class.WidgetKey,
                Class.Members,
                AttributesBundle(
                    scalars,
                    ValueSome
                        [| match constructor with
                           | ValueSome constructor -> Class.SimplePats.WithValue(constructor.Compile())
                           | ValueNone -> () |],
                    ValueNone
                )
            )

        static member Class(name: string) =
            Ast.BaseClass(name, ValueNone, ValueNone, true)

        static member Class(name: string, constructor: WidgetBuilder<ImplicitConstructorNode>) =
            Ast.BaseClass(name, ValueNone, ValueSome constructor, true)

        static member Class(name: string, typeParams: string list) =
            Ast.BaseClass(name, ValueSome typeParams, ValueNone, true)

        static member Class
            (
                name: string,
                typeParams: string list,
                constructor: WidgetBuilder<ImplicitConstructorNode>
            ) =
            Ast.BaseClass(name, ValueSome typeParams, ValueSome constructor, true)

        static member Interface(name: string) =
            Ast.BaseClass(name, ValueNone, ValueNone, false)

        static member Interface(name: string, typeParams: string list) =
            Ast.BaseClass(name, ValueSome typeParams, ValueNone, false)

[<Extension>]
type ClassModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnRegularNode>, xmlDocs: string list) =
        this.AddScalar(Class.XmlDocs.WithValue(xmlDocs))

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
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<TypeDefnRegularNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let typeDefn = TypeDefn.Regular(node)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)

        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<TypeDefnRegularNode, MemberDefn>,
            x: BindingNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            this: CollectionBuilder<TypeDefnRegularNode, MemberDefn>,
            x: WidgetBuilder<BindingNode>
        ) : CollectionContent =
        let node = Tree.compile x
        ClassYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<TypeDefnRegularNode, MemberDefn>,
            x: MemberDefnAbstractSlotNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.AbstractSlot(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            this: CollectionBuilder<TypeDefnRegularNode, MemberDefn>,
            x: WidgetBuilder<MemberDefnAbstractSlotNode>
        ) : CollectionContent =
        let node = Tree.compile x
        ClassYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<TypeDefnRegularNode, MemberDefn>,
            x: MemberDefnInterfaceNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Interface(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            this: CollectionBuilder<TypeDefnRegularNode, MemberDefn>,
            x: WidgetBuilder<MemberDefnInterfaceNode>
        ) : CollectionContent =
        let node = Tree.compile x
        ClassYieldExtensions.Yield(this, node)
