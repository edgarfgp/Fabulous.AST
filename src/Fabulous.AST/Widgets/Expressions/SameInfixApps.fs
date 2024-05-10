namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module SameInfixApps =
    let LeadingExpr = Attributes.defineWidget "LeadingExpr"

    let SubsequentExpressions = Attributes.defineScalar<(string * Expr) list> "Value"

    let WidgetKey =
        Widgets.register "SameInfixApps" (fun widget ->
            let leadingExpr = Widgets.getNodeFromWidget<Expr> widget LeadingExpr

            let subsequentExpressions =
                Widgets.getScalarValue widget SubsequentExpressions
                |> List.map(fun x -> SingleTextNode.Create(fst x), snd x)

            Expr.SameInfixApps(ExprSameInfixAppsNode(leadingExpr, subsequentExpressions, Range.Zero)))

[<AutoOpen>]
module SameInfixAppsBuilders =
    type Ast with

        static member SameInfixAppsExpr(leading: WidgetBuilder<Expr>, items: (string * WidgetBuilder<Expr>) list) =
            let subsequentExpressions = items |> List.map(fun (op, expr) -> op, Gen.mkOak expr)

            WidgetBuilder<Expr>(
                SameInfixApps.WidgetKey,
                AttributesBundle(
                    StackList.one(SameInfixApps.SubsequentExpressions.WithValue(subsequentExpressions)),
                    [| SameInfixApps.LeadingExpr.WithValue(leading.Compile()) |],
                    Array.empty
                )
            )

        static member SameInfixAppsExpr(leading: WidgetBuilder<Constant>, items: (string * WidgetBuilder<Expr>) list) =
            Ast.SameInfixAppsExpr(Ast.ConstantExpr(leading), items)

        static member SameInfixAppsExpr(leading: string, items: (string * WidgetBuilder<Expr>) list) =
            Ast.SameInfixAppsExpr(Ast.Constant(leading), items)

        static member SameInfixAppsExpr
            (leading: WidgetBuilder<Constant>, items: (string * WidgetBuilder<Constant>) list)
            =
            let items = items |> List.map(fun (op, expr) -> op, Ast.ConstantExpr(expr))
            Ast.SameInfixAppsExpr(Ast.ConstantExpr(leading), items)

        static member SameInfixAppsExpr(leading: WidgetBuilder<Constant>, items: (string * string) list) =
            let items = items |> List.map(fun (op, expr) -> op, Ast.Constant(expr))
            Ast.SameInfixAppsExpr(leading, items)
