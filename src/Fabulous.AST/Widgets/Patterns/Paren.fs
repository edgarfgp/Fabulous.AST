namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ParenPat =

    let Pat = Attributes.defineScalar<StringOrWidget<Pattern>> "Pat"

    let WidgetKey =
        Widgets.register "Paren" (fun widget ->
            let pat = Widgets.getScalarValue widget Pat

            let pat =
                match pat with
                | StringOrWidget.StringExpr value ->
                    let value = StringParsing.normalizeIdentifierBackticks value
                    Pattern.Named(PatNamedNode(None, SingleTextNode.Create(value), Range.Zero))
                | StringOrWidget.WidgetExpr pattern -> pattern

            Pattern.Paren(
                PatParenNode(SingleTextNode.leftParenthesis, pat, SingleTextNode.rightParenthesis, Range.Zero)
            ))

[<AutoOpen>]
module ParenPatBuilders =
    type Ast with

        static member ParenPat(pat: WidgetBuilder<Pattern>) =
            WidgetBuilder<Pattern>(
                ParenPat.WidgetKey,
                AttributesBundle(
                    StackList.one(ParenPat.Pat.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak pat))),
                    Array.empty,
                    Array.empty
                )
            )

        static member ParenPat(pat: string) =
            WidgetBuilder<Pattern>(
                ParenPat.WidgetKey,
                AttributesBundle(
                    StackList.one(ParenPat.Pat.WithValue(StringOrWidget.StringExpr(Unquoted(pat)))),
                    Array.empty,
                    Array.empty
                )
            )
