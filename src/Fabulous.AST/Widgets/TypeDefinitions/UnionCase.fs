namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module UnionCase =

    let Name = Attributes.defineScalar<string> "Name"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let Fields = Attributes.defineScalar<FieldNode list> "Fields"

    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let WidgetKey =
        Widgets.register "UnionCase" (fun widget ->
            let name =
                Widgets.getScalarValue widget Name
                |> PrettyNaming.NormalizeIdentifierBackticks
                |> SingleTextNode.Create

            let fields = Widgets.tryGetScalarValue widget Fields

            let fields =
                match fields with
                | ValueSome fields -> fields
                | ValueNone -> []

            let attributes =
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let lines = Widgets.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            UnionCaseNode(xmlDocs, attributes, None, name, fields, Range.Zero))

[<AutoOpen>]
module UnionCaseBuilders =
    type Ast with
        static member UnionCase(name: string) =
            WidgetBuilder<UnionCaseNode>(UnionCase.WidgetKey, UnionCase.Name.WithValue(name))

        static member UnionCase(name: string, parameters: WidgetBuilder<FieldNode> list) =
            WidgetBuilder<UnionCaseNode>(
                UnionCase.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        UnionCase.Name.WithValue(name),
                        UnionCase.Fields.WithValue(parameters |> List.map Gen.mkOak)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member UnionCase(name: string, parameters: WidgetBuilder<Type> list) =
            let parameters = parameters |> List.map Ast.Field
            Ast.UnionCase(name, parameters)

        static member UnionCase(name: string, parameter: WidgetBuilder<Type>) =
            Ast.UnionCase(name, [ Ast.Field parameter ])

        static member UnionCase(name: string, parameters: string list) =
            Ast.UnionCase(name, parameters |> List.map Ast.Field)

        static member UnionCase(name: string, parameter: WidgetBuilder<FieldNode>) = Ast.UnionCase(name, [ parameter ])

        static member UnionCase(name: string, parameter: string) =
            Ast.UnionCase(name, Ast.Field(parameter))

        static member UnionCase(name: string, parameters: (string * string) list) =
            Ast.UnionCase(name, parameters |> List.map(Ast.Field))

        static member UnionCase(name: string, parameters: (string * WidgetBuilder<Type>) list) =
            Ast.UnionCase(name, parameters |> List.map(Ast.Field))

type UnionCaseModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<UnionCaseNode>, xmlDocs: string list) =
        this.AddScalar(UnionCase.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<UnionCaseNode>, attributes: WidgetBuilder<AttributeNode> list) =
        this.AddScalar(
            UnionCase.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<UnionCaseNode>, attribute: WidgetBuilder<AttributeNode>) =
        UnionCaseModifiers.attributes(this, [ attribute ])

type UnionCaseYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<TypeDefnUnionNode, UnionCaseNode>, x: UnionCaseNode)
        : CollectionContent =
        let widget = Ast.EscapeHatch(x).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<TypeDefnUnionNode, UnionCaseNode>, x: WidgetBuilder<UnionCaseNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        UnionCaseYieldExtensions.Yield(this, node)
