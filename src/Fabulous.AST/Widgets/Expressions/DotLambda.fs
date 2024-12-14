namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module DotLambda =
    let Value = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "DotLambda" (fun widget ->
            let expr = Widgets.getNodeFromWidget widget Value
            Expr.DotLambda(ExprDotLambda(SingleTextNode.underscore, SingleTextNode.dot, expr, Range.Zero)))

[<AutoOpen>]
module DotLambdaBuilders =
    type Ast with

        static member DotLambdaExpr(value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(DotLambda.WidgetKey, DotLambda.Value.WithValue(value.Compile()))

        static member DotLambdaExpr(value: WidgetBuilder<Constant>) =
            Ast.DotLambdaExpr(Ast.ConstantExpr(value))

        static member DotLambdaExpr(value: string) = Ast.DotLambdaExpr(Ast.Constant(value))
