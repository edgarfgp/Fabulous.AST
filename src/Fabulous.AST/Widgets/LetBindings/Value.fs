namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

type ValueNode(xmlDoc, multipleAttributes, isMutable, isInlined, accessControl, name, value) =
    inherit
        BindingNode(
            xmlDoc,
            multipleAttributes,
            MultipleTextsNode([ SingleTextNode.``let`` ], Range.Zero),
            isMutable,
            (if isInlined then Some(SingleTextNode.``inline``) else None),
            accessControl,
            Choice1Of2(IdentListNode([ IdentifierOrDot.Ident(name) ], Range.Zero)),
            None,
            List.Empty,
            None,
            SingleTextNode.equals,
            value,
            Range.Zero
        )

module Value =
    let Name = Attributes.defineWidget "Name"
    let Value = Attributes.defineWidget "Value"
    let IsMutable = Attributes.defineScalar<bool> "IsMutable"
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"
    let IsInlined = Attributes.defineScalar<bool> "IsInlined"
    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"
    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"
    let ValueExpr = Attributes.defineScalar<Expr> "Expression"

    let WidgetKey =
        Widgets.register "Value" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name
            let value = Helpers.getNodeFromWidget<SingleTextNode> widget Value

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

            let xmlDoc =
                match lines with
                | ValueSome value -> Some(XmlDocNode((value |> Array.ofList), Range.Zero))
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

            ValueNode(
                xmlDoc,
                multipleAttributes,
                isMutable,
                isInlined,
                accessControl,
                name,
                Expr.Constant(Constant.FromText(value))
            ))

    let WidgetExprKey =
        Widgets.register "ValueExpr" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name
            let expression = Helpers.getScalarValue widget ValueExpr

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

            let xmlDoc =
                match lines with
                | ValueSome value -> Some(XmlDocNode((value |> Array.ofList), Range.Zero))
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

            ValueNode(xmlDoc, multipleAttributes, isMutable, isInlined, accessControl, name, expression))

[<AutoOpen>]
module ValueBuilders =
    type Ast with
        static member inline Value(name: WidgetBuilder<SingleTextNode>, value: WidgetBuilder<SingleTextNode>) =
            WidgetBuilder<ValueNode>(
                Value.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome
                        [| Value.Name.WithValue(name.Compile())
                           Value.Value.WithValue(value.Compile()) |],
                    ValueNone
                )
            )

        static member inline Value(name: SingleTextNode, value: SingleTextNode) =
            Ast.Value(Ast.EscapeHatch(name), Ast.EscapeHatch(value))

        static member inline Value(name: string, value: string) =
            Ast.Value(SingleTextNode.Create(name), SingleTextNode.Create(value))

        static member inline Value(name: WidgetBuilder<SingleTextNode>, value: Expr) =
            WidgetBuilder<ValueNode>(
                Value.WidgetExprKey,
                AttributesBundle(
                    StackList.one(Value.ValueExpr.WithValue(value)),
                    ValueSome [| Value.Name.WithValue(name.Compile()) |],
                    ValueNone
                )
            )

        static member inline Value(name: SingleTextNode, value: Expr) = Ast.Value(Ast.EscapeHatch(name), value)

        static member inline Value(name: string, value: Expr) =
            Ast.Value(SingleTextNode.Create(name), value)

        static member inline MutableValue(name: WidgetBuilder<SingleTextNode>, value: WidgetBuilder<SingleTextNode>) =
            WidgetBuilder<ValueNode>(
                Value.WidgetKey,
                AttributesBundle(
                    StackList.one(Value.IsMutable.WithValue(true)),
                    ValueSome
                        [| Value.Name.WithValue(name.Compile())
                           Value.Value.WithValue(value.Compile()) |],
                    ValueNone
                )
            )

        static member inline MutableValue(name: SingleTextNode, value: SingleTextNode) =
            Ast.MutableValue(Ast.EscapeHatch(name), Ast.EscapeHatch(value))

        static member inline MutableValue(name: string, value: string) =
            Ast.MutableValue(SingleTextNode.Create(name), SingleTextNode.Create(value))

        static member inline MutableValue(name: WidgetBuilder<SingleTextNode>, value: Expr) =
            WidgetBuilder<ValueNode>(
                Value.WidgetExprKey,
                AttributesBundle(
                    StackList.two(Value.IsMutable.WithValue(true), Value.ValueExpr.WithValue(value)),
                    ValueSome [| Value.Name.WithValue(name.Compile()) |],
                    ValueNone
                )
            )

        static member inline MutableValue(name: SingleTextNode, value: Expr) =
            Ast.MutableValue(Ast.EscapeHatch(name), value)

        static member inline MutableValue(name: string, value: Expr) =
            Ast.MutableValue(SingleTextNode.Create(name), value)

        static member inline InlinedValue(name: WidgetBuilder<SingleTextNode>, value: WidgetBuilder<SingleTextNode>) =
            WidgetBuilder<ValueNode>(
                Value.WidgetKey,
                AttributesBundle(
                    StackList.one(Value.IsInlined.WithValue(true)),
                    ValueSome
                        [| Value.Name.WithValue(name.Compile())
                           Value.Value.WithValue(value.Compile()) |],
                    ValueNone
                )
            )

        static member inline InlinedValue(name: SingleTextNode, value: SingleTextNode) =
            Ast.InlinedValue(Ast.EscapeHatch(name), Ast.EscapeHatch(value))

        static member inline InlinedValue(name: string, value: string) =
            Ast.InlinedValue(SingleTextNode.Create(name), SingleTextNode.Create(value))

        static member inline InlinedValue(name: WidgetBuilder<SingleTextNode>, value: Expr) =
            WidgetBuilder<ValueNode>(
                Value.WidgetExprKey,
                AttributesBundle(
                    StackList.two(Value.IsInlined.WithValue(true), Value.ValueExpr.WithValue(value)),
                    ValueSome [| Value.Name.WithValue(name.Compile()) |],
                    ValueNone
                )
            )

        static member inline InlinedValue(name: SingleTextNode, value: Expr) =
            Ast.InlinedValue(Ast.EscapeHatch(name), value)

        static member inline InlinedValue(name: string, value: Expr) =
            Ast.InlinedValue(SingleTextNode.Create(name), value)

[<Extension>]
type ValueModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<ValueNode>, xmlDocs: string list) =
        this.AddScalar(Value.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<ValueNode>, attributes: string list) =
        this.AddScalar(Value.MultipleAttributes.WithValue(attributes))

    [<Extension>]
    static member inline accessibility(this: WidgetBuilder<ValueNode>, ?value: AccessControl) =
        match value with
        | Some value -> this.AddScalar(Value.Accessibility.WithValue(value))
        | None -> this.AddScalar(Value.Accessibility.WithValue(AccessControl.Public))

[<Extension>]
type ValueYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<ValueNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let moduleDecl = ModuleDecl.TopLevelBinding node
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
