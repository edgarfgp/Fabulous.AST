namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ForEach =
    let Pat = Attributes.defineScalar<StringOrWidget<Pattern>> "Pat"

    let EnumExpr = Attributes.defineScalar<StringOrWidget<Expr>> "EnumExpr"

    let IsArrow = Attributes.defineScalar<bool> "IsArrow"

    let BodyExpr = Attributes.defineScalar<StringOrWidget<Expr>> "BodyExpr"

    let WidgetKey =
        Widgets.register "ForEach" (fun widget ->
            let pat = Widgets.getScalarValue widget Pat

            let pat =
                match pat with
                | StringOrWidget.StringExpr value ->
                    let value = StringParsing.normalizeIdentifierQuotes value
                    Pattern.Named(PatNamedNode(None, SingleTextNode.Create(value), Range.Zero))
                | StringOrWidget.WidgetExpr pat -> pat

            let enumExpr = Widgets.getScalarValue widget EnumExpr

            let enumExpr =
                match enumExpr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value)))
                    )
                | StringOrWidget.WidgetExpr expr -> expr

            let isArrow =
                Widgets.tryGetScalarValue widget IsArrow |> ValueOption.defaultValue false

            let bodyExpr = Widgets.getScalarValue widget BodyExpr

            let bodyExpr =
                match bodyExpr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value)))
                    )
                | StringOrWidget.WidgetExpr expr -> expr

            Expr.ForEach(ExprForEachNode(SingleTextNode.``for``, pat, enumExpr, isArrow, bodyExpr, Range.Zero)))

[<AutoOpen>]
module ForEachBuilders =
    type Ast with

        static member ForEachExpr
            (pattern: WidgetBuilder<Pattern>, enumExpr: WidgetBuilder<Expr>, bodyExpr: WidgetBuilder<Expr>)
            =
            WidgetBuilder<Expr>(
                ForEach.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        ForEach.Pat.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak pattern)),
                        ForEach.EnumExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak enumExpr)),
                        ForEach.BodyExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak bodyExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member ForEachExpr(pattern: string, enumExpr: string, bodyExpr: string) =
            WidgetBuilder<Expr>(
                ForEach.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        ForEach.Pat.WithValue(StringOrWidget.StringExpr(Unquoted pattern)),
                        ForEach.EnumExpr.WithValue(StringOrWidget.StringExpr(Unquoted enumExpr)),
                        ForEach.BodyExpr.WithValue(StringOrWidget.StringExpr(Unquoted bodyExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member ForEachExpr(pattern: string, enumExpr: WidgetBuilder<Expr>, bodyExpr: string) =
            WidgetBuilder<Expr>(
                ForEach.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        ForEach.Pat.WithValue(StringOrWidget.StringExpr(Unquoted pattern)),
                        ForEach.EnumExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak enumExpr)),
                        ForEach.BodyExpr.WithValue(StringOrWidget.StringExpr(Unquoted bodyExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member ForEachExpr(pattern: string, enumExpr: string, bodyExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                ForEach.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        ForEach.Pat.WithValue(StringOrWidget.StringExpr(Unquoted pattern)),
                        ForEach.EnumExpr.WithValue(StringOrWidget.StringExpr(Unquoted enumExpr)),
                        ForEach.BodyExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak bodyExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member ForEachExpr(pattern: string, enumExpr: WidgetBuilder<Expr>, bodyExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                ForEach.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        ForEach.Pat.WithValue(StringOrWidget.StringExpr(Unquoted pattern)),
                        ForEach.EnumExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak enumExpr)),
                        ForEach.BodyExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak bodyExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member ForEachExpr(pattern: WidgetBuilder<Pattern>, enumExpr: string, bodyExpr: string) =
            WidgetBuilder<Expr>(
                ForEach.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        ForEach.Pat.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak pattern)),
                        ForEach.EnumExpr.WithValue(StringOrWidget.StringExpr(Unquoted enumExpr)),
                        ForEach.BodyExpr.WithValue(StringOrWidget.StringExpr(Unquoted bodyExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member ForEachExpr(pattern: WidgetBuilder<Pattern>, enumExpr: WidgetBuilder<Expr>, bodyExpr: string) =
            WidgetBuilder<Expr>(
                ForEach.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        ForEach.Pat.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak pattern)),
                        ForEach.EnumExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak enumExpr)),
                        ForEach.BodyExpr.WithValue(StringOrWidget.StringExpr(Unquoted bodyExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

[<Extension>]
type ForEachExprModifiers =
    [<Extension>]
    static member useArrow(this: WidgetBuilder<Expr>) =
        this.AddScalar(ForEach.IsArrow.WithValue(true))
