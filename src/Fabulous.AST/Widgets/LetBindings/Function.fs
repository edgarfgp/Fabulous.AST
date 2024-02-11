namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

type FunctionNode
    (xmlDocs, multipleAttributes, isInlined, accessControl, name, typeParams, parameters, returnType, value) =
    inherit
        BindingNode(
            xmlDocs,
            multipleAttributes,
            MultipleTextsNode([ SingleTextNode.``let`` ], Range.Zero),
            false,
            (if isInlined then Some(SingleTextNode.``inline``) else None),
            accessControl,
            Choice1Of2(IdentListNode.Create(IdentifierOrDot.CreateIdent name)),
            typeParams,
            parameters,
            returnType,
            SingleTextNode.equals,
            value,
            Range.Zero
        )

module Function =
    let Name = Attributes.defineScalar "Name"
    let Value = Attributes.defineWidget "Value"
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"
    let IsInlined = Attributes.defineScalar<bool> "IsInlined"
    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"
    let Return = Attributes.defineScalar<Type> "Return"
    let TypeParams = Attributes.defineScalar<string list> "TypeParams"
    let Parameters = Attributes.defineWidget "Parameters"

    let WidgetKey =
        Widgets.register "Function" (fun widget ->
            let name = Helpers.getScalarValue widget Name
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

            FunctionNode(
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
                name: string,
                isInlined: bool,
                typeParams: string list voption,
                parameters: WidgetBuilder<Pattern>
            ) =
            let scalars =
                match typeParams with
                | ValueNone -> StackList.two(Function.Name.WithValue(name), Function.IsInlined.WithValue(isInlined))
                | ValueSome typeParams ->
                    StackList.three(
                        Function.Name.WithValue(name),
                        Function.IsInlined.WithValue(isInlined),
                        Function.TypeParams.WithValue(typeParams)
                    )

            SingleChildBuilder<FunctionNode, Expr>(
                Function.WidgetKey,
                Function.Value,
                AttributesBundle(
                    scalars,
                    ValueSome [| Function.Parameters.WithValue(parameters.Compile()) |],
                    ValueNone
                )
            )

        static member Function(name: string, parameters: WidgetBuilder<Pattern>) =
            Ast.BaseFunction(name, false, ValueNone, parameters)

        static member Function(name: string, typeParams: string list, parameters: WidgetBuilder<Pattern>) =
            Ast.BaseFunction(name, false, ValueSome typeParams, parameters)

        static member InlinedFunction(name: string, parameters: WidgetBuilder<Pattern>) =
            Ast.BaseFunction(name, true, ValueNone, parameters)

        static member InlinedFunction(name: string, typeParams: string list, parameters: WidgetBuilder<Pattern>) =
            Ast.BaseFunction(name, true, ValueSome typeParams, parameters)

[<Extension>]
type FunctionModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<FunctionNode>, xmlDocs: string list) =
        this.AddScalar(Function.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<FunctionNode>, attributes: string list) =
        this.AddScalar(Function.MultipleAttributes.WithValue(attributes))

    [<Extension>]
    static member inline toPrivate(this: WidgetBuilder<FunctionNode>) =
        this.AddScalar(Function.Accessibility.WithValue(AccessControl.Private))

    [<Extension>]
    static member inline toPublic(this: WidgetBuilder<FunctionNode>) =
        this.AddScalar(Function.Accessibility.WithValue(AccessControl.Public))

    [<Extension>]
    static member inline toInternal(this: WidgetBuilder<FunctionNode>) =
        this.AddScalar(Function.Accessibility.WithValue(AccessControl.Internal))

    [<Extension>]
    static member inline returnType(this: WidgetBuilder<FunctionNode>, returnType: Type) =
        this.AddScalar(Function.Return.WithValue(returnType))

[<Extension>]
type FunctionYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<FunctionNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let moduleDecl = ModuleDecl.TopLevelBinding node
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
