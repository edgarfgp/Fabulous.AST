namespace Fabulous.AST

open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

module ParenType =
    let Identifier = Attributes.defineWidget "Identifier"

    let WidgetKey =
        Widgets.register "ParenType" (fun widget ->
            let identifier = Widgets.getNodeFromWidget widget Identifier

            Type.Paren(
                TypeParenNode(SingleTextNode.leftParenthesis, identifier, SingleTextNode.rightParenthesis, Range.Zero)
            ))

[<AutoOpen>]
module ParenTypeBuilders =
    type Ast with
        static member ParenType(value: WidgetBuilder<Type>) =
            WidgetBuilder<Type>(
                ParenType.WidgetKey,
                AttributesBundle(StackList.empty(), [| ParenType.Identifier.WithValue(value.Compile()) |], Array.empty)
            )

        static member ParenType(value: string) = Ast.ParenType(Ast.LongIdent(value))
