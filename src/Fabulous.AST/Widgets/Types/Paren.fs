namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
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
        static member Paren(value: WidgetBuilder<Type>) =
            WidgetBuilder<Type>(ParenType.WidgetKey, ParenType.Identifier.WithValue(value.Compile()))

        static member Paren(value: string) = Ast.Paren(Ast.LongIdent(value))
