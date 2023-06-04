namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections

module Class =
    let Name = Attributes.defineWidget "Name"
    let Parameters = Attributes.defineScalar<SimplePatNode list option> "Parameters"
    let Members = Attributes.defineWidgetCollection "Members"

    let Interface = Attributes.defineWidgetCollection "Interface"

    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"

    let WidgetKey =
        Widgets.register "Class" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name
            let parameters = Helpers.tryGetScalarValue widget Parameters
            let members = Helpers.tryGetNodesFromWidgetCollection<MemberDefn> widget Members
            let attributes = Helpers.tryGetScalarValue widget MultipleAttributes

            let interfaceWidget =
                Helpers.tryGetNodesFromWidgetCollection<TypeDefnRegularNode> widget Interface

            let interfaceMembers =
                match interfaceWidget with
                | None -> None
                | Some defnRegularNode ->
                    let abstractsSlots = defnRegularNode |> List.map(fun x -> x.Children)
                    let members = abstractsSlots |> List.map(List.ofArray) |> List.concat
                    Some members

            let multipleAttributes =
                match attributes with
                | ValueSome values -> TypeHelpers.createAttributes values |> Some
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

                    let members =
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

                    members @ [ interfaceMemberDefn ]
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
                | ValueSome None -> None
                | ValueSome(Some(parameters)) when parameters.IsEmpty ->
                    Some(
                        ImplicitConstructorNode(
                            None,
                            None,
                            None,
                            SingleTextNode("(", Range.Zero),
                            [],
                            SingleTextNode(")", Range.Zero),
                            None,
                            Range.Zero
                        )
                    )
                | ValueSome(Some(simplePatNodes)) ->
                    Some(
                        ImplicitConstructorNode(
                            None,
                            None,
                            None,
                            SingleTextNode("(", Range.Zero),
                            simplePatNodes,
                            SingleTextNode(")", Range.Zero),
                            None,
                            Range.Zero
                        )
                    )

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
            ))

[<AutoOpen>]
module ClassBuilders =
    type Ast with

        static member inline Class(name: WidgetBuilder<#SingleTextNode>, parameters: SimplePatNode list option) =
            CollectionBuilder<TypeDefnRegularNode, MemberDefn>(
                Class.WidgetKey,
                Class.Members,
                AttributesBundle(
                    StackList.one(Class.Parameters.WithValue(parameters)),
                    ValueSome [| Class.Name.WithValue(name.Compile()) |],
                    ValueNone
                )
            )

        static member inline Class'(name: WidgetBuilder<#SingleTextNode>, parameters: SimplePatNode list option) =
            WidgetBuilder<TypeDefnRegularNode>(
                Class.WidgetKey,
                AttributesBundle(
                    StackList.one(Class.Parameters.WithValue(parameters)),
                    ValueSome [| Class.Name.WithValue(name.Compile()) |],
                    ValueNone
                )
            )

        static member inline Class(node: SingleTextNode, parameters: SimplePatNode list option) =
            match parameters with
            | None -> Ast.Class(Ast.EscapeHatch(node), None)
            | Some parameters -> Ast.Class(Ast.EscapeHatch(node), Some parameters)

        static member inline Class'(node: SingleTextNode, parameters: SimplePatNode list option) =
            match parameters with
            | None -> Ast.Class'(Ast.EscapeHatch(node), None)
            | Some parameters -> Ast.Class'(Ast.EscapeHatch(node), Some parameters)

        static member inline Class(name: string, ?parameters: SimplePatNode list) =
            Ast.Class(SingleTextNode(name, Range.Zero), parameters)

        static member inline Class'(name: string, ?parameters: SimplePatNode list) =
            Ast.Class'(SingleTextNode(name, Range.Zero), parameters)

[<Extension>]
type ClassModifiers =
    [<Extension>]
    static member inline isStruct(this: WidgetBuilder<TypeDefnRegularNode>) =
        this.AddScalar(Class.MultipleAttributes.WithValue([ "Struct" ]))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<TypeDefnRegularNode>, attributes) =
        this.AddScalar(Class.MultipleAttributes.WithValue(attributes))

    [<Extension>]
    static member inline implements(this: WidgetBuilder<TypeDefnRegularNode>) =
        AttributeCollectionBuilder(this, Class.Interface)

    [<Extension>]
    static member inline implements
        (
            this: WidgetBuilder<TypeDefnRegularNode>,
            content: WidgetBuilder<TypeDefnRegularNode>
        ) =
        AttributeCollectionBuilder(this, Class.Interface) { content }

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
            x: MemberDefn
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(x).Compile()
        { Widgets = MutStackArray1.One(widget) }
