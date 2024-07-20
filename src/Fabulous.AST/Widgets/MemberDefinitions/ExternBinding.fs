namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ExternBinding =
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let AttributesOfType = Attributes.defineWidget "MultipleAttributes"
    let TypeVal = Attributes.defineWidget "Type"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"
    let Identifier = Attributes.defineScalar<string> "Identifier"
    let Parameters = Attributes.defineScalar<ExternBindingPatternNode list> "Parameters"

    let WidgetKey =
        Widgets.register "ModuleDeclAttributes" (fun widget ->
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

            let attributesOfType =
                Widgets.tryGetNodeFromWidget<AttributeListNode> widget AttributesOfType

            let multipleAttributesOfType =
                match attributesOfType with
                | ValueSome values -> Some(MultipleAttributeListNode([ values ], Range.Zero))
                | ValueNone -> None

            let tp = Widgets.getNodeFromWidget widget TypeVal

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let name = Widgets.getScalarValue widget Identifier

            let parameters = Widgets.tryGetScalarValue widget Parameters

            let parameters =
                match parameters with
                | ValueSome parameters -> parameters
                | ValueNone -> []

            ExternBindingNode(
                xmlDocs,
                attributes,
                SingleTextNode.``extern``,
                multipleAttributesOfType,
                tp,
                accessControl,
                IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(name)) ], Range.Zero),
                SingleTextNode.leftParenthesis,
                parameters,
                SingleTextNode.rightParenthesis,
                Range.Zero
            ))

[<AutoOpen>]
module ExternBindingNodeBuilders =
    type Ast with
        static member ExternBinding
            (tp: WidgetBuilder<Type>, name: string, parameters: WidgetBuilder<ExternBindingPatternNode> list)
            =
            let parameters = parameters |> List.map Gen.mkOak

            WidgetBuilder<ExternBindingNode>(
                ExternBinding.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        ExternBinding.Identifier.WithValue(name),
                        ExternBinding.Parameters.WithValue(parameters)
                    ),
                    [| ExternBinding.TypeVal.WithValue(tp.Compile()) |],
                    Array.empty
                )
            )

        static member ExternBinding
            (tp: string, name: string, parameters: WidgetBuilder<ExternBindingPatternNode> list)
            =
            Ast.ExternBinding(Ast.LongIdent(tp), name, parameters)

        static member ExternBinding(tp: WidgetBuilder<Type>, name: string) = Ast.ExternBinding(tp, name, [])

        static member ExternBinding(tp: string, name: string) =
            Ast.ExternBinding(Ast.LongIdent(tp), name, [])

        static member ExternBinding
            (tp: WidgetBuilder<Type>, name: string, parameter: WidgetBuilder<ExternBindingPatternNode>)
            =
            Ast.ExternBinding(tp, name, [ parameter ])

        static member ExternBinding(tp: string, name: string, parameter: WidgetBuilder<ExternBindingPatternNode>) =
            Ast.ExternBinding(Ast.LongIdent(tp), name, [ parameter ])

type ExternBindingNodeModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<ExternBindingNode>, comments: string list) =
        this.AddScalar(ExternBinding.XmlDocs.WithValue(comments))

    [<Extension>]
    static member inline attributes
        (this: WidgetBuilder<ExternBindingNode>, attributes: WidgetBuilder<AttributeNode> list)
        =
        this.AddScalar(
            ExternBinding.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<ExternBindingNode>, attribute: WidgetBuilder<AttributeNode>) =
        ExternBindingNodeModifiers.attributes(this, [ attribute ])

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<ExternBindingNode>) =
        this.AddScalar(ExternBinding.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<ExternBindingNode>) =
        this.AddScalar(ExternBinding.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<ExternBindingNode>) =
        this.AddScalar(ExternBinding.Accessibility.WithValue(AccessControl.Internal))

type ExternBindingNodeYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: ExternBindingNode) : CollectionContent =
        let moduleDecl = ModuleDecl.ExternBinding x
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (this: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ExternBindingNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        ExternBindingNodeYieldExtensions.Yield(this, node)
