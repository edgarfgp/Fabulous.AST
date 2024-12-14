namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ParenPat =

    let Pat = Attributes.defineWidget "Pat"

    let WidgetKey =
        Widgets.register "Paren" (fun widget ->
            let pat = Widgets.tryGetNodeFromWidget widget Pat

            match pat with
            | ValueNone ->
                Pattern.Unit(UnitNode(SingleTextNode.leftParenthesis, SingleTextNode.rightParenthesis, Range.Zero))
            | ValueSome pat ->
                Pattern.Paren(
                    PatParenNode(SingleTextNode.leftParenthesis, pat, SingleTextNode.rightParenthesis, Range.Zero)
                ))

[<AutoOpen>]
module ParenPatBuilders =
    type Ast with

        static member ParenPat() =
            WidgetBuilder<Pattern>(ParenPat.WidgetKey)

        static member ParenPat(pat: WidgetBuilder<Pattern>) =
            WidgetBuilder<Pattern>(ParenPat.WidgetKey, ParenPat.Pat.WithValue(pat.Compile()))

        static member ParenPat(pat: WidgetBuilder<Constant>) = Ast.ParenPat(Ast.ConstantPat(pat))

        static member ParenPat(pat: string) = Ast.ParenPat(Ast.Constant(pat))
