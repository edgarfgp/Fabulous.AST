namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections

module ClassEnd =
    let Name = Attributes.defineScalar<string> "Name"
    let Members = Attributes.defineWidgetCollection "Members"
    let MultipleAttributes = Attributes.defineWidget "MultipleAttributes"
    let TypeParams = Attributes.defineScalar<string list> "TypeParams"
    let SimplePats = Attributes.defineWidget "SimplePats"
    let HasConstructor = Attributes.defineScalar<bool> "HasConstructor"

    let WidgetKey =
        Widgets.register "ClassEnd" (fun widget ->
            let name = Helpers.getScalarValue widget Name

            let attributes =
                Helpers.tryGetNodeFromWidget<AttributeListNode> widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | ValueSome values -> Some(MultipleAttributeListNode([ values ], Range.Zero))
                | ValueNone -> None

            let typeParams = Helpers.tryGetScalarValue widget TypeParams

            let implicitConstructor =
                Helpers.tryGetNodeFromWidget<ImplicitConstructorNode> widget SimplePats

            let hasConstructor = Helpers.getScalarValue widget HasConstructor

            let implicitConstructor =
                match implicitConstructor with
                | ValueNone when not hasConstructor -> None
                | ValueNone when hasConstructor ->
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
                    Some(SingleTextNode.Create(name)),
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

        static member private BaseClassEnd
            (
                name: string,
                parameters: string list voption,
                constructor: WidgetBuilder<ImplicitConstructorNode> voption,
                hasConstructor: bool
            ) =
            let scalars =
                match parameters with
                | ValueNone ->
                    StackList.two(ClassEnd.Name.WithValue(name), ClassEnd.HasConstructor.WithValue(hasConstructor))
                | ValueSome typeParams ->
                    StackList.three(
                        ClassEnd.Name.WithValue(name),
                        ClassEnd.TypeParams.WithValue(typeParams),
                        ClassEnd.HasConstructor.WithValue(hasConstructor)
                    )

            WidgetBuilder<TypeDefnExplicitNode>(
                ClassEnd.WidgetKey,
                AttributesBundle(
                    scalars,
                    ValueSome
                        [| match constructor with
                           | ValueSome constructor -> ClassEnd.SimplePats.WithValue(constructor.Compile())
                           | ValueNone -> () |],
                    ValueNone
                )
            )

        static member ClassEnd(node: string) =
            Ast.BaseClassEnd(node, ValueNone, ValueNone, false)

        static member ClassEnd(node: string, hasConstructor: bool) =
            Ast.BaseClassEnd(node, ValueNone, ValueNone, hasConstructor)

        static member ClassEnd(node: string, constructor: WidgetBuilder<ImplicitConstructorNode>) =
            Ast.BaseClassEnd(node, ValueNone, ValueSome constructor, false)

        static member ClassEnd(name: string, parameters: string list) =
            Ast.BaseClassEnd(name, ValueSome parameters, ValueNone, false)

        static member ClassEnd(name: string, parameters: string list, hasConstructor: bool) =
            Ast.BaseClassEnd(name, ValueSome parameters, ValueNone, hasConstructor)

        static member ClassEnd
            (
                name: string,
                parameters: string list,
                constructor: WidgetBuilder<ImplicitConstructorNode>
            ) =
            Ast.BaseClassEnd(name, ValueSome parameters, ValueSome constructor, false)

[<Extension>]
type ClassEndModifiers =
    [<Extension>]
    static member inline attributes
        (
            this: WidgetBuilder<TypeDefnExplicitNode>,
            attributes: WidgetBuilder<AttributeListNode>
        ) =
        this.AddWidget(ClassEnd.MultipleAttributes.WithValue(attributes.Compile()))

    [<Extension>]
    static member inline attributes
        (
            this: WidgetBuilder<TypeDefnExplicitNode>,
            attribute: WidgetBuilder<AttributeNode>
        ) =
        this.AddWidget(ClassEnd.MultipleAttributes.WithValue((Ast.AttributeNodes() { attribute }).Compile()))

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
