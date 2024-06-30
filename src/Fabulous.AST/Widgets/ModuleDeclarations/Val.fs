namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module Val =
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let MultipleAttributes =
        Attributes.defineScalar<AttributeNode list> "MultipleAttributes"

    let LeadingKeyword = Attributes.defineScalar<SingleTextNode list> "LeadingKeyword"

    let IsInlined = Attributes.defineScalar<bool> "Inlined"

    let IsMutable = Attributes.defineScalar<bool> "IsMutable"

    let Identifier = Attributes.defineScalar<string> "Identifier"

    let ReturnType = Attributes.defineWidget "Identifier"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let TypeParams = Attributes.defineWidget "TypeParams"

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
                Widgets.tryGetScalarValue widget MultipleAttributes
                |> ValueOption.map(fun x -> Some(MultipleAttributeListNode.Create(x)))
                |> ValueOption.defaultValue None

            let inlined = Widgets.tryGetScalarValue widget IsInlined

            let inlined =
                match inlined with
                | ValueSome _ -> Some(SingleTextNode.``inline``)
                | ValueNone -> None

            let isMutable =
                Widgets.tryGetScalarValue widget IsMutable |> ValueOption.defaultValue(false)

            let identifier = Widgets.getScalarValue widget Identifier
            let identifier = SingleTextNode.Create(identifier)

            let returnType = Widgets.getNodeFromWidget widget ReturnType

            let accessControl =
                Widgets.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let typeParams =
                Widgets.tryGetNodeFromWidget widget TypeParams
                |> ValueOption.map Some
                |> ValueOption.defaultValue None

            let leadingKeyword =
                Widgets.tryGetScalarValue widget LeadingKeyword
                |> ValueOption.map(fun x ->
                    match x with
                    | [] -> None
                    | nodes -> Some(MultipleTextsNode(nodes, Range.Zero)))
                |> ValueOption.defaultValue None

            ValNode(
                xmlDocs,
                attributes,
                leadingKeyword,
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
        static member private BaseValField
            (leadingKeyword: SingleTextNode list, identifier: string, returnType: WidgetBuilder<Type>)
            =
            WidgetBuilder<ValNode>(
                Val.WidgetKey,
                AttributesBundle(
                    StackList.two(Val.Identifier.WithValue(identifier), Val.LeadingKeyword.WithValue(leadingKeyword)),
                    [| Val.ReturnType.WithValue(returnType.Compile()) |],
                    Array.empty
                )
            )

        static member ValField(identifier: string, returnType: WidgetBuilder<Type>) =
            Ast.BaseValField([ SingleTextNode.``val`` ], identifier, returnType)

        static member ValField(identifier: string, returnType: string) =
            Ast.ValField(identifier, Ast.LongIdent(returnType))

        static member ValField(leadingKeyword: string list, identifier: string, returnType: WidgetBuilder<Type>) =
            Ast.BaseValField([ for kw in leadingKeyword -> SingleTextNode.Create(kw) ], identifier, returnType)

        static member ValField(leadingKeyword: string list, identifier: string, returnType: string) =
            Ast.BaseValField(
                [ for kw in leadingKeyword -> SingleTextNode.Create(kw) ],
                identifier,
                Ast.LongIdent(returnType)
            )

        static member ValField(leadingKeyword: string, identifier: string, returnType: WidgetBuilder<Type>) =
            Ast.BaseValField([ SingleTextNode.Create(leadingKeyword) ], identifier, returnType)

        static member ValField(leadingKeyword: string, identifier: string, returnType: string) =
            Ast.BaseValField([ SingleTextNode.Create(leadingKeyword) ], identifier, Ast.LongIdent(returnType))

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
    static member inline attributes(this: WidgetBuilder<ValNode>, attributes: WidgetBuilder<AttributeNode> list) =
        this.AddScalar(
            Val.MultipleAttributes.WithValue(
                [ for attr in attributes do
                      Gen.mkOak attr ]
            )
        )

    [<Extension>]
    static member inline attribute(this: WidgetBuilder<ValNode>, attribute: WidgetBuilder<AttributeNode>) =
        ValNodeModifiers.attributes(this, [ attribute ])

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
    static member inline typeParams(this: WidgetBuilder<ValNode>, typeParams: WidgetBuilder<TyparDecls>) =
        this.AddWidget(Val.TypeParams.WithValue(typeParams.Compile()))

type ValYieldExtensions =
    [<Extension>]
    static member inline Yield
        (_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<ValNode>)
        : CollectionContent =
        let node = Gen.mkOak x
        let moduleDecl = ModuleDecl.Val node
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }

    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, node: ValNode) : CollectionContent =
        let moduleDecl = ModuleDecl.Val node
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
