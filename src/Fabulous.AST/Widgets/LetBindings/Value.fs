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

    let WidgetNameTextNodeKey =
        Widgets.register "Value" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name
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
                Choice1Of2(IdentListNode.Create([ IdentifierOrDot.Ident(name) ])),
                typeParams,
                parameters,
                returnType,
                SingleTextNode.equals,
                value,
                Range.Zero
            ))

    let WidgetNamePatternKey =
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
                name: WidgetBuilder<SingleTextNode>,
                value: WidgetBuilder<Expr>,
                isMutable: bool,
                isInlined: bool,
                typeParams: string list voption
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
                Value.WidgetNameTextNodeKey,
                AttributesBundle(
                    scalars,
                    ValueSome
                        [| Value.Name.WithValue(name.Compile())
                           Value.Value.WithValue(value.Compile()) |],
                    ValueNone
                )
            )

        static member private BasePatternValue
            (
                name: WidgetBuilder<Pattern>,
                value: WidgetBuilder<Expr>,
                isMutable: bool,
                isInlined: bool,
                typeParams: string list voption
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
                Value.WidgetNamePatternKey,
                AttributesBundle(
                    scalars,
                    ValueSome
                        [| Value.Name.WithValue(name.Compile())
                           Value.Value.WithValue(value.Compile()) |],
                    ValueNone
                )
            )

        static member Value(name: WidgetBuilder<SingleTextNode>, value: WidgetBuilder<Expr>) =
            Ast.BaseValue(name, value, false, false, ValueNone)

        static member Value(name: SingleTextNode, value: Expr) =
            Ast.Value(Ast.EscapeHatch(name), Ast.EscapeHatch(value))

        static member Value(name: SingleTextNode, value: WidgetBuilder<Expr>) = Ast.Value(Ast.EscapeHatch(name), value)

        static member Value(name: string, value: Expr) =
            Ast.Value(SingleTextNode.Create(name), value)

        static member Value(name: string, value: WidgetBuilder<Expr>) =
            Ast.Value(EscapeHatch(SingleTextNode.Create(name)), value)

        static member Value(name: string, value: string) =
            Ast.Value(SingleTextNode.Create(name), ConstantExpr(value)) //Expr.Constant(Constant.FromText(SingleTextNode.Create(value))))

        static member Value(name: WidgetBuilder<SingleTextNode>, typeParams: string list, value: WidgetBuilder<Expr>) =
            Ast.BaseValue(name, value, false, false, ValueSome typeParams)

        static member Value(name: SingleTextNode, typeParams: string list, value: Expr) =
            Ast.Value(Ast.EscapeHatch(name), typeParams, Ast.EscapeHatch(value))

        static member Value(name: SingleTextNode, typeParams: string list, value: WidgetBuilder<Expr>) =
            Ast.Value(Ast.EscapeHatch(name), typeParams, value)

        static member Value(name: string, typeParams: string list, value: Expr) =
            Ast.Value(SingleTextNode.Create(name), typeParams, value)

        static member Value(name: string, typeParams: string list, value: WidgetBuilder<Expr>) =
            Ast.Value(EscapeHatch(SingleTextNode.Create(name)), typeParams, value)

        static member Value(name: string, typeParams: string list, value: string) =
            Ast.Value(SingleTextNode.Create(name), typeParams, ConstantExpr(value))

        static member Value(name: WidgetBuilder<Pattern>, value: WidgetBuilder<Expr>) =
            Ast.BasePatternValue(name, value, false, false, ValueNone)

        static member MutableValue(name: WidgetBuilder<SingleTextNode>, value: WidgetBuilder<Expr>) =
            Ast.BaseValue(name, value, true, false, ValueNone)

        static member MutableValue(name: SingleTextNode, value: Expr) =
            Ast.MutableValue(Ast.EscapeHatch(name), Ast.EscapeHatch(value))

        static member MutableValue(name: SingleTextNode, value: WidgetBuilder<Expr>) =
            Ast.MutableValue(Ast.EscapeHatch(name), value)

        static member MutableValue(name: string, value: Expr) =
            Ast.MutableValue(SingleTextNode.Create(name), value)

        static member MutableValue(name: string, value: WidgetBuilder<Expr>) =
            Ast.MutableValue(EscapeHatch(SingleTextNode.Create(name)), value)

        static member MutableValue(name: string, value: string) =
            Ast.MutableValue(SingleTextNode.Create(name), ConstantExpr(value))

        static member InlinedValue(name: WidgetBuilder<SingleTextNode>, value: WidgetBuilder<Expr>) =
            Ast.BaseValue(name, value, false, true, ValueNone)

        static member InlinedValue(name: SingleTextNode, value: Expr) =
            Ast.InlinedValue(Ast.EscapeHatch(name), Ast.EscapeHatch(value))

        static member InlinedValue(name: SingleTextNode, value: WidgetBuilder<Expr>) =
            Ast.InlinedValue(Ast.EscapeHatch(name), value)

        static member InlinedValue(name: string, value: Expr) =
            Ast.InlinedValue(SingleTextNode.Create(name), value)

        static member InlinedValue(name: string, value: WidgetBuilder<Expr>) =
            Ast.InlinedValue(EscapeHatch(SingleTextNode.Create(name)), value)

        static member InlinedValue(name: string, value: string) =
            Ast.InlinedValue(SingleTextNode.Create(name), ConstantExpr(value))

        static member InlinedValue
            (
                name: WidgetBuilder<SingleTextNode>,
                typeParams: string list,
                value: WidgetBuilder<Expr>
            ) =
            Ast.BaseValue(name, value, false, true, ValueSome typeParams)

        static member InlinedValue(name: SingleTextNode, typeParams: string list, value: Expr) =
            Ast.InlinedValue(Ast.EscapeHatch(name), typeParams, Ast.EscapeHatch(value))

        static member InlinedValue(name: SingleTextNode, typeParams: string list, value: WidgetBuilder<Expr>) =
            Ast.InlinedValue(Ast.EscapeHatch(name), typeParams, value)

        static member InlinedValue(name: string, typeParams: string list, value: Expr) =
            Ast.InlinedValue(SingleTextNode.Create(name), typeParams, value)

        static member InlinedValue(name: string, typeParams: string list, value: WidgetBuilder<Expr>) =
            Ast.InlinedValue(EscapeHatch(SingleTextNode.Create(name)), typeParams, value)

        static member InlinedValue(name: string, typeParams: string list, value: string) =
            Ast.InlinedValue(SingleTextNode.Create(name), typeParams, ConstantExpr(value))

        static member private BaseFunction
            (
                name: WidgetBuilder<SingleTextNode>,
                parameters: WidgetBuilder<Pattern>,
                isInlined: bool,
                typeParams: string list voption
            ) =
            let scalars =
                match typeParams with
                | ValueNone -> StackList.one(Value.IsInlined.WithValue(isInlined))
                | ValueSome typeParams ->
                    StackList.two(Value.IsInlined.WithValue(isInlined), Value.TypeParams.WithValue(typeParams))

            SingleChildBuilder<BindingNode, Expr>(
                Value.WidgetNameTextNodeKey,
                Value.Value,
                AttributesBundle(
                    scalars,
                    ValueSome
                        [| Value.Name.WithValue(name.Compile())
                           Value.Parameters.WithValue(parameters.Compile()) |],
                    ValueNone
                )
            )

        static member Function(name: WidgetBuilder<SingleTextNode>, parameters: WidgetBuilder<Pattern>) =
            Ast.BaseFunction(name, parameters, false, ValueNone)

        static member inline Function(name: SingleTextNode, parameters: Pattern) =
            Ast.Function(Ast.EscapeHatch(name), Ast.EscapeHatch(parameters))

        static member inline Function(name: string, parameters: Pattern) =
            Ast.Function(SingleTextNode.Create(name), parameters)

        static member Function(name: string, parameters: WidgetBuilder<Pattern>) =
            Ast.Function(EscapeHatch(SingleTextNode.Create(name)), parameters)

        static member Function
            (
                name: WidgetBuilder<SingleTextNode>,
                typeParams: string list,
                parameters: WidgetBuilder<Pattern>
            ) =
            Ast.BaseFunction(name, parameters, false, ValueSome typeParams)

        static member inline Function(name: SingleTextNode, typeParams: string list, parameters: Pattern) =
            Ast.Function(Ast.EscapeHatch(name), typeParams, Ast.EscapeHatch(parameters))

        static member inline Function(name: string, typeParams: string list, parameters: Pattern) =
            Ast.Function(SingleTextNode.Create(name), typeParams, parameters)

        static member Function(name: string, typeParams: string list, parameters: WidgetBuilder<Pattern>) =
            Ast.Function(EscapeHatch(SingleTextNode.Create(name)), typeParams, parameters)

        static member InlinedFunction(name: WidgetBuilder<SingleTextNode>, parameters: WidgetBuilder<Pattern>) =
            Ast.BaseFunction(name, parameters, true, ValueNone)

        static member inline InlinedFunction(name: SingleTextNode, parameters: Pattern) =
            Ast.InlinedFunction(Ast.EscapeHatch(name), Ast.EscapeHatch(parameters))

        static member inline InlinedFunction(name: string, parameters: Pattern) =
            Ast.InlinedFunction(SingleTextNode.Create(name), parameters)

        static member InlinedFunction(name: string, parameters: WidgetBuilder<Pattern>) =
            Ast.InlinedFunction(EscapeHatch(SingleTextNode.Create(name)), parameters)

        static member InlinedFunction
            (
                name: WidgetBuilder<SingleTextNode>,
                typeParams: string list,
                parameters: WidgetBuilder<Pattern>
            ) =
            Ast.BaseFunction(name, parameters, true, ValueSome typeParams)

        static member inline InlinedFunction(name: SingleTextNode, typeParams: string list, parameters: Pattern) =
            Ast.InlinedFunction(Ast.EscapeHatch(name), typeParams, Ast.EscapeHatch(parameters))

        static member inline InlinedFunction(name: string, typeParams: string list, parameters: Pattern) =
            Ast.InlinedFunction(SingleTextNode.Create(name), typeParams, parameters)

        static member InlinedFunction(name: string, typeParams: string list, parameters: WidgetBuilder<Pattern>) =
            Ast.InlinedFunction(EscapeHatch(SingleTextNode.Create(name)), typeParams, parameters)

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
