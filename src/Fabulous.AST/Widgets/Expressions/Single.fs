namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Single =
    let Value = Attributes.defineWidget "Value"

    let SupportsStroustrup = Attributes.defineScalar<bool> "SupportsStroustrup"

    let AddSpace = Attributes.defineScalar<bool> "AddSpace"

    let Leading = Attributes.defineScalar<string> "Leading"

    let WidgetKey =
        Widgets.register "Single" (fun widget ->
            let expr = Helpers.getNodeFromWidget<Expr> widget Value
            let supportsStroustrup = Helpers.getScalarValue widget SupportsStroustrup
            let addSpace = Helpers.getScalarValue widget AddSpace
            let leading = Helpers.getScalarValue widget Leading
            Expr.Single(ExprSingleNode(SingleTextNode.Create(leading), addSpace, supportsStroustrup, expr, Range.Zero)))

[<AutoOpen>]
module SingleBuilders =
    type Ast with

        static member SingleExpr
            (
                leading: string,
                addSpace: bool,
                supportsStroustrup: bool,
                value: WidgetBuilder<Expr>
            ) =
            WidgetBuilder<Expr>(
                Single.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        Single.Leading.WithValue(leading),
                        Single.AddSpace.WithValue(addSpace),
                        Single.SupportsStroustrup.WithValue(supportsStroustrup)
                    ),
                    ValueSome [| Single.Value.WithValue(value.Compile()) |],
                    ValueNone
                )
            )
