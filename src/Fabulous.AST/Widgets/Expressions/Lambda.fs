namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Lambda =
    let Value = Attributes.defineWidget "Value"

    let Parameters = Attributes.defineScalar<Pattern seq> "Parameters"

    let WidgetKey =
        Widgets.register "Lambda" (fun widget ->
            let expr = Widgets.getNodeFromWidget<Expr> widget Value
            let parameters = Widgets.getScalarValue widget Parameters |> List.ofSeq
            Expr.Lambda(ExprLambdaNode(SingleTextNode.``fun``, parameters, SingleTextNode.arrow, expr, Range.Zero)))

[<AutoOpen>]
module LambdaBuilders =
    type Ast with

        static member LambdaExpr(parameters: WidgetBuilder<Pattern> seq, value: WidgetBuilder<Expr>) =
            let parameters = parameters |> Seq.map Gen.mkOak

            WidgetBuilder<Expr>(
                Lambda.WidgetKey,
                AttributesBundle(
                    StackList.one(Lambda.Parameters.WithValue(parameters)),
                    [| Lambda.Value.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member LambdaExpr(parameter: WidgetBuilder<Pattern>, value: WidgetBuilder<Expr>) =
            Ast.LambdaExpr([ parameter ], value)

        static member LambdaExpr(parameter: WidgetBuilder<Pattern>, value: WidgetBuilder<Constant>) =
            Ast.LambdaExpr([ parameter ], Ast.ConstantExpr(value))

        static member LambdaExpr(parameters: WidgetBuilder<Pattern> seq, value: WidgetBuilder<Constant>) =
            Ast.LambdaExpr(parameters, Ast.ConstantExpr(value))

        static member LambdaExpr(parameters: WidgetBuilder<Constant> seq, value: WidgetBuilder<Expr>) =
            let parameter = parameters |> Seq.map Ast.ConstantPat
            Ast.LambdaExpr(parameter, value)

        static member LambdaExpr(parameters: WidgetBuilder<Constant> seq, value: WidgetBuilder<Constant>) =
            let parameter = parameters |> Seq.map Ast.ConstantPat
            Ast.LambdaExpr(parameter, Ast.ConstantExpr(value))

        static member LambdaExpr(parameters: WidgetBuilder<Pattern> seq, value: string) =
            Ast.LambdaExpr(parameters, Ast.Constant(value))

        static member LambdaExpr(parameters: string seq, value: WidgetBuilder<Expr>) =
            let parameters = parameters |> Seq.map Ast.ConstantPat
            Ast.LambdaExpr(parameters, value)

        static member LambdaExpr(parameters: string seq, value: WidgetBuilder<Constant>) =
            let parameters = parameters |> Seq.map Ast.ConstantPat
            Ast.LambdaExpr(parameters, Ast.ConstantExpr(value))

        static member LambdaExpr(parameters: string seq, value: string) =
            Ast.LambdaExpr(parameters |> Seq.map Ast.ConstantPat, Ast.Constant(value))

        static member LambdaExpr(parameters: string, value: string) =
            let parameters = [ parameters ] |> List.map Ast.ConstantPat
            Ast.LambdaExpr(parameters, Ast.Constant(value))
