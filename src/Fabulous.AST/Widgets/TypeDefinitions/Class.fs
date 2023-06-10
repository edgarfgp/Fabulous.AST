namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections
open Helpers

module Class =
    let Name = Attributes.defineWidget "Name"
    let Parameters = Attributes.defineScalar<SimplePatNode list option> "Parameters"
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
                | Some members -> members
                | None -> []

            let implicitConstructor =
                match parameters with
                | ValueNone -> None
                | ValueSome None -> None
                | ValueSome(Some(simplePatNodes)) ->
                    let parameters =
                        simplePatNodes
                        |> List.map Choice1Of2
                        |> List.intersperse(Choice2Of2(SingleTextNode(",", Range.Zero)))

                    Some(
                        ImplicitConstructorNode(
                            None,
                            None,
                            None,
                            SingleTextNode("(", Range.Zero),
                            parameters,
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

        static member inline Class(node: SingleTextNode, parameters: SimplePatNode list option) =
            match parameters with
            | None -> Ast.Class(Ast.EscapeHatch(node), None)
            | Some parameters -> Ast.Class(Ast.EscapeHatch(node), Some parameters)

        static member inline Class(name: string, ?parameters: SimplePatNode list) =
            Ast.Class(SingleTextNode(name, Range.Zero), parameters)

[<Extension>]
type ClassModifiers =
    [<Extension>]
    static member inline isStruct(this: WidgetBuilder<TypeDefnRegularNode>) =
        this.AddScalar(Class.MultipleAttributes.WithValue([ "Struct" ]))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<TypeDefnRegularNode>, attributes) =
        this.AddScalar(Class.MultipleAttributes.WithValue(attributes))

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
