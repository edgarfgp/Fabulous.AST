namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

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

        static member IfThenExpr(ifExpr: WidgetBuilder<Expr>, thenExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                IfThen.WidgetKey,
                AttributesBundle(
                    StackList.one(IfThen.IfNode.WithValue(IfKeywordNode.SingleWord(SingleTextNode.``if``))),
                    [| IfThen.IfExpr.WithValue(ifExpr.Compile())
                       IfThen.ThenExpr.WithValue(thenExpr.Compile()) |],
                    Array.empty
                )
            )

        static member IfThenExpr(ifExpr: WidgetBuilder<Constant>, thenExpr: WidgetBuilder<Expr>) =
            Ast.IfThenExpr(Ast.ConstantExpr(ifExpr), thenExpr)

        static member IfThenExpr(ifExpr: string, thenExpr: WidgetBuilder<Expr>) =
            Ast.IfThenExpr(Ast.Constant(ifExpr), thenExpr)

        static member IfThenExpr(ifExpr: WidgetBuilder<Constant>, thenExpr: WidgetBuilder<Constant>) =
            Ast.IfThenExpr(ifExpr, Ast.ConstantExpr(thenExpr))

        static member IfThenExpr(ifExpr: string, thenExpr: WidgetBuilder<Constant>) =
            Ast.IfThenExpr(Ast.Constant(ifExpr), thenExpr)

        static member IfThenExpr(ifExpr: string, thenExpr: string) =
            Ast.IfThenExpr(ifExpr, Ast.Constant(thenExpr))

        static member ElIfThenExpr(elIfExpr: WidgetBuilder<Expr>, thenExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                IfThen.WidgetKey,
                AttributesBundle(
                    StackList.one(IfThen.IfNode.WithValue(IfKeywordNode.SingleWord(SingleTextNode.``elif``))),
                    [| IfThen.IfExpr.WithValue(elIfExpr.Compile())
                       IfThen.ThenExpr.WithValue(thenExpr.Compile()) |],
                    Array.empty
                )
            )

        static member ElIfThenExpr(elIfExpr: WidgetBuilder<Constant>, thenExpr: WidgetBuilder<Expr>) =
            Ast.ElIfThenExpr(Ast.ConstantExpr(elIfExpr), thenExpr)

        static member ElIfThenExpr(elIfExpr: string, thenExpr: WidgetBuilder<Expr>) =
            Ast.ElIfThenExpr(Ast.Constant(elIfExpr), thenExpr)

        static member ElIfThenExpr(elIfExpr: WidgetBuilder<Constant>, thenExpr: WidgetBuilder<Constant>) =
            Ast.ElIfThenExpr(elIfExpr, Ast.ConstantExpr(thenExpr))

        static member ElIfThenExpr(elIfExpr: string, thenExpr: WidgetBuilder<Constant>) =
            Ast.ElIfThenExpr(Ast.Constant(elIfExpr), thenExpr)

        static member ElIfThenExpr(elIfExpr: string, thenExpr: string) =
            Ast.ElIfThenExpr(elIfExpr, Ast.Constant(thenExpr))

        static member ElseIfThenExpr(elseIfExpr: WidgetBuilder<Expr>, thenExpr: WidgetBuilder<Expr>) =
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

        static member ElseIfThenExpr(elseIfExpr: WidgetBuilder<Constant>, thenExpr: WidgetBuilder<Expr>) =
            Ast.ElseIfThenExpr(Ast.ConstantExpr(elseIfExpr), thenExpr)

        static member ElseIfThenExpr(elseIfExpr: string, thenExpr: WidgetBuilder<Expr>) =
            Ast.ElseIfThenExpr(Ast.Constant(elseIfExpr), thenExpr)

        static member ElseIfThenExpr(elseIfExpr: WidgetBuilder<Constant>, thenExpr: WidgetBuilder<Constant>) =
            Ast.ElseIfThenExpr(elseIfExpr, Ast.ConstantExpr(thenExpr))

        static member ElseIfThenExpr(elseIfExpr: string, thenExpr: WidgetBuilder<Constant>) =
            Ast.ElseIfThenExpr(Ast.Constant(elseIfExpr), thenExpr)

        static member ElseIfThenExpr(elseIfExpr: string, thenExpr: string) =
            Ast.ElseIfThenExpr(elseIfExpr, Ast.Constant(thenExpr))
