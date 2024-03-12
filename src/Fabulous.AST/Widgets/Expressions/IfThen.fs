namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module IfThen =
    let IfNode = Attributes.defineScalar<IfKeywordNode> "IfNode"
    let IfExpr = Attributes.defineWidget "IfExpr"
    let ThenExpr = Attributes.defineWidget "ThenExpr"

    let WidgetKey =
        Widgets.register "IfThen" (fun widget ->
            let ifNode = Widgets.getScalarValue widget IfNode
            let ifExpr = Widgets.getNodeFromWidget<Expr> widget IfExpr
            let thenExpr = Widgets.getNodeFromWidget<Expr> widget ThenExpr
            Expr.IfThen(ExprIfThenNode(ifNode, ifExpr, SingleTextNode.``then``, thenExpr, Range.Zero)))

[<AutoOpen>]
module IfThenBuilders =
    type Ast with

        static member inline IfThenExpr(ifExpr: WidgetBuilder<Expr>, thenExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                IfThen.WidgetKey,
                AttributesBundle(
                    StackList.one(IfThen.IfNode.WithValue(IfKeywordNode.SingleWord(SingleTextNode.``if``))),
                    ValueSome
                        [| IfThen.IfExpr.WithValue(ifExpr.Compile())
                           IfThen.ThenExpr.WithValue(thenExpr.Compile()) |],
                    ValueNone
                )
            )

        static member inline IfThenExpr(ifExpr: string, thenExpr: string) =
            WidgetBuilder<Expr>(
                IfThen.WidgetKey,
                AttributesBundle(
                    StackList.one(IfThen.IfNode.WithValue(IfKeywordNode.SingleWord(SingleTextNode.``if``))),
                    ValueSome
                        [| IfThen.IfExpr.WithValue(Ast.ConstantExpr(ifExpr, false).Compile())
                           IfThen.ThenExpr.WithValue(Ast.ConstantExpr(thenExpr, false).Compile()) |],
                    ValueNone
                )
            )

        static member inline ElIfThenExpr(elIfExpr: WidgetBuilder<Expr>, thenExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                IfThen.WidgetKey,
                AttributesBundle(
                    StackList.one(IfThen.IfNode.WithValue(IfKeywordNode.SingleWord(SingleTextNode.``elif``))),
                    ValueSome
                        [| IfThen.IfExpr.WithValue(elIfExpr.Compile())
                           IfThen.ThenExpr.WithValue(thenExpr.Compile()) |],
                    ValueNone
                )
            )

        static member inline ElIfThenExpr(elIfExpr: string, thenExpr: string) =
            WidgetBuilder<Expr>(
                IfThen.WidgetKey,
                AttributesBundle(
                    StackList.one(IfThen.IfNode.WithValue(IfKeywordNode.SingleWord(SingleTextNode.``elif``))),
                    ValueSome
                        [| IfThen.IfExpr.WithValue(Ast.ConstantExpr(elIfExpr, false).Compile())
                           IfThen.ThenExpr.WithValue(Ast.ConstantExpr(thenExpr, false).Compile()) |],
                    ValueNone
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
                    ValueSome
                        [| IfThen.IfExpr.WithValue(elseIfExpr.Compile())
                           IfThen.ThenExpr.WithValue(thenExpr.Compile()) |],
                    ValueNone
                )
            )

        static member inline ElseIfThenExpr(elseIfExpr: string, thenExpr: string) =
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
                    ValueSome
                        [| IfThen.IfExpr.WithValue(Ast.ConstantExpr(elseIfExpr, false).Compile())
                           IfThen.ThenExpr.WithValue(Ast.ConstantExpr(thenExpr, false).Compile()) |],
                    ValueNone
                )
            )
