namespace Fabulous.AST

open FSharp.Compiler.Text
open Fabulous.AST.StackAllocatedCollections
open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList

module EnumCase =

    let Name = Attributes.defineWidget "SingleTextNode"

    let Value = Attributes.defineWidget "SingleTextNode"

    let WidgetKey =
        Widgets.register "EnumCase" (fun widget ->
            let name = Helpers.getNodeFromWidget<SingleTextNode> widget Name
            let value = Helpers.getNodeFromWidget<SingleTextNode> widget Value

            EnumCaseNode(
                None,
                None,
                None,
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
                           EnumCase.Value.WithValue(value.Compile())

                           |],
                    ValueNone
                )
            )

        static member inline EnumCase(node: SingleTextNode, value: SingleTextNode) =
            Ast.EnumCase(Ast.EscapeHatch(node), Ast.EscapeHatch(value))

        static member inline EnumCase(name: string, value: string) =
            Ast.EnumCase(SingleTextNode(name, Range.Zero), SingleTextNode(value, Range.Zero))
