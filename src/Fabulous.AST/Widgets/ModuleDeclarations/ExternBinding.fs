namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module ExternBinding =
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"
    let MultipleAttributes = Attributes.defineWidgetCollection "MultipleAttributes"
    let AttributesOfType = Attributes.defineWidget "MultipleAttributes"
    let Type = Attributes.defineScalar<Type> "Type"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"
    let Identifier = Attributes.defineScalar<string> "Identifier"
    let Parameters = Attributes.defineWidgetCollection "Parameters"

    let WidgetKey =
        Widgets.register "ModuleDeclAttributes" (fun widget ->
            let lines = Helpers.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            let attributes =
                Helpers.tryGetNodesFromWidgetCollection<AttributeNode> widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | Some values ->
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
                | None -> None

            let attributesOfType =
                Helpers.tryGetNodeFromWidget<AttributeListNode> widget AttributesOfType

            let multipleAttributesOfType =
                match attributesOfType with
                | ValueSome values -> Some(MultipleAttributeListNode([ values ], Range.Zero))
                | ValueNone -> None

            let ``type`` = Helpers.getScalarValue widget Type

            let accessControl =
                Helpers.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let name = Helpers.getScalarValue widget Identifier

            let parameters =
                Helpers.tryGetNodesFromWidgetCollection<ExternBindingPatternNode> widget Parameters

            let parameters =
                match parameters with
                | Some parameters -> parameters
                | None -> []

            ExternBindingNode(
                xmlDocs,
                multipleAttributes,
                SingleTextNode.``extern``,
                multipleAttributesOfType,
                ``type``,
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

        static member ExternBindingNode(name: string, ``type``: string) =
            WidgetBuilder<ExternBindingNode>(
                ExternBinding.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        ExternBinding.Identifier.WithValue(name),
                        ExternBinding.Type.WithValue(CommonType.mkLongIdent(``type``))
                    ),
                    ValueSome [||],
                    ValueNone
                )
            )

[<Extension>]
type ExternBindingNodeModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<ExternBindingNode>, comments: string list) =
        this.AddScalar(ExternBinding.XmlDocs.WithValue(comments))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<ExternBindingNode>) =
        AttributeCollectionBuilder<ExternBindingNode, AttributeNode>(this, ExternBinding.MultipleAttributes)

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<ExternBindingNode>, attributes: string list) =
        AttributeCollectionBuilder<ExternBindingNode, AttributeNode>(this, ExternBinding.MultipleAttributes) {
            for attribute in attributes do
                Ast.Attribute(attribute)
        }

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<ExternBindingNode>, attribute: WidgetBuilder<AttributeNode>) =
        AttributeCollectionBuilder<ExternBindingNode, AttributeNode>(this, ExternBinding.MultipleAttributes) {
            attribute
        }

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<ExternBindingNode>, attribute: string) =
        AttributeCollectionBuilder<ExternBindingNode, AttributeNode>(this, ExternBinding.MultipleAttributes) {
            Ast.Attribute(attribute)
        }

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<ExternBindingNode>) =
        this.AddScalar(ExternBinding.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<ExternBindingNode>) =
        this.AddScalar(ExternBinding.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<ExternBindingNode>) =
        this.AddScalar(ExternBinding.Accessibility.WithValue(AccessControl.Internal))

    [<Extension>]
    static member inline parameter
        (
            this: WidgetBuilder<ExternBindingNode>,
            parameter: WidgetBuilder<ExternBindingPatternNode>
        ) =
        AttributeCollectionBuilder<ExternBindingNode, ExternBindingPatternNode>(this, ExternBinding.Parameters) {
            parameter
        }

    [<Extension>]
    static member inline parameters(this: WidgetBuilder<ExternBindingNode>) =
        AttributeCollectionBuilder<ExternBindingNode, ExternBindingPatternNode>(this, ExternBinding.Parameters)

[<Extension>]
type ExternBindingNodeYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<ExternBindingNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let moduleDecl = ModuleDecl.ExternBinding node
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield
        (
            _: AttributeCollectionBuilder<ExternBindingNode, ExternBindingPatternNode>,
            x: WidgetBuilder<ExternBindingPatternNode>
        ) : CollectionContent =
        { Widgets = MutStackArray1.One(x.Compile()) }
