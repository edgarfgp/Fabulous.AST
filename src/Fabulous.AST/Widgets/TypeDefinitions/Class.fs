namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections

module Class =
    let Name = Attributes.defineWidget "Name"
    let Parameters = Attributes.defineScalar "Constructor"
    let Members = Attributes.defineWidgetCollection "Members"

    let WidgetKey =
        Widgets.register "Class" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name
            let parameters = Helpers.tryGetScalarValue widget Parameters
            let members = Helpers.tryGetNodesFromWidgetCollection<MemberDefn> widget Members

            let members =
                match members with
                | Some members -> members
                | None -> []

            let implicitConstructor =
                match parameters with
                | ValueSome parameters ->
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
                | ValueNone -> None

            TypeDefnRegularNode(
                TypeNameNode(
                    None,
                    None,
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

        static member inline Class(name: WidgetBuilder<#SingleTextNode>, parameters: SimplePatNode list) =
            CollectionBuilder<TypeDefnRegularNode, MemberDefn>(
                Class.WidgetKey,
                Class.Members,
                AttributesBundle(
                    StackList.one(Class.Parameters.WithValue(parameters)),
                    ValueSome [| Class.Name.WithValue(name.Compile()) |],
                    ValueNone
                )
            )

        static member inline Class(node: SingleTextNode, parameters: SimplePatNode list) =
            Ast.Class(Ast.EscapeHatch(node), parameters)

        static member inline Class(name: string, parameters: SimplePatNode list) =
            Ast.Class(SingleTextNode(name, Range.Zero), parameters)

// [<Extension>]
// type ClassModifiers =
//     [<Extension>]
//     static member inline members(this: WidgetBuilder<TypeDefnRecordNode>) =
//         AttributeCollectionBuilder<TypeDefnRecordNode, MemberDefn>(this, Record.Members)

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
