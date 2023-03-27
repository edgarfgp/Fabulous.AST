namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

module Let =
    let Name = Attributes.defineScalar "Name"
    let Value = Attributes.defineScalar "Value"
    let IsMutable = Attributes.defineScalar<bool> "IsMutable"

    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"

    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"

    let Accessibility = Attributes.defineScalar<AccessControl> "Accessibility"

    let WidgetKey =
        Widgets.register "Let" (fun widget ->
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
                | ValueSome values ->
                    MultipleAttributeListNode(
                        [ AttributeListNode(
                              SingleTextNode("[<", Range.Zero),
                              [ for v in values do
                                    AttributeNode(
                                        IdentListNode(
                                            [ IdentifierOrDot.Ident(SingleTextNode(v, Range.Zero)) ],
                                            Range.Zero
                                        ),
                                        None,
                                        None,
                                        Range.Zero
                                    ) ],
                              SingleTextNode(">]", Range.Zero),
                              Range.Zero
                          ) ],
                        Range.Zero
                    )
                    |> Some
                | ValueNone -> None

            let isMutable =
                Helpers.tryGetScalarValue widget IsMutable |> ValueOption.defaultValue false

            BindingNode(
                xmlDoc,
                multipleAttributes,
                MultipleTextsNode([ SingleTextNode("let", Range.Zero) ], Range.Zero),
                isMutable,
                None,
                accessControl,
                Choice1Of2(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode(name, Range.Zero)) ], Range.Zero)),
                None,
                List.Empty,
                None,
                SingleTextNode("=", Range.Zero),
                Expr.Constant(Constant.FromText(SingleTextNode(value, Range.Zero))),
                Range.Zero
            ))

[<AutoOpen>]
module LetBuilders =
    type Fabulous.AST.Ast with

        static member inline Let(name: string, value: string) =
            WidgetBuilder<BindingNode>(Let.WidgetKey, Let.Name.WithValue(name), Let.Value.WithValue(value))

[<Extension>]
type LetModifiers =
    [<Extension>]
    static member inline isMutable(this: WidgetBuilder<BindingNode>) =
        this.AddScalar(Let.IsMutable.WithValue(true))

    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<BindingNode>, xmlDocs: string list) =
        this.AddScalar(Let.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline attributes(this: WidgetBuilder<BindingNode>, attributes: string list) =
        this.AddScalar(Let.MultipleAttributes.WithValue(attributes))

    [<Extension>]
    static member inline accessibility(this: WidgetBuilder<BindingNode>, ?value: AccessControl) =
        match value with
        | Some value -> this.AddScalar(Let.Accessibility.WithValue(value))
        | None -> this.AddScalar(Let.Accessibility.WithValue(AccessControl.Public))

[<Extension>]
type LetYieldExtensions =
    [<Extension>]
    static member inline Yield(_: CollectionBuilder<'parent, ModuleDecl>, x: WidgetBuilder<BindingNode>) : Content =
        { Widgets = MutStackArray1.One(TopLevelBinding(x).Compile()) }
