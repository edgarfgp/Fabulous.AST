namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

type FunctionBindingNode
    (xmlDocs, multipleAttributes, isInlined, accessControl, name, typeParams, parameters, returnType, value) =
    inherit
        BindingNode(
            xmlDocs,
            multipleAttributes,
            MultipleTextsNode([ SingleTextNode.``let`` ], Range.Zero),
            false,
            (if isInlined then Some(SingleTextNode.``inline``) else None),
            accessControl,
            Choice2Of2(name),
            typeParams,
            parameters,
            returnType,
            SingleTextNode.equals,
            value,
            Range.Zero
        )

module Function =
    let Name = Attributes.defineWidget "Name"
    let Value = Attributes.defineWidget "Value"
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

            FunctionBindingNode(
                xmlDocs,
                multipleAttributes,
                isInlined,
                accessControl,
                name,
                typeParams,
                parameters,
                returnType,
                value
            ))


[<AutoOpen>]
module FunctionBuilders =
    type Ast with
        static member private BaseFunction
            (
                name: WidgetBuilder<Pattern>,
                isInlined: bool,
                typeParams: string list voption,
                parameters: WidgetBuilder<Pattern>
            ) =
            let scalars =
                match typeParams with
                | ValueNone -> StackList.one(Function.IsInlined.WithValue(isInlined))
                | ValueSome typeParams ->
                    StackList.two(Function.IsInlined.WithValue(isInlined), Function.TypeParams.WithValue(typeParams))

            SingleChildBuilder<FunctionBindingNode, Expr>(
                Function.WidgetKey,
                Function.Value,
                AttributesBundle(
                    scalars,
                    ValueSome
                        [| Function.Name.WithValue(name.Compile())
                           Function.Parameters.WithValue(parameters.Compile()) |],
                    ValueNone
                )
            )

        static member Function(name: WidgetBuilder<Pattern>, parameters: WidgetBuilder<Pattern>) =
            Ast.BaseFunction(name, false, ValueNone, parameters)

        static member Function(name: string, parameters: WidgetBuilder<Pattern>) =
            Ast.BaseFunction(NamedPat(name), false, ValueNone, parameters)

        static member Function
            (
                name: WidgetBuilder<Pattern>,
                typeParams: string list,
                parameters: WidgetBuilder<Pattern>
            ) =
            Ast.BaseFunction(name, false, ValueSome typeParams, parameters)

        static member Function(name: string, typeParams: string list, parameters: WidgetBuilder<Pattern>) =
            Ast.BaseFunction(NamedPat(name), false, ValueSome typeParams, parameters)

        static member InlinedFunction(name: WidgetBuilder<Pattern>, parameters: WidgetBuilder<Pattern>) =
            Ast.BaseFunction(name, true, ValueNone, parameters)

        static member InlinedFunction(name: string, parameters: WidgetBuilder<Pattern>) =
            Ast.BaseFunction(NamedPat(name), true, ValueNone, parameters)

        static member InlinedFunction
            (
                name: WidgetBuilder<Pattern>,
                typeParams: string list,
                parameters: WidgetBuilder<Pattern>
            ) =
            Ast.BaseFunction(name, true, ValueSome typeParams, parameters)

        static member InlinedFunction(name: string, typeParams: string list, parameters: WidgetBuilder<Pattern>) =
            Ast.BaseFunction(NamedPat(name), true, ValueSome typeParams, parameters)

[<Extension>]
type FunctionModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<FunctionBindingNode>, xmlDocs: string list) =
        this.AddScalar(Function.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<FunctionBindingNode>, attributes: string list) =
        this.AddScalar(Function.MultipleAttributes.WithValue(attributes))

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<FunctionBindingNode>) =
        this.AddScalar(Function.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<FunctionBindingNode>) =
        this.AddScalar(Function.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<FunctionBindingNode>) =
        this.AddScalar(Function.Accessibility.WithValue(AccessControl.Internal))

    [<Extension>]
    static member inline returnType(this: WidgetBuilder<FunctionBindingNode>, returnType: Type) =
        this.AddScalar(Function.Return.WithValue(returnType))

[<Extension>]
type FunctionYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<FunctionBindingNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let moduleDecl = ModuleDecl.TopLevelBinding node
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
