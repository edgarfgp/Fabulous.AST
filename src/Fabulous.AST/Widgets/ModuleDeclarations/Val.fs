namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module Val =
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"
    let MultipleAttributes = Attributes.defineWidgetCollection "MultipleAttributes"

    let LeadingKeyword = Attributes.defineScalar<string list> "LeadingKeyword"

    let IsInlined = Attributes.defineScalar<bool> "Inlined"

    let IsMutable = Attributes.defineScalar<bool> "IsMutable"

    let Identifier = Attributes.defineScalar<string> "Identifier"

    let ReturnType = Attributes.defineWidget "Identifier"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let TypeParams = Attributes.defineScalar<string list> "TypeParams"

    let WidgetKey =
        Widgets.register "ValNode" (fun widget ->
            let lines = Widgets.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            let attributes =
                Widgets.tryGetNodesFromWidgetCollection<AttributeNode> widget MultipleAttributes

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

            let inlined = Widgets.tryGetScalarValue widget IsInlined

            let inlined =
                match inlined with
                | ValueSome _ -> Some(SingleTextNode.``inline``)
                | ValueNone -> None

            let isMutable =
                Widgets.tryGetScalarValue widget IsMutable |> ValueOption.defaultValue(false)

            let identifier = Widgets.getScalarValue widget Identifier
            let identifier = SingleTextNode.Create(identifier)

            let returnType = Widgets.getNodeFromWidget<Type> widget ReturnType

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let typeParams = Widgets.tryGetScalarValue widget TypeParams

            let typeParams =
                match typeParams with
                | ValueSome values ->
                    TyparDeclsPostfixListNode(
                        SingleTextNode.lessThan,
                        [ for v in values do
                              TyparDeclNode(None, SingleTextNode.Create v, [], Range.Zero) ],
                        [],
                        SingleTextNode.greaterThan,
                        Range.Zero
                    )
                    |> TyparDecls.PostfixList
                    |> Some
                | ValueNone -> None

            ValNode(
                xmlDocs,
                multipleAttributes,
                Some(MultipleTextsNode([ SingleTextNode.``val`` ], Range.Zero)),
                inlined,
                isMutable,
                accessControl,
                identifier,
                typeParams,
                returnType,
                None,
                None,
                Range.Zero
            ))

[<AutoOpen>]
module ValBuilders =
    type Ast with

        static member inline BaseVal(identifier: string, returnType: WidgetBuilder<Type>) =
            WidgetBuilder<ValNode>(
                Val.WidgetKey,
                AttributesBundle(
                    StackList.one(Val.Identifier.WithValue(identifier)),
                    [| Val.ReturnType.WithValue(returnType.Compile()) |],
                    Array.empty
                )
            )

        static member Val(identifier: string, returnType: WidgetBuilder<Type>) = Ast.BaseVal(identifier, returnType)

        static member Val(identifier: string, returnType: string) =
            Ast.BaseVal(identifier, Ast.LongIdent(returnType))

[<Extension>]
type ValNodeModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<ValNode>, comments: string list) =
        this.AddScalar(Val.XmlDocs.WithValue(comments))

    [<Extension>]
    static member inline toMutable(this: WidgetBuilder<ValNode>) =
        this.AddScalar(Val.IsMutable.WithValue(true))

    [<Extension>]
    static member inline toInlined(this: WidgetBuilder<ValNode>) =
        this.AddScalar(Val.IsInlined.WithValue(true))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<ValNode>) =
        AttributeCollectionBuilder<ValNode, AttributeNode>(this, Val.MultipleAttributes)

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<ValNode>, attributes: string list) =
        AttributeCollectionBuilder<ValNode, AttributeNode>(this, Val.MultipleAttributes) {
            for attribute in attributes do
                Ast.Attribute(attribute)
        }

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<ValNode>, attribute: WidgetBuilder<AttributeNode>) =
        AttributeCollectionBuilder<ValNode, AttributeNode>(this, Val.MultipleAttributes) { attribute }

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<ValNode>, attribute: string) =
        AttributeCollectionBuilder<ValNode, AttributeNode>(this, Val.MultipleAttributes) { Ast.Attribute(attribute) }

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<ValNode>) =
        this.AddScalar(Val.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<ValNode>) =
        this.AddScalar(Val.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<ValNode>) =
        this.AddScalar(Val.Accessibility.WithValue(AccessControl.Internal))

    [<Extension>]
    static member inline typeParams(this: WidgetBuilder<ValNode>, typeParams: string list) =
        this.AddScalar(Val.TypeParams.WithValue(typeParams))

[<Extension>]
type ValYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ValNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let moduleDecl = ModuleDecl.Val node
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
