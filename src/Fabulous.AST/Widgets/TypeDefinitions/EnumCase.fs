namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module EnumCase =

    let Name = Attributes.defineScalar<string> "Name"

    let Value = Attributes.defineWidget "Value"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let XmlDocs = Attributes.defineWidget "XmlDocs"

    let WidgetKey =
        Widgets.register "EnumCase" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name |> PrettyNaming.NormalizeIdentifierBackticks

            let value = Widgets.getNodeFromWidget widget Value

            let xmlDocs =
                Widgets.tryGetNodeFromWidget widget XmlDocs
                |> ValueOption.map(fun x -> Some(x))
                |> ValueOption.defaultValue None

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            EnumCaseNode(
                xmlDocs,
                None,
                attributes,
                SingleTextNode.Create(name),
                SingleTextNode.equals,
                value,
                Range.Zero
            ))

[<AutoOpen>]
module EnumCaseBuilders =
    type Ast with

        static member EnumCase(name: string, value: WidgetBuilder<Expr>) =
            WidgetBuilder<EnumCaseNode>(
                EnumCase.WidgetKey,
                AttributesBundle(
                    StackList.one(EnumCase.Name.WithValue(name)),
                    [| EnumCase.Value.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member EnumCase(name: string, value: WidgetBuilder<Constant>) =
            Ast.EnumCase(name, Ast.ConstantExpr(value))

        static member EnumCase(name: string, value: string) = Ast.EnumCase(name, Ast.Constant(value))

type EnumCaseModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<EnumCaseNode>, xmlDocs: WidgetBuilder<XmlDocNode>) =
        this.AddWidget(EnumCase.XmlDocs.WithValue(xmlDocs.Compile()))

    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<EnumCaseNode>, xmlDocs: string list) =
        EnumCaseModifiers.xmlDocs(this, Ast.XmlDocs(xmlDocs))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<EnumCaseNode>, attributes: WidgetBuilder<AttributeNode> list) =
        this.AddScalar(
            EnumCase.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<EnumCaseNode>, attribute: WidgetBuilder<AttributeNode>) =
        EnumCaseModifiers.attributes(this, [ attribute ])

type EnumCaseNodeYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<TypeDefnEnumNode, EnumCaseNode>, x: EnumCaseNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(x).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<TypeDefnEnumNode, EnumCaseNode>, x: WidgetBuilder<EnumCaseNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        EnumCaseNodeYieldExtensions.Yield(this, node)
