namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections

type GenericClassTypeDefnRegularNode
    (
        name: SingleTextNode,
        multipleAttributes: MultipleAttributeListNode option,
        implicitConstructor: ImplicitConstructorNode option,
        typeParams: TyparDecls option,
        members: MemberDefn list
    ) =
    inherit
        TypeDefnRegularNode(
            TypeNameNode(
                None,
                multipleAttributes,
                SingleTextNode.``type``,
                Some(name),
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
        )

module GenericClass =
    let Name = Attributes.defineWidget "Name"
    let Parameters = Attributes.defineScalar<SimplePatNode list> "Parameters"
    let Members = Attributes.defineWidgetCollection "Members"
    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"
    let TypeParams = Attributes.defineScalar<string list> "TypeParams"

    let WidgetKey =
        Widgets.register "GenericClass" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name
            let parameters = Helpers.tryGetScalarValue widget Parameters
            let members = Helpers.tryGetNodesFromWidgetCollection<MemberDefn> widget Members
            let attributes = Helpers.tryGetScalarValue widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | ValueSome values -> MultipleAttributeListNode.Create values |> Some
                | ValueNone -> None

            let typeParams = Helpers.tryGetScalarValue widget TypeParams

            let typeParams =
                match typeParams with
                | ValueSome values when values.IsEmpty -> None
                | ValueNone -> None
                | ValueSome values ->
                    TyparDeclsPostfixListNode(
                        SingleTextNode.lessThan,
                        [ for v in values do
                              // FIXME - Update
                              TyparDeclNode(None, SingleTextNode.Create v, [], Range.Zero) ],
                        [],
                        SingleTextNode.greaterThan,
                        Range.Zero
                    )
                    |> TyparDecls.PostfixList
                    |> Some

            let members =
                match members with
                | Some members -> members
                | None -> []

            let implicitConstructor =
                match parameters with
                | ValueNone -> None
                | ValueSome parameters when parameters.IsEmpty ->
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
                | ValueSome simplePatNodes ->
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

            GenericClassTypeDefnRegularNode(name, multipleAttributes, implicitConstructor, typeParams, members))

[<AutoOpen>]
module GenericClassBuilders =
    type Ast with

        static member inline GenericClass(name: WidgetBuilder<#SingleTextNode>, typeParams: string list) =
            CollectionBuilder<GenericClassTypeDefnRegularNode, MemberDefn>(
                GenericClass.WidgetKey,
                GenericClass.Members,
                AttributesBundle(
                    StackList.one(GenericClass.TypeParams.WithValue(typeParams)),
                    ValueSome [| GenericClass.Name.WithValue(name.Compile()) |],
                    ValueNone
                )
            )

        static member inline GenericClass(node: SingleTextNode, typeParams: string list) =
            Ast.GenericClass(Ast.EscapeHatch(node), typeParams)

        static member inline GenericClass(name: string, typeParams: string list) =
            Ast.GenericClass(SingleTextNode(name, Range.Zero), typeParams)

[<Extension>]
type GenericClassModifiers =

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<GenericClassTypeDefnRegularNode>, attributes: string list) =
        this.AddScalar(GenericClass.MultipleAttributes.WithValue(attributes))

    [<Extension>]
    static member isStruct(this: WidgetBuilder<GenericClassTypeDefnRegularNode>) =
        GenericClassModifiers.attributes(this, [ "Struct" ])

    [<Extension>]
    static member inline implicitConstructorParameters
        (
            this: WidgetBuilder<GenericClassTypeDefnRegularNode>,
            parameters: SimplePatNode list
        ) =
        this.AddScalar(GenericClass.Parameters.WithValue(parameters))

[<Extension>]
type GenericClassYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<GenericClassTypeDefnRegularNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let typeDefn = TypeDefn.Regular(node)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<GenericClassTypeDefnRegularNode, MemberDefn>,
            x: MemberDefn
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(x).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<GenericClassTypeDefnRegularNode, MemberDefn>,
            x: MethodMemberNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            this: CollectionBuilder<GenericClassTypeDefnRegularNode, MemberDefn>,
            x: WidgetBuilder<MethodMemberNode>
        ) : CollectionContent =
        let node = Tree.compile x
        GenericClassYieldExtensions.Yield(this, node)

    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<GenericClassTypeDefnRegularNode, MemberDefn>,
            x: PropertyMemberNode
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(MemberDefn.Member(x)).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            this: CollectionBuilder<GenericClassTypeDefnRegularNode, MemberDefn>,
            x: WidgetBuilder<PropertyMemberNode>
        ) : CollectionContent =
        let node = Tree.compile x
        GenericClassYieldExtensions.Yield(this, node)
