namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module AppWithLambda =
    let FuncName = Attributes.defineWidget "FuncName"

    let Arguments = Attributes.defineScalar<Expr list> "Parameters"

    let Clauses = Attributes.defineScalar<MatchClauseNode list> "Clauses"

    let Parameters = Attributes.defineScalar<Pattern list * Expr> "Parameters"

    let IsMatchLambda = Attributes.defineScalar<bool> "IsMatchLambda"

    let WidgetKey =
        Widgets.register "AppWithLambda" (fun widget ->
            let funcName = Widgets.getNodeFromWidget<Expr> widget FuncName
            let arguments = Widgets.getScalarValue widget Arguments
            let isMatchLambda = Widgets.getScalarValue widget IsMatchLambda

            let lambda =
                if isMatchLambda then
                    let clauses = Widgets.getScalarValue widget Clauses
                    Choice2Of2(ExprMatchLambdaNode(SingleTextNode.``function``, clauses, Range.Zero))
                else
                    let parameters, expr = Widgets.getScalarValue widget Parameters

                    Choice1Of2(
                        ExprLambdaNode(SingleTextNode.``fun``, parameters, SingleTextNode.arrow, expr, Range.Zero)
                    )

            Expr.AppWithLambda(
                ExprAppWithLambdaNode(
                    funcName,
                    arguments,
                    SingleTextNode.leftParenthesis,
                    lambda,
                    SingleTextNode.rightParenthesis,
                    Range.Zero
                )
            ))

[<AutoOpen>]
module AppWithLambdaBuilders =
    type Ast with

        static member AppWithLambdaExpr
            (
                funcName: WidgetBuilder<Expr>,
                arguments: WidgetBuilder<Expr> list,
                parameters: WidgetBuilder<Pattern> list,
                value: WidgetBuilder<Expr>
            ) =
            let arguments = arguments |> List.map Gen.mkOak
            let parameters = parameters |> List.map Gen.mkOak

            WidgetBuilder<Expr>(
                AppWithLambda.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        AppWithLambda.IsMatchLambda.WithValue(false),
                        AppWithLambda.Arguments.WithValue(arguments),
                        AppWithLambda.Parameters.WithValue(parameters, Gen.mkOak value)
                    ),
                    [| AppWithLambda.FuncName.WithValue(funcName.Compile()) |],
                    Array.empty
                )
            )

        static member AppWithLambdaExpr
            (funcName: WidgetBuilder<Expr>, parameters: WidgetBuilder<Pattern> list, value: WidgetBuilder<Expr>)
            =
            Ast.AppWithLambdaExpr(funcName, [], parameters, value)

        static member AppWithLambdaExpr
            (
                funcName: string,
                arguments: WidgetBuilder<Expr> list,
                parameters: WidgetBuilder<Pattern> list,
                value: WidgetBuilder<Expr>
            ) =
            Ast.AppWithLambdaExpr(Ast.ConstantExpr(funcName), arguments, parameters, value)

        static member AppWithLambdaExpr
            (
                funcName: string,
                arguments: WidgetBuilder<Expr> list,
                parameters: WidgetBuilder<Pattern> list,
                value: string
            ) =
            Ast.AppWithLambdaExpr(Ast.ConstantExpr(funcName), arguments, parameters, Ast.ConstantExpr value)

        static member AppWithLambdaExpr
            (funcName: string, arguments: string list, parameters: WidgetBuilder<Pattern> list, value: string)
            =
            let arguments = arguments |> List.map Ast.ConstantExpr
            Ast.AppWithLambdaExpr(Ast.ConstantExpr(funcName), arguments, parameters, Ast.ConstantExpr value)

        static member AppWithLambdaExpr
            (funcName: string, arguments: WidgetBuilder<Expr> list, parameters: string list, value: string)
            =
            let parameters = parameters |> List.map Ast.ConstantPat
            Ast.AppWithLambdaExpr(Ast.ConstantExpr(funcName), arguments, parameters, Ast.ConstantExpr value)

        static member AppWithLambdaExpr
            (funcName: string, arguments: string list, parameters: string list, value: string)
            =
            let parameters = parameters |> List.map Ast.ConstantPat
            let arguments = arguments |> List.map Ast.ConstantExpr
            Ast.AppWithLambdaExpr(Ast.ConstantExpr(funcName), arguments, parameters, Ast.ConstantExpr value)

        static member AppWithLambdaExpr(funcName: string, parameters: string list, value: string) =
            let parameters = parameters |> List.map Ast.ConstantPat
            Ast.AppWithLambdaExpr(Ast.ConstantExpr(funcName), [], parameters, Ast.ConstantExpr value)

        static member AppWithLambdaExpr
            (
                funcName: WidgetBuilder<Expr>,
                arguments: WidgetBuilder<Expr> list,
                parameters: WidgetBuilder<Pattern> list,
                value: string
            ) =
            Ast.AppWithLambdaExpr(funcName, arguments, parameters, Ast.ConstantExpr value)

        static member AppWithMatchLambdaExpr
            (
                funcName: WidgetBuilder<Expr>,
                arguments: WidgetBuilder<Expr> list,
                clauses: WidgetBuilder<MatchClauseNode> list
            ) =
            let clauses = clauses |> List.map Gen.mkOak
            let arguments = arguments |> List.map Gen.mkOak

            WidgetBuilder<Expr>(
                AppWithLambda.WidgetKey,
                AttributesBundle(
                    StackList.three(
                        AppWithLambda.IsMatchLambda.WithValue(true),
                        AppWithLambda.Arguments.WithValue(arguments),
                        AppWithLambda.Clauses.WithValue(clauses)
                    ),
                    [| AppWithLambda.FuncName.WithValue(funcName.Compile()) |],
                    Array.empty
                )
            )

        static member AppWithMatchLambdaExpr
            (funcName: string, arguments: WidgetBuilder<Expr> list, clauses: WidgetBuilder<MatchClauseNode> list)
            =
            Ast.AppWithMatchLambdaExpr(Ast.ConstantExpr(funcName), arguments, clauses)

        static member AppWithMatchLambdaExpr
            (funcName: string, arguments: string list, clauses: WidgetBuilder<MatchClauseNode> list)
            =
            let arguments = arguments |> List.map Ast.ConstantExpr
            Ast.AppWithMatchLambdaExpr(Ast.ConstantExpr(funcName), arguments, clauses)
