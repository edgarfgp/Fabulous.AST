namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

type ValueNode(xmlDoc, multipleAttributes, isMutable, isInlined, accessControl, name, value) =
    inherit
        BindingNode(
            xmlDoc,
            multipleAttributes,
            MultipleTextsNode([ SingleTextNode("let", Range.Zero) ], Range.Zero),
            isMutable,
            (if isInlined then
                 Some(SingleTextNode("inline", Range.Zero))
             else
                 None),
            accessControl,
            Choice1Of2(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode(name, Range.Zero)) ], Range.Zero)),
            None,
            List.Empty,
            None,
            SingleTextNode("=", Range.Zero),
            Expr.Constant(Constant.FromText(SingleTextNode(value, Range.Zero))),
            Range.Zero
        )

module Value =
    let Name = Attributes.defineScalar "Name"
    let Value = Attributes.defineScalar "Value"
    let IsMutable = Attributes.defineScalar<bool> "IsMutable"

    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"
    let IsInlined = Attributes.defineScalar<bool> "IsInlined"

    let WidgetKey =
        Widgets.register "Value" (fun widget ->
            let name = Helpers.getScalarValue widget Name
            let value = Helpers.getScalarValue widget Value

            let accessControl =
                Helpers.tryGetScalarValue widget Accessibility
                |> ValueOption.defaultValue AccessControl.Public

            let accessControl =
                match accessControl with
                | Public -> None
                | Private -> Some(SingleTextNode("private", Range.Zero))
                | Internal -> Some(SingleTextNode("internal", Range.Zero))

            let lines = Helpers.tryGetScalarValue widget XmlDocs

            let xmlDoc =
                match lines with
                | ValueSome value -> Some(XmlDocNode((value |> Array.ofList), Range.Zero))
                | ValueNone -> None

            let attributes = Helpers.tryGetScalarValue widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | ValueSome values -> TypeHelpers.createAttributes values |> Some
                | ValueNone -> None

            let isMutable =
                Helpers.tryGetScalarValue widget IsMutable |> ValueOption.defaultValue false

            let isInlined =
                Helpers.tryGetScalarValue widget IsInlined |> ValueOption.defaultValue false

            ValueNode(xmlDoc, multipleAttributes, isMutable, isInlined, accessControl, name, value))

[<AutoOpen>]
module ValueBuilders =
    type Fabulous.AST.Ast with

        static member inline Value(name: string, value: string) =
            WidgetBuilder<ValueNode>(Value.WidgetKey, Value.Name.WithValue(name), Value.Value.WithValue(value))

[<Extension>]
type ValueModifiers =
    [<Extension>]
    static member inline isMutable(this: WidgetBuilder<ValueNode>) =
        this.AddScalar(Value.IsMutable.WithValue(true))

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
    static member inline isInlined(this: WidgetBuilder<ValueNode>) =
        this.AddScalar(Value.IsInlined.WithValue(true))

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
