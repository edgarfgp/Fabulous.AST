namespace Fabulous.AST

open Fabulous.Builders
open Fabulous.Builders.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module IfThenElse =
    let IfExpr = Attributes.defineWidget "IfExpr"
    let ThenExpr = Attributes.defineWidget "ThenExpr"
    let ElseExpr = Attributes.defineWidget "ElseExpr"

    let WidgetKey =
        Widgets.register "IfThenElse" (fun widget ->
            let ifExpr = Widgets.getNodeFromWidget widget IfExpr
            let thenExpr = Widgets.getNodeFromWidget widget ThenExpr
            let elseExpr = Widgets.getNodeFromWidget widget ElseExpr

            Expr.IfThenElse(
                ExprIfThenElseNode(
                    IfKeywordNode.SingleWord(SingleTextNode.``if``),
                    ifExpr,
                    SingleTextNode.``then``,
                    thenExpr,
                    SingleTextNode.``else``,
                    elseExpr,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module IfThenElseBuilders =
    type Ast with

        static member inline IfThenElseExpr
            (ifExpr: WidgetBuilder<Expr>, thenExpr: WidgetBuilder<Expr>, elseExpr: WidgetBuilder<Expr>)
            =
            WidgetBuilder<Expr>(
                IfThenElse.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| IfThenElse.IfExpr.WithValue(ifExpr.Compile())
                       IfThenElse.ThenExpr.WithValue(thenExpr.Compile())
                       IfThenElse.ElseExpr.WithValue(elseExpr.Compile()) |],
                    Array.empty
                )
            )

        static member inline IfThenElseExpr
            (ifExpr: WidgetBuilder<Expr>, thenExpr: WidgetBuilder<Expr>, elseExpr: string)
            =
            Ast.IfThenElseExpr(ifExpr, thenExpr, Ast.ConstantExpr(elseExpr))

        static member inline IfThenElseExpr
            (ifExpr: WidgetBuilder<Expr>, thenExpr: string, elseExpr: WidgetBuilder<Expr>)
            =
            Ast.IfThenElseExpr(ifExpr, Ast.ConstantExpr(thenExpr), elseExpr)

        static member inline IfThenElseExpr(ifExpr: WidgetBuilder<Expr>, thenExpr: string, elseExpr: string) =
            Ast.IfThenElseExpr(ifExpr, Ast.ConstantExpr(thenExpr), Ast.ConstantExpr(elseExpr))

        static member inline IfThenElseExpr
            (ifExpr: WidgetBuilder<Expr>, thenExpr: WidgetBuilder<Constant>, elseExpr: WidgetBuilder<Expr>)
            =
            Ast.IfThenElseExpr(ifExpr, Ast.ConstantExpr(thenExpr), elseExpr)

        static member inline IfThenElseExpr
            (ifExpr: WidgetBuilder<Expr>, thenExpr: WidgetBuilder<Constant>, elseExpr: WidgetBuilder<Constant>) =
            Ast.IfThenElseExpr(ifExpr, Ast.ConstantExpr(thenExpr), Ast.ConstantExpr(elseExpr))

        static member inline IfThenElseExpr
            (ifExpr: WidgetBuilder<Expr>, thenExpr: WidgetBuilder<Constant>, elseExpr: string)
            =
            Ast.IfThenElseExpr(ifExpr, thenExpr, Ast.Constant(elseExpr))

        static member inline IfThenElseExpr
            (ifExpr: WidgetBuilder<Expr>, thenExpr: string, elseExpr: WidgetBuilder<Constant>)
            =
            Ast.IfThenElseExpr(ifExpr, Ast.Constant(thenExpr), elseExpr)

        static member inline IfThenElseExpr
            (ifExpr: WidgetBuilder<Constant>, thenExpr: WidgetBuilder<Expr>, elseExpr: WidgetBuilder<Expr>)
            =
            Ast.IfThenElseExpr(Ast.ConstantExpr(ifExpr), thenExpr, elseExpr)

        static member inline IfThenElseExpr
            (ifExpr: string, thenExpr: WidgetBuilder<Expr>, elseExpr: WidgetBuilder<Expr>)
            =
            Ast.IfThenElseExpr(Ast.Constant(ifExpr), thenExpr, elseExpr)

        static member inline IfThenElseExpr
            (ifExpr: WidgetBuilder<Constant>, thenExpr: WidgetBuilder<Constant>, elseExpr: WidgetBuilder<Expr>) =
            Ast.IfThenElseExpr(ifExpr, Ast.ConstantExpr(thenExpr), elseExpr)

        static member inline IfThenElseExpr
            (ifExpr: WidgetBuilder<Constant>, thenExpr: WidgetBuilder<Constant>, elseExpr: WidgetBuilder<Constant>) =
            Ast.IfThenElseExpr(ifExpr, Ast.ConstantExpr(thenExpr), Ast.ConstantExpr(elseExpr))

        static member inline IfThenElseExpr(ifExpr: string, thenExpr: string, elseExpr: WidgetBuilder<Expr>) =
            Ast.IfThenElseExpr(Ast.Constant(ifExpr), Ast.Constant(thenExpr), elseExpr)

        static member inline IfThenElseExpr
            (ifExpr: string, thenExpr: WidgetBuilder<Constant>, elseExpr: WidgetBuilder<Expr>)
            =
            Ast.IfThenElseExpr(Ast.Constant(ifExpr), thenExpr, elseExpr)

        static member inline IfThenElseExpr
            (ifExpr: string, thenExpr: WidgetBuilder<Constant>, elseExpr: WidgetBuilder<Constant>)
            =
            Ast.IfThenElseExpr(Ast.Constant(ifExpr), thenExpr, Ast.ConstantExpr(elseExpr))

        static member inline IfThenElseExpr(ifExpr: string, thenExpr: string, elseExpr: WidgetBuilder<Constant>) =
            Ast.IfThenElseExpr(Ast.Constant(ifExpr), Ast.Constant(thenExpr), elseExpr)

        static member inline IfThenElseExpr(ifExpr: string, thenExpr: string, elseExpr: string) =
            Ast.IfThenElseExpr(Ast.Constant(ifExpr), Ast.Constant(thenExpr), Ast.Constant(elseExpr))
