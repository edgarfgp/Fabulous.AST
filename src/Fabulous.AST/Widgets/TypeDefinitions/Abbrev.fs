namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

[<RequireQualifiedAccess>]
type TypeDefnAbbrev =
    | Abbrev
    | Measure

module TypeDefnAbbrevNode =

    let Name = Attributes.defineScalar<string> "Name"

    let AliasType = Attributes.defineWidget "AliasType"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let TypeDefnAbbrev = Attributes.defineScalar<TypeDefnAbbrev> "TypeDefnAbbrev"

    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let TypeParams = Attributes.defineWidget "TypeParams"

    let WidgetKey =
        Widgets.register "Abbrev" (fun widget ->
            let name = Widgets.getScalarValue widget Name

            let lines = Widgets.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            let aliasType = Widgets.getNodeFromWidget widget AliasType

            let typeDefnAbbrev = Widgets.getScalarValue widget TypeDefnAbbrev

            let attributeNode =
                AttributeNode(
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.measure) ], Range.Zero),
                    None,
                    None,
                    Range.Zero
                )

            let attributes =
                match Widgets.tryGetScalarValue widget MultipleAttributes, typeDefnAbbrev with
                | ValueNone, TypeDefnAbbrev.Abbrev -> None
                | ValueNone, TypeDefnAbbrev.Measure -> Some(MultipleAttributeListNode.Create([ attributeNode ]))
                | ValueSome attributeNodes, TypeDefnAbbrev.Abbrev ->
                    Some(MultipleAttributeListNode.Create(attributeNodes))
                | ValueSome attributeNodes, TypeDefnAbbrev.Measure ->
                    Some(MultipleAttributeListNode.Create([ attributeNode ] @ attributeNodes))

            let typeParams =
                Widgets.tryGetNodeFromWidget widget TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let equals =
                match typeDefnAbbrev with
                | TypeDefnAbbrev.Abbrev -> Some(SingleTextNode.equals)
                | TypeDefnAbbrev.Measure -> None

            let identListNode, leadingNode =
                match typeDefnAbbrev with
                | TypeDefnAbbrev.Abbrev ->
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero), None
                | TypeDefnAbbrev.Measure ->
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.equals) ], Range.Zero),
                    Some(SingleTextNode.Create(name))

            TypeDefnAbbrevNode(
                TypeNameNode(
                    xmlDocs,
                    attributes,
                    SingleTextNode.``type``,
                    leadingNode,
                    identListNode,
                    typeParams,
                    [],
                    None,
                    equals,
                    None,
                    Range.Zero
                ),
                aliasType,
                [],
                Range.Zero
            ))

[<AutoOpen>]
module TypeDefnAbbrevNodeBuilders =
    type Ast with

        static member Abbrev(name: string, alias: WidgetBuilder<Type>) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name

            WidgetBuilder<TypeDefnAbbrevNode>(
                TypeDefnAbbrevNode.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        TypeDefnAbbrevNode.Name.WithValue(name),
                        TypeDefnAbbrevNode.TypeDefnAbbrev.WithValue(TypeDefnAbbrev.Abbrev)
                    ),
                    [| TypeDefnAbbrevNode.AliasType.WithValue(alias.Compile()) |],
                    Array.empty
                )
            )

        static member Abbrev(name: string, alias: string) = Ast.Abbrev(name, Ast.LongIdent(alias))

        static member Measure(name: string, powerType: WidgetBuilder<Type>) =
            let name = PrettyNaming.NormalizeIdentifierBackticks name

            WidgetBuilder<TypeDefnAbbrevNode>(
                TypeDefnAbbrevNode.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        TypeDefnAbbrevNode.Name.WithValue(name),
                        TypeDefnAbbrevNode.TypeDefnAbbrev.WithValue(TypeDefnAbbrev.Measure)
                    ),
                    [| TypeDefnAbbrevNode.AliasType.WithValue(powerType.Compile()) |],
                    Array.empty
                )
            )

        static member Measure(name: string, powerType: string) =
            Ast.Measure(name, Ast.LongIdent(powerType))

type TypeDefnAbbrevNodeModifiers =
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<TypeDefnAbbrevNode>, comments: string list) =
        this.AddScalar(TypeDefnAbbrevNode.XmlDocs.WithValue(comments))

    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<TypeDefnAbbrevNode>, attributes: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            TypeDefnAbbrevNode.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefnAbbrevNode>, attribute: WidgetBuilder<AttributeNode>) =
        TypeDefnAbbrevNodeModifiers.attributes(this, [ attribute ])

    [<Extension>]
    static member inline typeParams(this: WidgetBuilder<TypeDefnAbbrevNode>, typeParams: WidgetBuilder<TyparDecls>) =
        this.AddWidget(TypeDefnAbbrevNode.TypeParams.WithValue(typeParams.Compile()))

type AbbrevYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: TypeDefnAbbrevNode) : CollectionContent =
        let typeDefn = TypeDefn.Abbrev(x)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefnAbbrevNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        AbbrevYieldExtensions.Yield(this, node)
