namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module EnumCase =

    let Name = Attributes.defineWidget "Name"

    let Value = Attributes.defineWidget "Value"

    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"

    let WidgetKey =
        Widgets.register "EnumCase" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name
            let value = Helpers.getNodeFromWidget<SingleTextNode> widget Value
            let attributes = Helpers.tryGetScalarValue widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | ValueSome values -> MultipleAttributeListNode.Create values |> Some
                | ValueNone -> None

            EnumCaseNode(
                None,
                None,
                multipleAttributes,
                name,
                SingleTextNode("=", Range.Zero),
                Expr.Constant(Constant.FromText(value)),
                Range.Zero
            ))

[<AutoOpen>]
module EnumCaseBuilders =
    type Ast with

        static member inline EnumCase(name: WidgetBuilder<#SingleTextNode>, value: WidgetBuilder<#SingleTextNode>) =
            WidgetBuilder<EnumCaseNode>(
                EnumCase.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome
                        [| EnumCase.Name.WithValue(name.Compile())
                           EnumCase.Value.WithValue(value.Compile()) |],
                    ValueNone
                )
            )

        static member inline EnumCase(node: SingleTextNode, value: SingleTextNode) =
            Ast.EnumCase(Ast.EscapeHatch(node), Ast.EscapeHatch(value))

        static member inline EnumCase(name: string, value: string) =
            Ast.EnumCase(SingleTextNode(name, Range.Zero), SingleTextNode(value, Range.Zero))

[<Extension>]
type EnumCaseModifiers =
    [<Extension>]
    static member inline attributes(this: WidgetBuilder<EnumCaseNode>, attributes: string list) =
        this.AddScalar(EnumCase.MultipleAttributes.WithValue(attributes))
