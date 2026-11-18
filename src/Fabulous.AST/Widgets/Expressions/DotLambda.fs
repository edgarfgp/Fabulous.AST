namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module DotLambda =
    let Value = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "DotLambda" (fun widget ->
            let expr = Widgets.getNodeFromWidget widget Value
            // Fantomas 8: `_.Body` is a chain with `_` as its head. The F# compiler requires a
            // dot-lambda body to be atomic, hence no space before a terminal call.
            Expr.Chain(
                ExprChain(
                    Expr.Ident(SingleTextNode.underscore),
                    [ ChainSegment.DotMember(SingleTextNode.dot, expr) ],
                    ChainTerminal.NoTerminal,
                    Range.range0
                )
            ))

[<AutoOpen>]
module DotLambdaBuilders =
    type Ast with

        static member DotLambdaExpr(value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(DotLambda.WidgetKey, DotLambda.Value.WithValue(value.Compile()))

        static member DotLambdaExpr(value: WidgetBuilder<Constant>) =
            Ast.DotLambdaExpr(Ast.ConstantExpr(value))

        static member DotLambdaExpr(value: string) = Ast.DotLambdaExpr(Ast.Constant(value))
