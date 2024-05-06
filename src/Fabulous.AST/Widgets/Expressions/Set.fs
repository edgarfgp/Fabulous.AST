namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Set =
    let IdentifierExpr = Attributes.defineWidget "Identifier"

    let SetExpr = Attributes.defineWidget "SetExpr"

    let WidgetKey =
        Widgets.register "Lazy" (fun widget ->
            let identifierExpr = Widgets.getNodeFromWidget widget IdentifierExpr
            let setExpr = Widgets.getNodeFromWidget widget SetExpr
            Expr.Set(ExprSetNode(identifierExpr, setExpr, Range.Zero)))

[<AutoOpen>]
module SetBuilders =
    type Ast with

        static member SetExpr(identifier: WidgetBuilder<Expr>, expr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                Set.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| Set.IdentifierExpr.WithValue(identifier.Compile())
                       Set.SetExpr.WithValue(expr.Compile()) |],
                    Array.empty
                )
            )
