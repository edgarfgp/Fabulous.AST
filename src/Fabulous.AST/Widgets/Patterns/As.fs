namespace Fabulous.AST

open System
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module As =
    let LHSPattern = Attributes.defineScalar<StringOrWidget<Pattern>> "LHSPattern"

    let MiddlePattern = Attributes.defineScalar<SingleTextNode> "MHSPattern"

    let RHSPattern = Attributes.defineScalar<StringOrWidget<Pattern>> "RHSPattern"

    let WidgetKey =
        Widgets.register "As" (fun widget ->
            let lhs = Widgets.getScalarValue widget LHSPattern

            let lhs =
                match lhs with
                | StringOrWidget.StringExpr lhs ->
                    let lhs = StringParsing.normalizeIdentifierBackticks lhs
                    Pattern.Named(PatNamedNode(None, SingleTextNode.Create(lhs), Range.Zero))
                | StringOrWidget.WidgetExpr pattern -> pattern

            let middle = Widgets.getScalarValue widget MiddlePattern
            let rhs = Widgets.getScalarValue widget RHSPattern

            let rhs =
                match rhs with
                | StringOrWidget.StringExpr rhs ->
                    let rhs = StringParsing.normalizeIdentifierBackticks rhs
                    Pattern.Named(PatNamedNode(None, SingleTextNode.Create(rhs), Range.Zero))
                | StringOrWidget.WidgetExpr pattern -> pattern

            Pattern.As(PatLeftMiddleRight(lhs, Choice1Of2(middle), rhs, Range.Zero)))

[<AutoOpen>]
module AsBuilders =
    type Ast with

        static member AsPat(lhs: WidgetBuilder<Pattern>, rhs: WidgetBuilder<Pattern>) =
            WidgetBuilder<Pattern>(
                As.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        As.LHSPattern.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak lhs)),
                        As.MiddlePattern.WithValue(SingleTextNode.``as``),
                        As.RHSPattern.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak rhs))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member AsPat(lhs: string, rhs: string) =
            WidgetBuilder<Pattern>(
                As.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        As.LHSPattern.WithValue(StringOrWidget.StringExpr(Unquoted lhs)),
                        As.MiddlePattern.WithValue(SingleTextNode.``as``),
                        As.RHSPattern.WithValue(StringOrWidget.StringExpr(Unquoted rhs))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member AsPat(lhs: WidgetBuilder<Pattern>, middle: string, rhs: WidgetBuilder<Pattern>) =
            WidgetBuilder<Pattern>(
                As.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        As.LHSPattern.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak lhs)),
                        As.MiddlePattern.WithValue(SingleTextNode.Create(middle)),
                        As.RHSPattern.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak rhs))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member AsPat(lhs: string, middle: string, rhs: string) =
            WidgetBuilder<Pattern>(
                As.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        As.LHSPattern.WithValue(StringOrWidget.StringExpr(Unquoted lhs)),
                        As.MiddlePattern.WithValue(SingleTextNode.Create(middle)),
                        As.RHSPattern.WithValue(StringOrWidget.StringExpr(Unquoted rhs))
                    ),
                    Array.empty,
                    Array.empty
                )
            )
