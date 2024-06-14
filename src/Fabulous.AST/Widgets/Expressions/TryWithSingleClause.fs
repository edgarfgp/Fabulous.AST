namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module TryWithSingleClause =
    let Value = Attributes.defineWidget "Value"

    let Clause = Attributes.defineWidget "Clause"

    let WidgetKey =
        Widgets.register "TryWithSingleClause" (fun widget ->
            let expr = Widgets.getNodeFromWidget widget Value
            let clause = Widgets.getNodeFromWidget widget Clause

            Expr.TryWithSingleClause(
                ExprTryWithSingleClauseNode(SingleTextNode.``try``, expr, SingleTextNode.``with``, clause, Range.Zero)
            ))

[<AutoOpen>]
module TryWithSingleClauseBuilders =
    type Ast with

        static member TryWithSingleClauseExpr(value: WidgetBuilder<Expr>, clause: WidgetBuilder<MatchClauseNode>) =
            WidgetBuilder<Expr>(
                TryWithSingleClause.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| TryWithSingleClause.Value.WithValue(value.Compile())
                       TryWithSingleClause.Clause.WithValue(clause.Compile()) |],
                    Array.empty
                )
            )

        static member TryWithSingleClauseExpr(value: WidgetBuilder<Constant>, clause: WidgetBuilder<MatchClauseNode>) =
            Ast.TryWithSingleClauseExpr(Ast.ConstantExpr(value), clause)

        static member TryWithSingleClauseExpr(value: string, clause: WidgetBuilder<MatchClauseNode>) =
            Ast.TryWithSingleClauseExpr(Ast.Constant(value), clause)
