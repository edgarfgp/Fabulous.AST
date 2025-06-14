namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ParenLambda =
    let Value = Attributes.defineWidget "Lambda"

    let Parameters = Attributes.defineScalar<Pattern seq> "Parameters"

    let WidgetKey =
        Widgets.register "ParenLambda" (fun widget ->
            let expr = Widgets.getNodeFromWidget<Expr> widget Value
            let parameters = Widgets.getScalarValue widget Parameters |> List.ofSeq

            Expr.ParenLambda(
                ExprParenLambdaNode(
                    SingleTextNode.leftParenthesis,
                    ExprLambdaNode(SingleTextNode.``fun``, parameters, SingleTextNode.arrow, expr, Range.Zero),
                    SingleTextNode.rightParenthesis,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module ParenLambdaBuilders =
    type Ast with

        static member ParenLambdaExpr(parameters: WidgetBuilder<Pattern> seq, value: WidgetBuilder<Expr>) =
            let parameters = parameters |> Seq.map Gen.mkOak

            WidgetBuilder<Expr>(
                ParenLambda.WidgetKey,
                AttributesBundle(
                    StackList.one(ParenLambda.Parameters.WithValue(parameters)),
                    [| ParenLambda.Value.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member ParenLambdaExpr(parameters: WidgetBuilder<Pattern> seq, value: WidgetBuilder<Constant>) =
            Ast.ParenLambdaExpr(parameters, Ast.ConstantExpr(value))

        static member ParenLambdaExpr(parameters: WidgetBuilder<Constant> seq, value: WidgetBuilder<Constant>) =
            let parameter = parameters |> Seq.map Ast.ConstantPat
            Ast.ParenLambdaExpr(parameter, Ast.ConstantExpr(value))

        static member ParenLambdaExpr(parameters: WidgetBuilder<Pattern> seq, value: string) =
            Ast.ParenLambdaExpr(parameters, Ast.Constant(value))

        static member ParenLambdaExpr(parameters: WidgetBuilder<Constant> seq, value: string) =
            let parameter = parameters |> Seq.map Ast.ConstantPat
            Ast.ParenLambdaExpr(parameter, Ast.Constant(value))

        static member ParenLambdaExpr(parameter: WidgetBuilder<Pattern>, value: WidgetBuilder<Expr>) =
            Ast.ParenLambdaExpr([ parameter ], value)

        static member ParenLambdaExpr(parameter: WidgetBuilder<Pattern>, value: WidgetBuilder<Constant>) =
            Ast.ParenLambdaExpr([ parameter ], Ast.ConstantExpr(value))

        static member ParenLambdaExpr(parameter: WidgetBuilder<Constant>, value: WidgetBuilder<Constant>) =
            Ast.ParenLambdaExpr([ Ast.ConstantPat(parameter) ], Ast.ConstantExpr(value))

        static member ParenLambdaExpr(parameter: WidgetBuilder<Pattern>, value: string) =
            Ast.ParenLambdaExpr([ parameter ], Ast.Constant(value))

        static member ParenLambdaExpr(parameter: WidgetBuilder<Constant>, value: string) =
            Ast.ParenLambdaExpr([ Ast.ConstantPat(parameter) ], Ast.Constant(value))

        static member ParenLambdaExpr(parameter: string, value: WidgetBuilder<Expr>) =
            Ast.ParenLambdaExpr([ Ast.ConstantPat(parameter) ], value)

        static member ParenLambdaExpr(parameters: string seq, value: WidgetBuilder<Expr>) =
            let parameters = parameters |> Seq.map Ast.ConstantPat
            Ast.ParenLambdaExpr(parameters, value)

        static member ParenLambdaExpr(parameters: string seq, value: WidgetBuilder<Constant>) =
            let parameters = parameters |> Seq.map Ast.ConstantPat
            Ast.ParenLambdaExpr(parameters, Ast.ConstantExpr(value))

        static member ParenLambdaExpr(parameters: string seq, value: string) =
            Ast.ParenLambdaExpr(parameters |> Seq.map Ast.ConstantPat, Ast.Constant(value))
