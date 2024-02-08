namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module EnumCase =

    let Name = Attributes.defineScalar<string> "Name"

    let Value = Attributes.defineWidget "Value"

    let MultipleAttributes = Attributes.defineScalar<string list> "MultipleAttributes"

    let WidgetKey =
        Widgets.register "EnumCase" (fun widget ->
            let name = Helpers.getScalarValue widget Name
            let value = Helpers.getNodeFromWidget<Expr> widget Value
            let attributes = Helpers.tryGetScalarValue widget MultipleAttributes

            let multipleAttributes =
                match attributes with
                | ValueSome values -> MultipleAttributeListNode.Create values |> Some
                | ValueNone -> None

            EnumCaseNode(
                None,
                None,
                multipleAttributes,
                SingleTextNode.Create(name),
                SingleTextNode.equals,
                value,
                Range.Zero
            ))

[<AutoOpen>]
module EnumCaseBuilders =
    type Ast with

        static member EnumCase(name: string, value: WidgetBuilder<Expr>) =
            WidgetBuilder<EnumCaseNode>(
                EnumCase.WidgetKey,
                AttributesBundle(
                    StackList.one(EnumCase.Name.WithValue(name)),
                    ValueSome [| EnumCase.Value.WithValue(value.Compile()) |],
                    ValueNone
                )
            )

        static member EnumCase(name: string, value: string) =
            Ast.EnumCase(name, Ast.ConstantExpr(value))

[<Extension>]
type EnumCaseModifiers =
    [<Extension>]
    static member inline attributes(this: WidgetBuilder<EnumCaseNode>, attributes: string list) =
        this.AddScalar(EnumCase.MultipleAttributes.WithValue(attributes))
