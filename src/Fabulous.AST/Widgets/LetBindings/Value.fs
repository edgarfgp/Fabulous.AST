namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module Value =
    let Name = Attributes.defineWidget "Name"
    let Value = Attributes.defineWidget "Value"
    let IsMutable = Attributes.defineScalar<bool> "IsMutable"
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"
    let IsInlined = Attributes.defineScalar<bool> "IsInlined"
    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"
    let Return = Attributes.defineScalar<Type> "Return"
    let TypeParams = Attributes.defineScalar<string list> "TypeParams"
    let Parameters = Attributes.defineWidget "Parameters"

    let WidgetKey =
        Widgets.register "Value" (fun widget ->
            let name = Helpers.getNodeFromWidget<Pattern> widget Name
            let value = Helpers.getNodeFromWidget<Expr> widget Value
            let parameters = Helpers.tryGetNodeFromWidget<Pattern> widget Parameters

            let accessControl =
                Helpers.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let lines = Helpers.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            let attributes = Helpers.tryGetScalarValue widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | ValueSome values -> MultipleAttributeListNode.Create values |> Some
                | ValueNone -> None

            let isMutable =
                Helpers.tryGetScalarValue widget IsMutable |> ValueOption.defaultValue false

            let isInlined =
                Helpers.tryGetScalarValue widget IsInlined |> ValueOption.defaultValue false

            let returnType = Helpers.tryGetScalarValue widget Return

            let returnType =
                match returnType with
                | ValueSome returnType -> Some(BindingReturnInfoNode(SingleTextNode.colon, returnType, Range.Zero))
                | ValueNone -> None

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

            let parameters =
                match parameters with
                | ValueSome parameters -> [ parameters ]
                | ValueNone -> []

            BindingNode(
                xmlDocs,
                multipleAttributes,
                MultipleTextsNode([ SingleTextNode.``let`` ], Range.Zero),
                isMutable,
                (if isInlined then Some(SingleTextNode.``inline``) else None),
                accessControl,
                Choice2Of2(name),
                typeParams,
                parameters,
                returnType,
                SingleTextNode.equals,
                value,
                Range.Zero
            ))


[<AutoOpen>]
module ValueBuilders =
    type Ast with
        static member private BaseValue
            (
                name: WidgetBuilder<Pattern>,
                isMutable: bool,
                isInlined: bool,
                typeParams: string list voption,
                value: WidgetBuilder<Expr>
            ) =
            let scalars =
                match typeParams with
                | ValueNone -> StackList.two(Value.IsMutable.WithValue(isMutable), Value.IsInlined.WithValue(isInlined))
                | ValueSome typeParams ->
                    StackList.three(
                        Value.IsMutable.WithValue(isMutable),
                        Value.IsInlined.WithValue(isInlined),
                        Value.TypeParams.WithValue(typeParams)
                    )

            WidgetBuilder<BindingNode>(
                Value.WidgetKey,
                AttributesBundle(
                    scalars,
                    ValueSome
                        [| Value.Name.WithValue(name.Compile())
                           Value.Value.WithValue(value.Compile()) |],
                    ValueNone
                )
            )

        static member Value(name: WidgetBuilder<Pattern>, value: WidgetBuilder<Expr>) =
            Ast.BaseValue(name, false, false, ValueNone, value)

        static member Value(name: string, value: WidgetBuilder<Expr>) =
            Ast.BaseValue(NamedPat(name), false, false, ValueNone, value)

        static member Value(name: WidgetBuilder<Pattern>, value: string) =
            Ast.BaseValue(name, false, false, ValueNone, ConstantExpr(ConstantString(value)))

        static member Value(name: WidgetBuilder<Pattern>, typeParams: string list, value: WidgetBuilder<Expr>) =
            Ast.BaseValue(name, false, false, ValueSome typeParams, value)

        static member Value(name: string, typeParams: string list, value: WidgetBuilder<Expr>) =
            Ast.BaseValue(NamedPat(name), false, false, ValueSome typeParams, value)

        static member Value(name: string, value: string) =
            Ast.BaseValue(NamedPat(name), false, false, ValueNone, ConstantExpr(ConstantString(value)))

        static member Value(name: string, typeParams: string list, value: string) =
            Ast.BaseValue(NamedPat(name), false, false, ValueSome typeParams, ConstantExpr(ConstantString(value)))

        static member MutableValue(name: WidgetBuilder<Pattern>, value: WidgetBuilder<Expr>) =
            Ast.BaseValue(name, true, false, ValueNone, value)

        static member MutableValue(name: WidgetBuilder<Pattern>, value: string) =
            Ast.BaseValue(name, true, false, ValueNone, ConstantExpr(ConstantString(value)))

        static member MutableValue(name: string, value: WidgetBuilder<Expr>) =
            Ast.BaseValue(NamedPat(name), true, false, ValueNone, value)

        static member MutableValue(name: string, value: string) =
            Ast.BaseValue(NamedPat(name), true, false, ValueNone, ConstantExpr(ConstantString(value)))

        static member InlinedValue(name: WidgetBuilder<Pattern>, typeParams: string list, value: WidgetBuilder<Expr>) =
            Ast.BaseValue(name, false, true, ValueSome typeParams, value)

        static member InlinedValue(name: string, typeParams: string list, value: WidgetBuilder<Expr>) =
            Ast.BaseValue(NamedPat(name), false, true, ValueSome typeParams, value)

        static member InlinedValue(name: WidgetBuilder<Pattern>, typeParams: string list, value: string) =
            Ast.BaseValue(name, false, true, ValueSome typeParams, ConstantExpr(ConstantString(value)))

        static member InlinedValue(name: string, typeParams: string list, value: string) =
            Ast.BaseValue(NamedPat(name), false, true, ValueSome typeParams, ConstantExpr(ConstantString(value)))

[<Extension>]
type ValueModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<BindingNode>, xmlDocs: string list) =
        this.AddScalar(Value.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<BindingNode>, attributes: string list) =
        this.AddScalar(Value.MultipleAttributes.WithValue(attributes))

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<BindingNode>) =
        this.AddScalar(Value.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<BindingNode>) =
        this.AddScalar(Value.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<BindingNode>) =
        this.AddScalar(Value.Accessibility.WithValue(AccessControl.Internal))

    [<Extension>]
    static member inline returnType(this: WidgetBuilder<BindingNode>, returnType: Type) =
        this.AddScalar(Value.Return.WithValue(returnType))

[<Extension>]
type ValueYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<BindingNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let moduleDecl = ModuleDecl.TopLevelBinding node
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
