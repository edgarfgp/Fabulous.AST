namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module IndexWithoutDot =
    let IdentifierExpr = Attributes.defineWidget "Identifier"

    let IndexExpr = Attributes.defineWidget "IndexExpr"

    let WidgetKey =
        Widgets.register "IndexWithoutDotExpr" (fun widget ->
            let identifierExpr = Widgets.getNodeFromWidget widget IdentifierExpr
            let indexExpr = Widgets.getNodeFromWidget widget IndexExpr
            Expr.IndexWithoutDot(ExprIndexWithoutDotNode(identifierExpr, indexExpr, Range.Zero)))

[<AutoOpen>]
module IndexWithoutDotBuilders =
    type Ast with

        static member IndexWithoutDotExpr(identifier: WidgetBuilder<Expr>, indexer: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                IndexWithoutDot.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| IndexWithoutDot.IdentifierExpr.WithValue(identifier.Compile())
                       IndexWithoutDot.IndexExpr.WithValue(indexer.Compile()) |],
                    Array.empty
                )
            )

        static member IndexWithoutDotExpr(identifier: WidgetBuilder<Expr>, indexer: WidgetBuilder<Constant>) =
            Ast.IndexWithoutDotExpr(identifier, Ast.ConstantExpr(indexer))

        static member IndexWithoutDotExpr(identifier: WidgetBuilder<Constant>, indexer: WidgetBuilder<Constant>) =
            Ast.IndexWithoutDotExpr(Ast.ConstantExpr(identifier), Ast.ConstantExpr(indexer))

        static member IndexWithoutDotExpr(identifier: WidgetBuilder<Constant>, indexer: WidgetBuilder<Expr>) =
            Ast.IndexWithoutDotExpr(Ast.ConstantExpr(identifier), indexer)

        static member IndexWithoutDotExpr(identifier: string, indexer: string) =
            Ast.IndexWithoutDotExpr(Ast.Constant(identifier), Ast.Constant(indexer))
