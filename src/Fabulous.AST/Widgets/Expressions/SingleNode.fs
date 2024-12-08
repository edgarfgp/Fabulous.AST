namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module SingleNode =
    let Value = Attributes.defineWidget "Value"

    let SupportsStroustrup = Attributes.defineScalar<bool> "SupportsStroustrup"

    let AddSpace = Attributes.defineScalar<bool> "AddSpace"

    let Leading = Attributes.defineScalar<SingleTextNode> "Leading"

    let WidgetKey =
        Widgets.register "SingleNode" (fun widget ->
            let expr = Widgets.getNodeFromWidget widget Value

            let supportsStroustrup =
                Widgets.tryGetScalarValue widget SupportsStroustrup
                |> ValueOption.defaultValue false

            let addSpace =
                Widgets.tryGetScalarValue widget AddSpace |> ValueOption.defaultValue false

            let leading = Widgets.getScalarValue widget Leading
            ExprSingleNode(leading, addSpace, supportsStroustrup, expr, Range.Zero))

[<AutoOpen>]
module SingleNodeBuilders =
    type Ast with

        static member private BaseSingleNode(leading: SingleTextNode, value: WidgetBuilder<Expr>) =
            WidgetBuilder<ExprSingleNode>(
                SingleNode.WidgetKey,
                AttributesBundle(
                    StackList.one(SingleNode.Leading.WithValue(leading)),
                    [| SingleNode.Value.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member SingleNode(leading: string, value: WidgetBuilder<Expr>) =
            Ast.BaseSingleNode(SingleTextNode.Create(leading), value)

        static member SingleNode(leading: string, value: WidgetBuilder<Constant>) =
            Ast.BaseSingleNode(SingleTextNode.Create(leading), Ast.ConstantExpr(value))

        static member SingleNode(leading: string, value: string) =
            Ast.BaseSingleNode(SingleTextNode.Create(leading), Ast.ConstantExpr(Ast.Constant value))

        static member DoExpr(value: WidgetBuilder<Expr>) =
            Ast.BaseSingleNode(SingleTextNode.``do``, value)

        static member DoExpr(value: string) =
            Ast.BaseSingleNode(SingleTextNode.``do``, Ast.ConstantExpr(Ast.Constant value))

        static member DoExpr(value: WidgetBuilder<Constant>) =
            Ast.BaseSingleNode(SingleTextNode.``do``, Ast.ConstantExpr(value))

type SingleNodeModifiers =
    [<Extension>]
    static member inline addSpace(this: WidgetBuilder<ExprSingleNode>, value: bool) =
        this.AddScalar(SingleNode.AddSpace.WithValue(value))

    [<Extension>]
    static member inline supportsStroustrup(this: WidgetBuilder<ExprSingleNode>, value: bool) =
        this.AddScalar(SingleNode.SupportsStroustrup.WithValue(value))
