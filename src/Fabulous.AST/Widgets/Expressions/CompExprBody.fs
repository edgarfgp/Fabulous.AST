namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module CompExprBody =
    let Statements = Attributes.defineWidgetCollection "Value"

    let WidgetKey =
        Widgets.register "CompExprBody" (fun widget ->
            let statements =
                Widgets.getNodesFromWidgetCollection<ComputationExpressionStatement> widget Statements

            Expr.CompExprBody(ExprCompExprBodyNode(statements, Range.Zero)))

[<AutoOpen>]
module CompExprBodyBuilders =
    type Ast with

        static member CompExprBodyExpr() =
            CollectionBuilder<Expr, ComputationExpressionStatement>(
                CompExprBody.WidgetKey,
                CompExprBody.Statements,
                AttributesBundle(StackList.empty(), Array.empty, Array.empty)
            )
