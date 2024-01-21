namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text
open type Fabulous.AST.Ast


type FunctionNode
    (name, parameters, bodyExpr, isInlined, accessControl, multipleAttributes, xmlDoc, returnType, typeParams) =
    inherit
        BindingNode(
            xmlDoc,
            multipleAttributes,
            MultipleTextsNode([ SingleTextNode.``let`` ], Range.Zero),
            false,
            (if isInlined then Some(SingleTextNode.``inline``) else None),
            accessControl,
            Choice1Of2(IdentListNode.Create([ IdentifierOrDot.Ident(name) ])),
            typeParams,
            parameters,
            returnType,
            SingleTextNode.equals,
            bodyExpr,
            Range.Zero
        )

module Function =
    let Name = Attributes.defineWidget "Name"
    let Parameters = Attributes.defineScalar "Value"
    let BodyExpr = Attributes.defineWidget "BodyExpr"
    let IsInlined = Attributes.defineScalar "IsInlined"
    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"
    let Return = Attributes.defineScalar<Type> "Return"
    let TypeParams = Attributes.defineScalar<TyparDecls> "TypeParams"

    let WidgetKey =
        Widgets.register "Function" (fun widget ->
            let name = Helpers.getNodeFromWidget widget Name
            let parameters = Helpers.getScalarValue widget Parameters
            let bodyExpr = Helpers.getNodeFromWidget<Expr> widget BodyExpr

            let isInlined =
                Helpers.tryGetScalarValue widget IsInlined |> ValueOption.defaultValue false

            let accessControl =
                Helpers.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Unknown

            let accessControl =
                match accessControl with
                | Public -> Some(SingleTextNode.``public``)
                | Private -> Some(SingleTextNode.``private``)
                | Internal -> Some(SingleTextNode.``internal``)
                | Unknown -> None

            let attributes = Helpers.tryGetScalarValue widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | ValueSome values -> MultipleAttributeListNode.Create values |> Some
                | ValueNone -> None

            let lines = Helpers.tryGetScalarValue widget XmlDocs

            let xmlDocs =
                match lines with
                | ValueSome values ->
                    let xmlDocNode = XmlDocNode.Create(values)
                    Some xmlDocNode
                | ValueNone -> None

            let returnType = Helpers.tryGetScalarValue widget Return

            let returnType =
                match returnType with
                | ValueSome returnType -> Some(BindingReturnInfoNode(SingleTextNode.colon, returnType, Range.Zero))
                | ValueNone -> None

            let typeParams = Helpers.tryGetScalarValue widget TypeParams

            let typeParams =
                match typeParams with
                | ValueSome typeParams -> Some(typeParams)
                | ValueNone -> None

            FunctionNode(
                name,
                parameters,
                bodyExpr,
                isInlined,
                accessControl,
                multipleAttributes,
                xmlDocs,
                returnType,
                typeParams
            ))

[<AutoOpen>]
module FunctionBuilders =
    type Ast with

        static member inline Function(name: WidgetBuilder<SingleTextNode>, parameters: Pattern list) =
            SingleChildBuilder<FunctionNode, Expr>(
                Function.WidgetKey,
                Function.BodyExpr,
                AttributesBundle(
                    StackList.one(Function.Parameters.WithValue(parameters)),
                    ValueSome [| (Function.Name.WithValue(name.Compile())) |],
                    ValueNone
                )
            )

        static member inline Function(name: SingleTextNode, parameters: Pattern list) =
            Ast.Function(Ast.EscapeHatch(name), parameters)

        static member inline Function(name: string, parameters: Pattern list) =
            Ast.Function(SingleTextNode.Create(name), parameters)


        static member inline InlinedFunction(name: WidgetBuilder<SingleTextNode>, parameters: Pattern list) =
            SingleChildBuilder<FunctionNode, Expr>(
                Function.WidgetKey,
                Function.BodyExpr,
                AttributesBundle(
                    StackList.two(Function.Parameters.WithValue(parameters), Function.IsInlined.WithValue(true)),
                    ValueSome [| (Function.Name.WithValue(name.Compile())) |],
                    ValueNone
                )
            )

        static member inline InlinedFunction(name: SingleTextNode, parameters: Pattern list) =
            Ast.InlinedFunction(Ast.EscapeHatch(name), parameters)

        static member inline InlinedFunction(name: string, parameters: Pattern list) =
            Ast.InlinedFunction(SingleTextNode.Create(name), parameters)

[<Extension>]
type FunctionModifiers =
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
    static member inline attributes(this: WidgetBuilder<FunctionNode>, attributes: string list) =
        this.AddScalar(Function.MultipleAttributes.WithValue(attributes))

    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<FunctionNode>, xmlDocs: string list) =
        this.AddScalar(Function.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline returnType(this: WidgetBuilder<FunctionNode>, returnType: Type) =
        this.AddScalar(Function.Return.WithValue(returnType))

    [<Extension>]
    static member inline typeParams(this: WidgetBuilder<FunctionNode>, typeParams: TyparDecls) =
        this.AddScalar(Function.TypeParams.WithValue(typeParams))

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
