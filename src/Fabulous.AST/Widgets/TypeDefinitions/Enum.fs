namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module Enum =

    let EnumCaseNode = Attributes.defineWidgetCollection "UnionCaseNode"

    let Name = Attributes.defineScalar<string> "Name"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let WidgetKey =
        Widgets.register "Enum" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

            let enumCaseNodes =
                Widgets.getNodesFromWidgetCollection<EnumCaseNode> widget EnumCaseNode

            let lines = Widgets.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            TypeDefnEnumNode(
                TypeNameNode(
                    xmlDocs,
                    attributes,
                    SingleTextNode.``type``,
                    Some(SingleTextNode.Create(name)),
                    IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.equals) ], Range.Zero),
                    None,
                    [],
                    None,
                    None,
                    None,
                    Range.Zero
                ),
                enumCaseNodes,
                [],
                Range.Zero
            ))

[<AutoOpen>]
module EnumBuilders =
    type Ast with

        static member Enum(name: string) =
            CollectionBuilder<TypeDefnEnumNode, EnumCaseNode>(
                Enum.WidgetKey,
                Enum.EnumCaseNode,
                Enum.Name.WithValue(name)
            )

type EnumModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<TypeDefnEnumNode>, xmlDocs: string list) =
        this.AddScalar(Enum.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<TypeDefnEnumNode>, attributes: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            Enum.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<TypeDefnEnumNode>, attribute: WidgetBuilder<AttributeNode>) =
        EnumModifiers.attributes(this, [ attribute ])

type EnumYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: TypeDefnEnumNode) : CollectionContent =
        let typeDefn = TypeDefn.Enum(x)
        let typeDefn = ModuleDecl.TypeDefn(typeDefn)
        let widget = Ast.EscapeHatch(typeDefn).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<TypeDefnEnumNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        EnumYieldExtensions.Yield(this, node)
