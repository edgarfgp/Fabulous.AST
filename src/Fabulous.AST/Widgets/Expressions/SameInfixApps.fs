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

        static member SameInfixAppsExpr
            (leading: WidgetBuilder<Expr>, subsequentExpressions: (string * WidgetBuilder<Expr>) list)
            =
            let subsequentExpressions =
                subsequentExpressions |> List.map(fun (op, expr) -> op, Gen.mkOak expr)

            WidgetBuilder<Expr>(
                SameInfixApps.WidgetKey,
                AttributesBundle(
                    StackList.one(SameInfixApps.SubsequentExpressions.WithValue(subsequentExpressions)),
                    [| SameInfixApps.LeadingExpr.WithValue(leading.Compile()) |],
                    Array.empty
                )
            )
