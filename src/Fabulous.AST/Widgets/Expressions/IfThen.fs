namespace Fabulous.AST

open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module IfThen =
    let IfNode = Attributes.defineScalar<IfKeywordNode> "IfNode"
    let IfExpr = Attributes.defineWidget "IfExpr"
    let ThenExpr = Attributes.defineWidget "ThenExpr"

    let WidgetKey =
        Widgets.register "IfThen" (fun widget ->
            let ifNode = Widgets.getScalarValue widget IfNode
            let ifExpr = Widgets.getNodeFromWidget widget IfExpr
            let thenExpr = Widgets.getNodeFromWidget widget ThenExpr
            Expr.IfThen(ExprIfThenNode(ifNode, ifExpr, SingleTextNode.``then``, thenExpr, Range.Zero)))

[<AutoOpen>]
module IfThenBuilders =
    type Ast with

        static member inline IfThenExpr(ifExpr: WidgetBuilder<Expr>, thenExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                IfThen.WidgetKey,
                AttributesBundle(
                    StackList.one(IfThen.IfNode.WithValue(IfKeywordNode.SingleWord(SingleTextNode.``if``))),
                    [| IfThen.IfExpr.WithValue(ifExpr.Compile())
                       IfThen.ThenExpr.WithValue(thenExpr.Compile()) |],
                    Array.empty
                )
            )

        static member inline ElIfThenExpr(elIfExpr: WidgetBuilder<Expr>, thenExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                IfThen.WidgetKey,
                AttributesBundle(
                    StackList.one(IfThen.IfNode.WithValue(IfKeywordNode.SingleWord(SingleTextNode.``elif``))),
                    [| IfThen.IfExpr.WithValue(elIfExpr.Compile())
                       IfThen.ThenExpr.WithValue(thenExpr.Compile()) |],
                    Array.empty
                )
            )

        static member inline ElseIfThenExpr(elseIfExpr: WidgetBuilder<Expr>, thenExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                IfThen.WidgetKey,
                AttributesBundle(
                    StackList.one(
                        IfThen.IfNode.WithValue(
                            IfKeywordNode.ElseIf(
                                ElseIfNode(Range.Zero, Range.Zero, Unchecked.defaultof<Node>, Range.Zero)
                            )
                        )
                    ),
                    [| IfThen.IfExpr.WithValue(elseIfExpr.Compile())
                       IfThen.ThenExpr.WithValue(thenExpr.Compile()) |],
                    Array.empty
                )
            )
