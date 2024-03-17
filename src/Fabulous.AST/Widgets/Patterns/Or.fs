namespace Fabulous.AST

open System
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Or =
    let LHSPattern = Attributes.defineScalar<StringOrWidget<Pattern>> "LHSPattern"

    let MHSPattern = Attributes.defineScalar<SingleTextNode> "MHSPattern"

    let RHSPattern = Attributes.defineScalar<StringOrWidget<Pattern>> "RHSPattern"

    let WidgetKey =
        Widgets.register "Or" (fun widget ->
            let lhs = Widgets.getScalarValue widget LHSPattern

            let lhs =
                match lhs with
                | StringOrWidget.StringExpr lhs ->
                    let lhs = StringParsing.normalizeIdentifierBackticks lhs
                    Pattern.Named(PatNamedNode(None, SingleTextNode.Create(lhs), Range.Zero))
                | StringOrWidget.WidgetExpr pattern -> pattern

            let middle = Widgets.getScalarValue widget MHSPattern

            let rhs = Widgets.getScalarValue widget RHSPattern

            let rhs =
                match rhs with
                | StringOrWidget.StringExpr rhs ->
                    let rhs = StringParsing.normalizeIdentifierBackticks rhs
                    Pattern.Named(PatNamedNode(None, SingleTextNode.Create(rhs), Range.Zero))
                | StringOrWidget.WidgetExpr pattern -> pattern

            Pattern.Or(PatLeftMiddleRight(lhs, Choice1Of2(middle), rhs, Range.Zero)))

[<AutoOpen>]
module OrBuilders =
    type Ast with

        static member OrPat(lhs: WidgetBuilder<Pattern>, rhs: WidgetBuilder<Pattern>) =
            WidgetBuilder<Pattern>(
                Or.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        Or.LHSPattern.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak lhs)),
                        Or.MHSPattern.WithValue(SingleTextNode.bar),
                        Or.RHSPattern.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak rhs))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member OrPat(lhs: string, rhs: string) =
            WidgetBuilder<Pattern>(
                Or.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        Or.LHSPattern.WithValue(StringOrWidget.StringExpr(Unquoted(lhs))),
                        Or.MHSPattern.WithValue(SingleTextNode.bar),
                        Or.RHSPattern.WithValue(StringOrWidget.StringExpr(Unquoted(rhs)))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member OrPat(lhs: WidgetBuilder<Pattern>, middle: string, rhs: WidgetBuilder<Pattern>) =
            WidgetBuilder<Pattern>(
                Or.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        Or.LHSPattern.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak lhs)),
                        Or.MHSPattern.WithValue(SingleTextNode.Create(middle)),
                        Or.RHSPattern.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak rhs))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member OrPat(lhs: StringVariant, middle: string, rhs: string) =
            WidgetBuilder<Pattern>(
                Or.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        Or.LHSPattern.WithValue(StringOrWidget.StringExpr(lhs)),
                        Or.MHSPattern.WithValue(SingleTextNode.Create(middle)),
                        Or.RHSPattern.WithValue(StringOrWidget.StringExpr(Unquoted(rhs)))
                    ),
                    Array.empty,
                    Array.empty
                )
            )
