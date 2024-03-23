namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ComputationExpressionStatement =
    let Binding = Attributes.defineWidget "Value"

    let InKeyword = Attributes.defineScalar<bool> "InKeyword"

    let OtherExpr = Attributes.defineWidget "OtherExpr"

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
            let otherExpr = Widgets.getNodeFromWidget<Expr> widget OtherExpr
            ComputationExpressionStatement.OtherStatement(otherExpr))

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
                    [| ComputationExpressionStatement.OtherExpr.WithValue(value.Compile()) |],
                    Array.empty
                )
            )
