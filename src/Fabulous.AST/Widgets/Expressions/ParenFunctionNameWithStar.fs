namespace Fabulous.AST

open Fabulous.AST
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ParenFunctionNameWithStar =
    let FunctionName = Attributes.defineScalar<string> "Parameters"

    let WidgetKey =
        Widgets.register "ParenFunctionNameWithStar" (fun widget ->
            let functionName = Widgets.getScalarValue widget FunctionName

            Expr.ParenFunctionNameWithStar(
                ExprParenFunctionNameWithStarNode(
                    SingleTextNode.leftParenthesis,
                    SingleTextNode.Create(functionName),
                    SingleTextNode.rightParenthesis,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module ParenFunctionNameWithStarBuilders =
    type Ast with

        static member ParenFunctionNameWithStarExpr(name: string) =
            WidgetBuilder<Expr>(
                ParenFunctionNameWithStar.WidgetKey,
                ParenFunctionNameWithStar.FunctionName.WithValue(name)
            )
