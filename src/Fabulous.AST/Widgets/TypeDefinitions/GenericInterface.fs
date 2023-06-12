namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections

type GeneticInterfaceTypeDefnRegularNode
    (
        name: SingleTextNode,
        typeParams: TyparDecls option,
        multipleAttributes: MultipleAttributeListNode option,
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
                None,
                Some(SingleTextNode.equals),
                None,
                Range.Zero
            ),
            members,
            Range.Zero
        )

module GenericInterface =
    let Name = Attributes.defineWidget "Name"
    let Parameters = Attributes.defineScalar<SimplePatNode list option> "Parameters"
    let Members = Attributes.defineWidgetCollection "Members"
    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"
    let TypeParams = Attributes.defineScalar<string list> "TypeParams"

    let WidgetKey =
        Widgets.register "GenericInterface" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name

            let members =
                Helpers.tryGetNodesFromWidgetCollection<MemberDefnAbstractSlotNode> widget Members

            let attributes = Helpers.tryGetScalarValue widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | ValueNone -> None
                | ValueSome values -> Some(MultipleAttributeListNode.Create(values))

            let members =
                match members with
                | Some members ->
                    [ for item in members do
                          MemberDefn.AbstractSlot(item) ]
                | None -> []

            let typeParams = Helpers.tryGetScalarValue widget TypeParams

            let typeParams =
                match typeParams with
                | ValueSome values ->
                    TyparDeclsPostfixListNode(
                        SingleTextNode.lessThan,
                        [ for v in values do
                              TyparDeclNode(None, SingleTextNode.Create(v), Range.Zero) ],
                        [],
                        SingleTextNode.greaterThan,
                        Range.Zero
                    )
                    |> TyparDecls.PostfixList
                    |> Some
                | ValueNone -> None

            GeneticInterfaceTypeDefnRegularNode(name, typeParams, multipleAttributes, members))

[<AutoOpen>]
module GenericInterfaceBuilders =
    type Ast with

        static member inline GenericInterface(name: WidgetBuilder<#SingleTextNode>, typeParams: string list) =
            CollectionBuilder<GeneticInterfaceTypeDefnRegularNode, MemberDefnAbstractSlotNode>(
                GenericInterface.WidgetKey,
                GenericInterface.Members,
                AttributesBundle(
                    StackList.one(GenericInterface.TypeParams.WithValue(typeParams)),
                    ValueSome [| GenericInterface.Name.WithValue(name.Compile()) |],
                    ValueNone
                )
            )

        static member inline GenericInterface(node: SingleTextNode, typeParams: string list) =
            Ast.GenericInterface(Ast.EscapeHatch(node), typeParams)

        static member inline GenericInterface(name: string, typeParams: string list) =
            Ast.GenericInterface(SingleTextNode(name, Range.Zero), typeParams)

[<Extension>]
type GenericInterfaceModifiers =
    [<Extension>]
    static member inline attributes(this: WidgetBuilder<GeneticInterfaceTypeDefnRegularNode>, attributes: string list) =
        this.AddScalar(GenericInterface.MultipleAttributes.WithValue(attributes))

    [<Extension>]
    static member inline isStruct(this: WidgetBuilder<GeneticInterfaceTypeDefnRegularNode>) =
        GenericInterfaceModifiers.attributes(this, [ "Struct" ])

[<Extension>]
type GenericInterfaceBuildersYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<GeneticInterfaceTypeDefnRegularNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let typeDefn = TypeDefn.Regular(node)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<GeneticInterfaceTypeDefnRegularNode, MemberDefnAbstractSlotNode>,
            x: MemberDefnAbstractSlotNode
        ) : CollectionContent =
        let x = MemberDefn.AbstractSlot(x)
        let widget = Ast.EscapeHatch(x).Compile()
        { Widgets = MutStackArray1.One(widget) }
