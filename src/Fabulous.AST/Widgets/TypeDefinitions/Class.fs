namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections

type ClassTypeDefnRegularNode
    (
        name: SingleTextNode,
        implicitConstructor: ImplicitConstructorNode option,
        members: MemberDefn list,
        multipleAttributes: MultipleAttributeListNode option
    ) =
    inherit
        TypeDefnRegularNode(
            TypeNameNode(
                None,
                multipleAttributes,
                SingleTextNode("type", Range.Zero),
                Some(name),
                IdentListNode([], Range.Zero),
                None,
                [],
                implicitConstructor,
                Some(SingleTextNode("=", Range.Zero)),
                None,
                Range.Zero
            ),
            members,
            Range.Zero
        )

module Class =
    let Name = Attributes.defineWidget "Name"
    let Parameters = Attributes.defineScalar<SimplePatNode list> "Parameters"
    let Members = Attributes.defineWidgetCollection "Members"
    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"

    let Interface = Attributes.defineWidgetCollection "Interface"

    let WidgetKey =
        Widgets.register "Class" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name
            let parameters = Helpers.tryGetScalarValue widget Parameters
            let members = Helpers.tryGetNodesFromWidgetCollection<MemberDefn> widget Members
            let attributes = Helpers.tryGetScalarValue widget MultipleAttributes

            let interfaceWidget =
                Helpers.tryGetNodesFromWidgetCollection<InterfaceTypeDefnRegularNode> widget Interface

            let interfaceMembers =
                match interfaceWidget with
                | None -> None
                | Some defnRegularNode ->
                    let abstractsSlots = defnRegularNode |> List.map(fun x -> x.Children)
                    let members = abstractsSlots |> List.map(List.ofArray) |> List.concat
                    Some members

            let multipleAttributes =
                match attributes with
                | ValueSome values -> MultipleAttributeListNode.Create values |> Some
                | ValueNone -> None

            let members =
                match members, interfaceMembers with
                | Some members, None -> members
                | Some members, Some interfaceMembers ->
                    let name =
                        interfaceMembers
                        |> List.choose(fun x ->
                            match x with
                            | :? TypeNameNode as node -> Some(node.Accessibility.Value.Text)
                            | _ -> None)
                        |> List.head

                    let interfaceMembers =
                        interfaceMembers
                        |> List.choose(fun x ->
                            match x with
                            | :? MemberDefnAbstractSlotNode as node -> Some(MemberDefn.AbstractSlot(node))
                            | _ -> None)

                    let interfaceMemberDefn =
                        MemberDefn.Interface(
                            MemberDefnInterfaceNode(
                                SingleTextNode("interface", Range.Zero),
                                Type.FromString(name),
                                Some(SingleTextNode("with", Range.Zero)),
                                members,
                                Range.Zero
                            )
                        )

                    interfaceMembers @ [ interfaceMemberDefn ]
                | None, Some interfaceMembers ->
                    let textNode =
                        interfaceMembers
                        |> List.choose(fun x ->
                            match x with
                            | :? TypeNameNode as node -> node.Accessibility
                            | _ -> None)
                        |> List.tryHead

                    let members =
                        interfaceMembers
                        |> List.choose(fun x ->
                            match x with
                            | :? MemberDefnAbstractSlotNode as node -> Some(MemberDefn.AbstractSlot(node))
                            | _ -> None)

                    let name =
                        match textNode with
                        | Some textNode -> textNode.Text
                        | None -> failwith "No accessibility found"

                    let interfaceMemberDefn =
                        MemberDefn.Interface(
                            MemberDefnInterfaceNode(
                                SingleTextNode("interface", Range.Zero),
                                Type.FromString(name),
                                Some(SingleTextNode("with", Range.Zero)),
                                members,
                                Range.Zero
                            )
                        )

                    [ interfaceMemberDefn ]
                | None, None -> []

            let implicitConstructor =
                match parameters with
                | ValueNone -> None
                | ValueSome(parameters) when parameters.IsEmpty ->
                    Some(
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
                    )
                | ValueSome(simplePatNodes) ->
                    let simplePats =
                        match simplePatNodes with
                        | [] -> []
                        | head :: tail ->
                            [ yield Choice1Of2 head
                              for p in tail do
                                  yield Choice2Of2 SingleTextNode.comma
                                  yield Choice1Of2 p ]

                    Some(
                        ImplicitConstructorNode(
                            None,
                            None,
                            None,
                            SingleTextNode.leftParenthesis,
                            simplePats,
                            SingleTextNode.rightParenthesis,
                            None,
                            Range.Zero
                        )
                    )

            ClassTypeDefnRegularNode(name, implicitConstructor, members, multipleAttributes))

[<AutoOpen>]
module ClassBuilders =
    type Ast with

        static member inline Class(name: WidgetBuilder<#SingleTextNode>) =
            CollectionBuilder<ClassTypeDefnRegularNode, MemberDefn>(
                Class.WidgetKey,
                Class.Members,
                AttributesBundle(StackList.empty(), ValueSome [| Class.Name.WithValue(name.Compile()) |], ValueNone)
            )

        static member inline EmptyClass(name: WidgetBuilder<#SingleTextNode>) =
            WidgetBuilder<ClassTypeDefnRegularNode>(
                Class.WidgetKey,
                AttributesBundle(StackList.empty(), ValueSome [| Class.Name.WithValue(name.Compile()) |], ValueNone)
            )

        static member inline Class(node: SingleTextNode) = Ast.Class(Ast.EscapeHatch(node))
        static member inline EmptyClass(node: SingleTextNode) = Ast.EmptyClass(Ast.EscapeHatch(node))
        static member inline Class(name: string) = Ast.Class(SingleTextNode.Create(name))

        static member inline EmptyClass(name: string) =
            Ast.EmptyClass(SingleTextNode.Create(name))

[<Extension>]
type ClassModifiers =
    [<Extension>]
    static member inline isStruct(this: WidgetBuilder<ClassTypeDefnRegularNode>) =
        this.AddScalar(Class.MultipleAttributes.WithValue([ "Struct" ]))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<ClassTypeDefnRegularNode>, attributes: string list) =
        this.AddScalar(Class.MultipleAttributes.WithValue(attributes))

    [<Extension>]
    static member inline parameters(this: WidgetBuilder<ClassTypeDefnRegularNode>, parameters: SimplePatNode list) =
        this.AddScalar(Class.Parameters.WithValue(parameters))

    [<Extension>]
    static member inline implements(this: WidgetBuilder<ClassTypeDefnRegularNode>) =
        AttributeCollectionBuilder(this, Class.Interface)

    [<Extension>]
    static member inline implements
        (
            this: WidgetBuilder<ClassTypeDefnRegularNode>,
            content: WidgetBuilder<InterfaceTypeDefnRegularNode>
        ) =
        AttributeCollectionBuilder(this, Class.Interface) { content }

[<Extension>]
type ClassYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<ClassTypeDefnRegularNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let typeDefn = TypeDefn.Regular(node)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<ClassTypeDefnRegularNode, MemberDefn>,
            x: MemberDefn
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(x).Compile()
        { Widgets = MutStackArray1.One(widget) }
