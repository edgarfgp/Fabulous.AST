namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module Val =
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"
    let MultipleAttributes = Attributes.defineWidget "MultipleAttributes"

    let LeadingKeyword = Attributes.defineScalar<string list> "LeadingKeyword"

    let Inlined = Attributes.defineScalar<bool> "Inlined"

    let IsMutable = Attributes.defineScalar<bool> "IsMutable"

    let IdentifierReturnType =
        Attributes.defineScalar<struct (string * Type)> "Identifier"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let TypeParams = Attributes.defineScalar<string list> "TypeParams"

    let WidgetKey =
        Widgets.register "OpenType" (fun widget ->
            let lines = Helpers.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            let attributes =
                Helpers.tryGetNodeFromWidget<AttributeListNode> widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | ValueSome values -> Some(MultipleAttributeListNode([ values ], Range.Zero))
                | ValueNone -> None

            let inlined =
                if Helpers.getScalarValue widget Inlined then
                    Some(SingleTextNode.``inline``)
                else
                    None

            let isMutable =
                Helpers.tryGetScalarValue widget IsMutable |> ValueOption.defaultValue(false)

            let struct (identifier, returnType) =
                Helpers.getScalarValue widget IdentifierReturnType

            let identifier = SingleTextNode.Create(identifier)

            let accessControl =
                Helpers.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let typeParams = Helpers.tryGetScalarValue widget TypeParams

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

        static member inline BaseVal(identifier: string, isMutable: bool, isInlined: bool, returnType: Type) =
            WidgetBuilder<ValNode>(
                Val.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        Val.IdentifierReturnType.WithValue(struct (identifier, returnType)),
                        Val.IsMutable.WithValue(isMutable),
                        Val.Inlined.WithValue(isInlined)
                    ),
                    ValueNone,
                    ValueNone
                )
            )

        static member Val(identifier: string, returnType: Type) =
            Ast.BaseVal(identifier, false, false, returnType)

        static member Val(identifier: string, returnType: string) =
            Ast.BaseVal(identifier, false, false, CommonType.mkLongIdent(returnType))

        static member MutableVal(identifier: string, returnType: Type) =
            Ast.BaseVal(identifier, true, false, returnType)

        static member MutableVal(identifier: string, returnType: string) =
            Ast.BaseVal(identifier, true, false, CommonType.mkLongIdent(returnType))

        static member InlinedVal(identifier: string, returnType: Type) =
            Ast.BaseVal(identifier, false, true, returnType)

        static member InlinedVal(identifier: string, returnType: string) =
            Ast.BaseVal(identifier, false, true, CommonType.mkLongIdent(returnType))

[<Extension>]
type ValNodeModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<ValNode>, comments: string list) =
        this.AddScalar(Val.XmlDocs.WithValue(comments))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<ValNode>, attributes: WidgetBuilder<AttributeListNode>) =
        this.AddWidget(Val.MultipleAttributes.WithValue(attributes.Compile()))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<ValNode>, attribute: WidgetBuilder<AttributeNode>) =
        this.AddWidget(Val.MultipleAttributes.WithValue((Ast.AttributeNodes() { attribute }).Compile()))

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
    static member inline typeParameters(this: WidgetBuilder<ValNode>, typeParams: string list) =
        this.AddScalar(Val.TypeParams.WithValue(typeParams))

[<Extension>]
type ValYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<ValNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let moduleDecl = ModuleDecl.Val node
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
