namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module SingleNode =
    let Value = Attributes.defineScalar<StringOrWidget<Expr>> "Value"

    let SupportsStroustrup = Attributes.defineScalar<bool> "SupportsStroustrup"

    let AddSpace = Attributes.defineScalar<bool> "AddSpace"

    let Leading = Attributes.defineScalar<string> "Leading"

    let WidgetKey =
        Widgets.register "SingleNode" (fun widget ->
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
            ExprSingleNode(SingleTextNode.Create(leading), addSpace, supportsStroustrup, expr, Range.Zero))

[<AutoOpen>]
module SingleNodeBuilders =
    type Ast with

        static member SingleNode(leading: string, value: WidgetBuilder<Expr>) =
            WidgetBuilder<ExprSingleNode>(
                SingleNode.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        SingleNode.Leading.WithValue(leading),
                        SingleNode.Value.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak value))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member SingleNode(leading: string, value: StringVariant) =
            WidgetBuilder<ExprSingleNode>(
                SingleNode.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        SingleNode.Leading.WithValue(leading),
                        SingleNode.Value.WithValue(StringOrWidget.StringExpr(value))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

type SingleNodeModifiers =
    [<Extension>]
    static member inline addSpace(this: WidgetBuilder<ExprSingleNode>, value: bool) =
        this.AddScalar(SingleNode.AddSpace.WithValue(value))

    [<Extension>]
    static member inline supportsStroustrup(this: WidgetBuilder<ExprSingleNode>, value: bool) =
        this.AddScalar(SingleNode.SupportsStroustrup.WithValue(value))
