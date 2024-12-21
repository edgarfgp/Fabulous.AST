namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module TypeDefnAbbrevNode =

    let Name = Attributes.defineScalar<string> "Name"

    let AliasType = Attributes.defineWidget "AliasType"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let IsMeasure = Attributes.defineScalar<bool> "IsMeasure"

    let XmlDocs = Attributes.defineWidget "XmlDocs"

    let TypeParams = Attributes.defineWidget "TypeParams"

    let WidgetKey =
        Widgets.register "Abbrev" (fun widget ->
            let name = Widgets.getScalarValue widget Name

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget XmlDocs
                |> ValueOption.map(Some)
                |> ValueOption.defaultValue None

            let aliasType = Widgets.getNodeFromWidget widget AliasType

            let typeParams =
                Widgets.tryGetNodeFromWidget widget TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            TypeDefnAbbrevNode(
                TypeNameNode(
                    xmlDocs,
                    attributes,
                    SingleTextNode.``type``,
                    None,
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero),
                    typeParams,
                    [],
                    None,
                    Some(SingleTextNode.equals),
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
                    StackList.one(TypeDefnAbbrevNode.Name.WithValue(name)),
                    [| TypeDefnAbbrevNode.AliasType.WithValue(alias.Compile()) |],
                    Array.empty
                )
            )

        static member Abbrev(name: string, alias: string) = Ast.Abbrev(name, Ast.LongIdent(alias))

type TypeDefnAbbrevNodeModifiers =
    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<TypeDefnAbbrevNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(TypeDefnAbbrevNode.XmlDocs.WithValue(xmlDocs.Compile()))

    [<Extension>]
    static member xmlDocs(this: WidgetBuilder<TypeDefnAbbrevNode>, xmlDocs: string list) =
        TypeDefnAbbrevNodeModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

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
