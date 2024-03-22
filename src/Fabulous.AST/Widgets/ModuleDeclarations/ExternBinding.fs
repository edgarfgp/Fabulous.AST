namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module ExternBinding =
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let AttributesOfType = Attributes.defineWidget "MultipleAttributes"
    let TypeVal = Attributes.defineScalar<StringOrWidget<Type>> "Type"

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

            let attributesOfType =
                Widgets.tryGetNodeFromWidget<AttributeListNode> widget AttributesOfType

            let multipleAttributesOfType =
                match attributesOfType with
                | ValueSome values -> Some(MultipleAttributeListNode([ values ], Range.Zero))
                | ValueNone -> None

            let tp = Widgets.getScalarValue widget TypeVal

            let tp =
                match tp with
                | StringOrWidget.StringExpr value ->
                    Type.LongIdent(
                        IdentListNode([ IdentifierOrDot.Ident(SingleTextNode.Create(value.Normalize())) ], Range.Zero)
                    )
                | StringOrWidget.WidgetExpr widget -> widget

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
                multipleAttributes,
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

        static member ExternBinding(name: string, ``type``: string) =
            WidgetBuilder<ExternBindingNode>(
                ExternBinding.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        ExternBinding.Identifier.WithValue(name),
                        ExternBinding.TypeVal.WithValue(StringOrWidget.StringExpr(Unquoted ``type``))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member ExternBinding(name: string, ``type``: WidgetBuilder<Type>) =
            WidgetBuilder<ExternBindingNode>(
                ExternBinding.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        ExternBinding.Identifier.WithValue(name),
                        ExternBinding.TypeVal.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak(``type``)))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

[<Extension>]
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
    static member inline attributes(this: WidgetBuilder<ExternBindingNode>, attributes: string list) =
        ExternBindingNodeModifiers.attributes(
            this,
            [ for attribute in attributes do
                  Ast.Attribute(attribute) ]
        )

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<ExternBindingNode>, attribute: WidgetBuilder<AttributeNode>) =
        ExternBindingNodeModifiers.attributes(this, [ attribute ])

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<ExternBindingNode>, attribute: string) =
        ExternBindingNodeModifiers.attributes(this, [ Ast.Attribute(attribute) ])

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
    static member inline parameters
        (this: WidgetBuilder<ExternBindingNode>, values: WidgetBuilder<ExternBindingPatternNode> list)
        =
        this.AddScalar(
            ExternBinding.Parameters.WithValue(
                [ for vals in values do
                      Gen.mkOak vals ]
            )
        )

    [<Extension>]
    static member inline parameter
        (this: WidgetBuilder<ExternBindingNode>, value: WidgetBuilder<ExternBindingPatternNode>)
        =
        ExternBindingNodeModifiers.parameters(this, [ value ])

[<Extension>]
type ExternBindingNodeYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ExternBindingNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let moduleDecl = ModuleDecl.ExternBinding node
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
