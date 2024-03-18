namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ListCons =
    let LHSPattern = Attributes.defineScalar<StringOrWidget<Pattern>> "LHSPattern"

    let MiddlePattern = Attributes.defineScalar<SingleTextNode> "MHSPattern"

    let RHSPattern = Attributes.defineScalar<StringOrWidget<Pattern>> "RHSPattern"

    let WidgetKey =
        Widgets.register "ListCons" (fun widget ->
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

            Pattern.ListCons(PatLeftMiddleRight(lhs, Choice1Of2(middle), rhs, Range.Zero)))

[<AutoOpen>]
module ListConsBuilders =
    type Ast with

        static member ListConsPat(lhs: WidgetBuilder<Pattern>, rhs: WidgetBuilder<Pattern>) =
            WidgetBuilder<Pattern>(
                ListCons.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        ListCons.LHSPattern.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak lhs)),
                        ListCons.RHSPattern.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak rhs)),
                        ListCons.MiddlePattern.WithValue(SingleTextNode.doubleColon)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member ListConsPat(lhs: string, rhs: string) =
            WidgetBuilder<Pattern>(
                ListCons.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        ListCons.LHSPattern.WithValue(StringOrWidget.StringExpr(Unquoted lhs)),
                        ListCons.RHSPattern.WithValue(StringOrWidget.StringExpr(Unquoted rhs)),
                        ListCons.MiddlePattern.WithValue(SingleTextNode.doubleColon)
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member ListConsPat(lhs: WidgetBuilder<Pattern>, middle: string, rhs: WidgetBuilder<Pattern>) =
            WidgetBuilder<Pattern>(
                ListCons.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        ListCons.LHSPattern.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak lhs)),
                        ListCons.RHSPattern.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak rhs)),
                        ListCons.MiddlePattern.WithValue(SingleTextNode.Create(middle))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member ListConsPat(lhs: string, middle: string, rhs: string) =
            WidgetBuilder<Pattern>(
                ListCons.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        ListCons.LHSPattern.WithValue(StringOrWidget.StringExpr(Unquoted lhs)),
                        ListCons.RHSPattern.WithValue(StringOrWidget.StringExpr(Unquoted rhs)),
                        ListCons.MiddlePattern.WithValue(SingleTextNode.Create(middle))
                    ),
                    Array.empty,
                    Array.empty
                )
            )
