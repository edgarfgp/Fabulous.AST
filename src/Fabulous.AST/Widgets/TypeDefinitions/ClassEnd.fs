namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections

module ClassEnd =
    let Name = Attributes.defineWidget "Name"
    let Parameters = Attributes.defineScalar<SimplePatNode list> "Parameters"
    let Members = Attributes.defineWidgetCollection "Members"
    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"
    let TypeParams = Attributes.defineScalar<string list> "TypeParams"

    let WidgetKey =
        Widgets.register "ClassEnd" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name
            let parameters = Helpers.tryGetScalarValue widget Parameters
            let attributes = Helpers.tryGetScalarValue widget MultipleAttributes
            let typeParams = Helpers.tryGetScalarValue widget TypeParams

            let multipleAttributes =
                match attributes with
                | ValueSome values -> MultipleAttributeListNode.Create values |> Some
                | ValueNone -> None

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

            let typeParams =
                match typeParams with
                | ValueSome values when values.IsEmpty -> None
                | ValueNone -> None
                | ValueSome values ->
                    TyparDeclsPostfixListNode(
                        SingleTextNode.lessThan,
                        [ for v in values do
                              // FIXME - Update
                              TyparDeclNode(None, SingleTextNode.Create(v), [], Range.Zero) ],
                        [],
                        SingleTextNode.greaterThan,
                        Range.Zero
                    )
                    |> TyparDecls.PostfixList
                    |> Some

            TypeDefnExplicitNode(
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
                TypeDefnExplicitBodyNode(SingleTextNode.``class``, [], SingleTextNode.``end``, Range.Zero),
                [],
                Range.Zero
            ))

[<AutoOpen>]
module ClassEndBuilders =
    type Ast with

        static member inline ClassEnd(name: WidgetBuilder<#SingleTextNode>, parameters: string list) =
            WidgetBuilder<TypeDefnExplicitNode>(
                ClassEnd.WidgetKey,
                AttributesBundle(
                    StackList.one(ClassEnd.TypeParams.WithValue(parameters)),
                    ValueSome [| ClassEnd.Name.WithValue(name.Compile()) |],
                    ValueNone
                )
            )

        static member inline ClassEnd(node: SingleTextNode, parameters: string list option) =
            match parameters with
            | None -> Ast.ClassEnd(Ast.EscapeHatch(node), [])
            | Some(parameters) -> Ast.ClassEnd(Ast.EscapeHatch(node), parameters)

        static member inline ClassEnd(name: string, ?parameters: string list) =
            Ast.ClassEnd(SingleTextNode.Create(name), parameters)

[<Extension>]
type ClassEndModifiers =
    [<Extension>]
    static member inline attributes(this: WidgetBuilder<TypeDefnExplicitNode>, attributes: string list) =
        this.AddScalar(ClassEnd.MultipleAttributes.WithValue(attributes))

    [<Extension>]
    static member inline isStruct(this: WidgetBuilder<TypeDefnExplicitNode>) =
        ClassEndModifiers.attributes(this, [ "Struct" ])

    [<Extension>]
    static member inline implicitConstructorParameters
        (
            this: WidgetBuilder<TypeDefnExplicitNode>,
            parameters: SimplePatNode list
        ) =
        this.AddScalar(ClassEnd.Parameters.WithValue(parameters))

[<Extension>]
type ClassEndYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<TypeDefnExplicitNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let typeDefn = TypeDefn.Explicit(node)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<TypeDefnExplicitNode, MemberDefn>,
            x: MemberDefn
        ) : CollectionContent =
        let widget = Ast.EscapeHatch(x).Compile()
        { Widgets = MutStackArray1.One(widget) }
