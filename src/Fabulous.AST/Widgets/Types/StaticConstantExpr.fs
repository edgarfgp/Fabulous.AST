namespace Fabulous.AST

open Fantomas.Core.SyntaxOak
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.FCS.Text

module TypeStaticConstantExpr =
    let Expr = Attributes.defineWidget "Expr"
    let Const = Attributes.defineScalar<string> "Const"

    let WidgetKey =
        Widgets.register "TypeStaticConstantExpr" (fun widget ->
            let expr = Helpers.getNodeFromWidget<Expr> widget Expr
            let constant = Helpers.getScalarValue widget Const
            Type.StaticConstantExpr(TypeStaticConstantExprNode(SingleTextNode.Create(constant), expr, Range.Zero)))

[<AutoOpen>]
module TypeStaticConstantExprBuilders =
    type Ast with
        static member TypeStaticConstantExpr(constant: string, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<Type>(
                TypeStaticConstantExpr.WidgetKey,
                AttributesBundle(
                    StackList.one(TypeStaticConstantExpr.Const.WithValue(constant)),
                    ValueSome [| TypeStaticConstantExpr.Expr.WithValue(expr.Compile()) |],
                    ValueNone
                )
            )
