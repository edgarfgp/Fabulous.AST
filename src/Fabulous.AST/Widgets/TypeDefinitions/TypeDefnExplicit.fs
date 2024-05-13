namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Microsoft.FSharp.Collections

type TypeDefn =
    | Class
    | Interface
    | Struct

module TypeDefnExplicit =
    let Name = Attributes.defineScalar<string> "Name"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let TypeParams = Attributes.defineScalar<string list> "TypeParams"
    let Constructor = Attributes.defineWidget "Constructor"
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let TypeDefn = Attributes.defineScalar<TypeDefn> "TypeDefn"

    let WidgetKey =
        Widgets.register "ClassEnd" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

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

            let lines = Widgets.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
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

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let typeDefn =
                match Widgets.getScalarValue widget TypeDefn with
                | Class -> SingleTextNode.``class``
                | Interface -> SingleTextNode.``interface``
                | Struct -> SingleTextNode.``struct``

            TypeDefnExplicitNode(
                TypeNameNode(
                    xmlDocs,
                    multipleAttributes,
                    SingleTextNode.``type``,
                    accessControl,
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero),
                    typeParams,
                    [],
                    implicitConstructor,
                    Some(SingleTextNode.equals),
                    None,
                    Range.Zero
                ),
                TypeDefnExplicitBodyNode(typeDefn, [], SingleTextNode.``end``, Range.Zero),
                [],
                Range.Zero
            ))

[<AutoOpen>]
module ClassEndBuilders =
    type Ast with

        static member ClassEnd(name: string) =
            WidgetBuilder<TypeDefnExplicitNode>(
                TypeDefnExplicit.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        TypeDefnExplicit.Name.WithValue(name),
                        TypeDefnExplicit.TypeDefn.WithValue(TypeDefn.Class)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member ClassEnd(name: string, constructor: WidgetBuilder<ImplicitConstructorNode>) =
            WidgetBuilder<TypeDefnExplicitNode>(
                TypeDefnExplicit.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        TypeDefnExplicit.Name.WithValue(name),
                        TypeDefnExplicit.TypeDefn.WithValue(TypeDefn.Class)
                    ),
                    [| TypeDefnExplicit.Constructor.WithValue(constructor.Compile()) |],
                    Array.empty
                )
            )

        static member StructEnd(name: string, constructor: WidgetBuilder<ImplicitConstructorNode>) =
            WidgetBuilder<TypeDefnExplicitNode>(
                TypeDefnExplicit.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        TypeDefnExplicit.Name.WithValue(name),
                        TypeDefnExplicit.TypeDefn.WithValue(TypeDefn.Struct)
                    ),
                    [| TypeDefnExplicit.Constructor.WithValue(constructor.Compile()) |],
                    Array.empty
                )
            )

        static member InterfaceEnd(name: string) =
            WidgetBuilder<TypeDefnExplicitNode>(
                TypeDefnExplicit.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        TypeDefnExplicit.Name.WithValue(name),
                        TypeDefnExplicit.TypeDefn.WithValue(TypeDefn.Interface)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

type ClassEndModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnExplicitNode>, xmlDocs: string list) =
        this.AddScalar(TypeDefnExplicit.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<TypeDefnExplicitNode>, values: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            TypeDefnExplicit.MultipleAttributes.WithValue(
                [ for vals in values do
                      Gen.mkOak vals ]
            )
        )

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefnExplicitNode>, attribute: WidgetBuilder<AttributeNode>) =
        ClassEndModifiers.attributes(this, [ attribute ])

    [<Extension>]
    static member inline typeParams(this: WidgetBuilder<TypeDefnExplicitNode>, typeParams: string list) =
        this.AddScalar(TypeDefnExplicit.TypeParams.WithValue(typeParams))

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<TypeDefnExplicitNode>) =
        this.AddScalar(TypeDefnExplicit.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<TypeDefnExplicitNode>) =
        this.AddScalar(TypeDefnExplicit.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<TypeDefnExplicitNode>) =
        this.AddScalar(TypeDefnExplicit.Accessibility.WithValue(AccessControl.Internal))

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
