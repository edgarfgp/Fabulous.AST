namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections

module ClassEnd =
    let Name = Attributes.defineScalar<string> "Name"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let TypeParams = Attributes.defineScalar<string list> "TypeParams"
    let Constructor = Attributes.defineWidget "Constructor"

    let WidgetKey =
        Widgets.register "ClassEnd" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name
                |> Unquoted
                |> StringParsing.normalizeIdentifierBackticks

            let attributes = Widgets.tryGetScalarValue widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | ValueSome values ->
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
                | ValueNone -> None

            let typeParams = Widgets.tryGetScalarValue widget TypeParams

            let implicitConstructor =
                Widgets.tryGetNodeFromWidget<ImplicitConstructorNode> widget Constructor

            let implicitConstructor =
                match implicitConstructor with
                | ValueNone -> None
                | ValueSome implicitConstructor -> Some implicitConstructor

            let typeParams =
                match typeParams with
                | ValueSome values when values.IsEmpty -> None
                | ValueNone -> None
                | ValueSome values ->
                    TyparDeclsPostfixListNode(
                        SingleTextNode.lessThan,
                        [ for v in values do
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

        static member ClassEnd(name: string) =
            WidgetBuilder<TypeDefnExplicitNode>(
                ClassEnd.WidgetKey,
                AttributesBundle(StackList.one(ClassEnd.Name.WithValue(name)), Array.empty, Array.empty)
            )

        static member ClassEnd(name: string, constructor: WidgetBuilder<ImplicitConstructorNode>) =
            WidgetBuilder<TypeDefnExplicitNode>(
                ClassEnd.WidgetKey,
                AttributesBundle(
                    StackList.one(ClassEnd.Name.WithValue(name)),
                    [| ClassEnd.Constructor.WithValue(constructor.Compile()) |],
                    Array.empty
                )
            )

[<Extension>]
type ClassEndModifiers =
    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<TypeDefnExplicitNode>, values: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            ClassEnd.MultipleAttributes.WithValue(
                [ for vals in values do
                      Gen.mkOak vals ]
            )
        )

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<TypeDefnExplicitNode>, attributes: string list) =
        ClassEndModifiers.attributes(
            this,
            [ for attribute in attributes do
                  Ast.Attribute(attribute) ]
        )

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefnExplicitNode>, attribute: WidgetBuilder<AttributeNode>) =
        ClassEndModifiers.attributes(this, [ attribute ])

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefnExplicitNode>, attribute: string) =
        ClassEndModifiers.attributes(this, [ Ast.Attribute(attribute) ])

    [<Extension>]
    static member inline typeParams(this: WidgetBuilder<TypeDefnExplicitNode>, typeParams: string list) =
        this.AddScalar(ClassEnd.TypeParams.WithValue(typeParams))

[<Extension>]
type ClassEndYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefnExplicitNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let typeDefn = TypeDefn.Explicit(node)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<TypeDefnExplicitNode, MemberDefn>, x: MemberDefn)
        : CollectionContent =
        let widget = Ast.EscapeHatch(x).Compile()
        { Widgets = MutStackArray1.One(widget) }
