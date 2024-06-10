namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ComputationExpressionStatement =
    let Binding = Attributes.defineWidget "Value"
    let InKeyword = Attributes.defineScalar<bool> "InKeyword"
    let ExprValue = Attributes.defineWidget "OtherExpr"
    let PatternValue = Attributes.defineWidget "Pattern"

    let WidgetLetOrUseStatementKey =
        Widgets.register "LetOrUseStatement" (fun widget ->
            let binding = Widgets.getNodeFromWidget<BindingNode> widget Binding

            let inKeyword =
                match Widgets.tryGetScalarValue widget InKeyword with
                | ValueSome true -> Some(SingleTextNode.inKeyword)
                | _ -> None

            ComputationExpressionStatement.LetOrUseStatement(ExprLetOrUseNode(binding, inKeyword, Range.Zero)))

    let WidgetOtherStatementKey =
        Widgets.register "OtherStatement" (fun widget ->
            let otherExpr = Widgets.getNodeFromWidget<Expr> widget ExprValue
            ComputationExpressionStatement.OtherStatement(otherExpr))

    let WidgetLetOrUseBangStatementKey =
        Widgets.register "LetOrUseBangStatement" (fun widget ->
            let pat = Widgets.getNodeFromWidget<Pattern> widget PatternValue
            let expr = Widgets.getNodeFromWidget<Expr> widget ExprValue

            ComputationExpressionStatement.LetOrUseBangStatement(
                ExprLetOrUseBangNode(SingleTextNode.letBang, pat, SingleTextNode.equals, expr, Range.Zero)
            ))

    let WidgetAndBangStatementKey =
        Widgets.register "AndBangStatement" (fun widget ->
            let pat = Widgets.getNodeFromWidget<Pattern> widget PatternValue
            let expr = Widgets.getNodeFromWidget<Expr> widget ExprValue

            ComputationExpressionStatement.AndBangStatement(
                ExprAndBang(SingleTextNode.andBang, pat, SingleTextNode.equals, expr, Range.Zero)
            ))

[<AutoOpen>]
module ComputationExpressionStatementBuilders =
    type Ast with

        static member LetOrUseExpr(value: WidgetBuilder<BindingNode>) =
            WidgetBuilder<ComputationExpressionStatement>(
                ComputationExpressionStatement.WidgetLetOrUseStatementKey,
                AttributesBundle(
                    StackList.empty(),
                    [| ComputationExpressionStatement.Binding.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member OtherExpr(value: WidgetBuilder<Expr>) =
            WidgetBuilder<ComputationExpressionStatement>(
                ComputationExpressionStatement.WidgetOtherStatementKey,
                AttributesBundle(
                    StackList.empty(),
                    [| ComputationExpressionStatement.ExprValue.WithValue(value.Compile()) |],
                    Array.empty
                )
            )

        static member OtherExpr(value: WidgetBuilder<Constant>) = Ast.OtherExpr(Ast.ConstantExpr(value))

        static member OtherExpr(value: string) = Ast.OtherExpr(Ast.ConstantExpr(value))

        static member LetOrUseBangExpr(pat: WidgetBuilder<Pattern>, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<ComputationExpressionStatement>(
                ComputationExpressionStatement.WidgetLetOrUseBangStatementKey,
                AttributesBundle(
                    StackList.empty(),
                    [| ComputationExpressionStatement.PatternValue.WithValue(pat.Compile())
                       ComputationExpressionStatement.ExprValue.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member LetOrUseBangExpr(pat: WidgetBuilder<Constant>, expr: WidgetBuilder<Expr>) =
            Ast.LetOrUseBangExpr(Ast.ConstantPat(pat), expr)

        static member LetOrUseBangExpr(pat: WidgetBuilder<Constant>, expr: WidgetBuilder<Constant>) =
            Ast.LetOrUseBangExpr(Ast.ConstantPat(pat), Ast.ConstantExpr(expr))

        static member LetOrUseBangExpr(pat: string, expr: string) =
            Ast.LetOrUseBangExpr(Ast.Constant(pat), Ast.Constant(expr))

        static member AndBangExpr(pat: WidgetBuilder<Pattern>, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<ComputationExpressionStatement>(
                ComputationExpressionStatement.WidgetAndBangStatementKey,
                AttributesBundle(
                    StackList.empty(),
                    [| ComputationExpressionStatement.PatternValue.WithValue(pat.Compile())
                       ComputationExpressionStatement.ExprValue.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )

        static member AndBangExpr(pat: WidgetBuilder<Constant>, expr: WidgetBuilder<Expr>) =
            Ast.AndBangExpr(Ast.ConstantPat(pat), expr)

        static member AndBangExpr(pat: WidgetBuilder<Constant>, expr: WidgetBuilder<Constant>) =
            Ast.AndBangExpr(pat, Ast.ConstantExpr(expr))

        static member AndBangExpr(pat: string, expr: string) =
            Ast.AndBangExpr(Ast.Constant(pat), Ast.Constant(expr))
