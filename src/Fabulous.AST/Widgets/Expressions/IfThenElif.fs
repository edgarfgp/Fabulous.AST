namespace Fabulous.AST

open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module IfThenElif =
    let Branches = Attributes.defineWidgetCollection "ElifExpr"

    let ElseExpr = Attributes.defineScalar<StringOrWidget<Expr>> "ElseExpr"

    let WidgetKey =
        Widgets.register "IfThenElif" (fun widget ->
            let branches =
                Widgets.getNodesFromWidgetCollection<Expr> widget Branches
                |> List.choose(fun x ->
                    match Expr.Node(x) with
                    | :? ExprIfThenNode as node -> Some node
                    | _ -> None)

            let elseExpr = Widgets.tryGetScalarValue widget ElseExpr

            let hasQuotes =
                Widgets.tryGetScalarValue widget Expr.HasQuotes |> ValueOption.defaultValue true

            let elseExpr =
                match elseExpr with
                | ValueNone -> None
                | ValueSome stringOrWidget ->
                    match stringOrWidget with
                    | StringOrWidget.StringExpr value ->
                        Some(
                            SingleTextNode.``else``,
                            Expr.Constant(
                                Constant.FromText(
                                    SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value, hasQuotes))
                                )
                            )
                        )
                    | StringOrWidget.WidgetExpr expr -> Some(SingleTextNode.``else``, expr)

            Expr.IfThenElif(ExprIfThenElifNode(branches, elseExpr, Range.Zero)))

[<AutoOpen>]
module IfThenElifBuilders =
    type Ast with

        static member inline IfThenElifExpr(elseExpr: WidgetBuilder<Expr>) =
            CollectionBuilder<Expr, Expr>(
                IfThenElif.WidgetKey,
                IfThenElif.Branches,
                AttributesBundle(
                    StackList.one(IfThenElif.ElseExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak elseExpr))),
                    ValueNone,
                    ValueNone
                )
            )

        static member inline IfThenElifExpr(elseExpr: string) =
            CollectionBuilder<Expr, Expr>(
                IfThenElif.WidgetKey,
                IfThenElif.Branches,
                AttributesBundle(
                    StackList.one(IfThenElif.ElseExpr.WithValue(StringOrWidget.StringExpr(elseExpr))),
                    ValueNone,
                    ValueNone
                )
            )

        static member inline IfThenElifExpr() =
            CollectionBuilder<Expr, Expr>(
                IfThenElif.WidgetKey,
                IfThenElif.Branches,
                AttributesBundle(StackList.empty(), ValueNone, ValueNone)
            )
