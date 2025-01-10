namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ExprBeginEnd =
    let Value = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "ExprBeginEnd" (fun widget ->
            let expr = Widgets.getNodeFromWidget widget Value
            Expr.BeginEnd(ExprBeginEndNode(SingleTextNode.``begin``, expr, SingleTextNode.``end``, Range.Zero)))

[<AutoOpen>]
module ExprBeginEndBuilders =
    type Ast with

        static member BeginEndExpr(value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(ExprBeginEnd.WidgetKey, ExprBeginEnd.Value.WithValue(value.Compile()))

        static member BeginEndExpr(value: WidgetBuilder<Constant>) =
            Ast.BeginEndExpr(Ast.ConstantExpr(value))

        static member BeginEndExpr(value: string) = Ast.BeginEndExpr(Ast.Constant(value))
