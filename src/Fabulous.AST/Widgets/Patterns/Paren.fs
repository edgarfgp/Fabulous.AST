namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ParenPat =

    let Pat = Attributes.defineWidget "Pat"

    let WidgetKey =
        Widgets.register "Paren" (fun widget ->
            let pat = Helpers.getNodeFromWidget widget Pat

            Pattern.Paren(
                PatParenNode(SingleTextNode.leftParenthesis, pat, SingleTextNode.rightParenthesis, Range.Zero)
            ))

[<AutoOpen>]
module ParenPatBuilders =
    type Ast with

        static member ParenPat(pat: WidgetBuilder<Pattern>) =
            WidgetBuilder<Pattern>(
                ParenPat.WidgetKey,
                AttributesBundle(StackList.empty(), ValueSome [| ParenPat.Pat.WithValue(pat.Compile()) |], ValueNone)
            )

        static member ParenPat(pat: string) =
            WidgetBuilder<Pattern>(
                ParenPat.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    ValueSome [| ParenPat.Pat.WithValue(Ast.NamedPat(pat).Compile()) |],
                    ValueNone
                )
            )
