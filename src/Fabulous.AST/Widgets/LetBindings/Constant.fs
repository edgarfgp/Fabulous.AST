namespace Fabulous.AST

open System.Runtime.CompilerServices
open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak

open type Fabulous.AST.Ast

type ConstantNode(xmlDoc: XmlDocNode option, accessControl: SingleTextNode option, name: string, value: string) =
    inherit
        BindingNode(
            xmlDoc,
            Some(
                MultipleAttributeListNode(
                    [ AttributeListNode(
                          SingleTextNode("[<", Range.Zero),
                          [ AttributeNode(
                                IdentListNode(
                                    [ IdentifierOrDot.Ident(SingleTextNode("Literal", Range.Zero)) ],
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
            ),
            MultipleTextsNode([ SingleTextNode("let", Range.Zero) ], Range.Zero),
            false,
            None,
            accessControl,
            Choice1Of2(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode(name, Range.Zero)) ], Range.Zero)),
            None,
            List.Empty,
            None,
            SingleTextNode("=", Range.Zero),
            Expr.Constant(Constant.FromText(SingleTextNode(value, Range.Zero))),
            Range.Zero
        )

module Constant =
    let Name = Attributes.defineScalar "Name"
    let Value = Attributes.defineScalar "Value"
    let XmlDocs = Attributes.defineScalar<string list> "XmlDoc"
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

            ConstantNode(xmlDoc, accessControl, name, value))

[<AutoOpen>]
module ConstantBuilders =
    type Fabulous.AST.Ast with

        static member inline Constant(name: string, value: string) =
            WidgetBuilder<ConstantNode>(
                Constant.WidgetKey,
                Constant.Name.WithValue(name),
                Constant.Value.WithValue(value)
            )

[<Extension>]
type ConstantModifiers =
    [<Extension>]
    static member inline xmlDocs(this: WidgetBuilder<ConstantNode>, xmlDocs: string list) =
        this.AddScalar(Constant.XmlDocs.WithValue(xmlDocs))

    [<Extension>]
    static member inline accessibility(this: WidgetBuilder<ConstantNode>, ?value: AccessControl) =
        match value with
        | Some value -> this.AddScalar(Constant.Accessibility.WithValue(value))
        | None -> this.AddScalar(Constant.Accessibility.WithValue(AccessControl.Public))

[<Extension>]
type ConstantYieldExtensions =
    [<Extension>]
    static member inline Yield
        (
            _: CollectionBuilder<'parent, ModuleDecl>,
            x: WidgetBuilder<ConstantNode>
        ) : CollectionContent =
        let node = Tree.compile x
        let moduleDecl = ModuleDecl.TopLevelBinding node
        let widget = Ast.EscapeHatch(moduleDecl).Compile()
        { Widgets = MutStackArray1.One(widget) }
