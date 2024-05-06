namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module SingleNode =
    let Value = Attributes.defineWidget "Value"

    let SupportsStroustrup = Attributes.defineScalar<bool> "SupportsStroustrup"

    let AddSpace = Attributes.defineScalar<bool> "AddSpace"

    let Leading = Attributes.defineScalar<string> "Leading"

    let WidgetKey =
        Widgets.register "SingleNode" (fun widget ->
            let expr = Widgets.getNodeFromWidget widget Value
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
                    StackList.one(SingleNode.Leading.WithValue(leading)),
                    [| SingleNode.Value.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member SingleNode(leading: string, value: WidgetBuilder<Constant>) =
            Ast.SingleNode(leading, Ast.ConstantExpr(value))

        static member SingleNode(leading: string, value: string) =
            Ast.SingleNode(leading, Ast.Constant(value))

type SingleNodeModifiers =
    [<Extension>]
    static member inline addSpace(this: WidgetBuilder<ExprSingleNode>, value: bool) =
        this.AddScalar(SingleNode.AddSpace.WithValue(value))

    [<Extension>]
    static member inline supportsStroustrup(this: WidgetBuilder<ExprSingleNode>, value: bool) =
        this.AddScalar(SingleNode.SupportsStroustrup.WithValue(value))
