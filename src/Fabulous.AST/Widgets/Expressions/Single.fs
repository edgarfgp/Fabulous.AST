namespace Fabulous.AST

open System
open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Single =
    let Value = Attributes.defineScalar<StringOrWidget<Expr>> "Value"

    let SupportsStroustrup = Attributes.defineScalar<bool> "SupportsStroustrup"

    let AddSpace = Attributes.defineScalar<bool> "AddSpace"

    let Leading = Attributes.defineScalar<string> "Leading"

    let WidgetKey =
        Widgets.register "Single" (fun widget ->
            let expr = Widgets.getScalarValue widget Value

            let expr =
                match expr with
                | StringOrWidget.StringExpr value ->
                    Expr.Constant(
                        Constant.FromText(SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value)))
                    )
                | StringOrWidget.WidgetExpr expr -> expr

            let supportsStroustrup = Widgets.getScalarValue widget SupportsStroustrup
            let addSpace = Widgets.getScalarValue widget AddSpace
            let leading = Widgets.getScalarValue widget Leading
            Expr.Single(ExprSingleNode(SingleTextNode.Create(leading), addSpace, supportsStroustrup, expr, Range.Zero)))

[<AutoOpen>]
module SingleBuilders =
    type Ast with

        static member SingleExpr(leading: string, value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                Single.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        Single.Leading.WithValue(leading),
                        Single.Value.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak value))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member SingleExpr(leading: string, value: StringVariant) =
            WidgetBuilder<Expr>(
                Single.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        Single.Leading.WithValue(leading),
                        Single.Value.WithValue(StringOrWidget.StringExpr(value))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

type SingleExprModifiers =
    [<Extension>]
    static member inline addSpace(this: WidgetBuilder<Expr>, value: bool) =
        this.AddScalar(Single.AddSpace.WithValue(value))

    [<Extension>]
    static member inline supportsStroustrup(this: WidgetBuilder<Expr>, value: bool) =
        this.AddScalar(Single.SupportsStroustrup.WithValue(value))
