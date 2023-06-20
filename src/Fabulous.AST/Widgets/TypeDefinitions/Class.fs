namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections
open Helpers

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
                SingleTextNode.``type``,
                Some(name),
                IdentListNode([], Range.Zero),
                None,
                [],
                implicitConstructor,
                Some(SingleTextNode.equals),
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

    let WidgetKey =
        Widgets.register "Class" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name
            let parameters = Helpers.tryGetScalarValue widget Parameters
            let members = Helpers.tryGetNodesFromWidgetCollection<MemberDefn> widget Members
            let attributes = Helpers.tryGetScalarValue widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | ValueSome values -> MultipleAttributeListNode.Create values |> Some
                | ValueNone -> None

            let members =
                match members with
                | None -> []
                | Some members -> members

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
    static member inline attributes(this: WidgetBuilder<ClassTypeDefnRegularNode>, attributes: string list) =
        this.AddScalar(Class.MultipleAttributes.WithValue(attributes))

    [<Extension>]
    static member inline isStruct(this: WidgetBuilder<ClassTypeDefnRegularNode>) =
        ClassModifiers.attributes(this, [ "Struct" ])

    [<Extension>]
    static member inline implicitConstructorParameters
        (
            this: WidgetBuilder<ClassTypeDefnRegularNode>,
            parameters: SimplePatNode list
        ) =
        this.AddScalar(Class.Parameters.WithValue(parameters))

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
            x: MethodMemberNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            this: CollectionBuilder<ClassTypeDefnRegularNode, MemberDefn>,
            x: WidgetBuilder<MethodMemberNode>
        ) : CollectionContent =
        let node = Tree.compile x
        ClassYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<ClassTypeDefnRegularNode, MemberDefn>,
            x: PropertyMemberNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            this: CollectionBuilder<ClassTypeDefnRegularNode, MemberDefn>,
            x: WidgetBuilder<PropertyMemberNode>
        ) : CollectionContent =
        let node = Tree.compile x
        ClassYieldExtensions.Yield(this, node)
