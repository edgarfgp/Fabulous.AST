namespace Fabulous.AST

open Fantomas.FCS.Text
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak

module IfThenElif =
    let Branches = Attributes.defineScalar<Expr list> "ElifExpr"

    let ElseExpr = Attributes.defineScalar<StringOrWidget<Expr>> "ElseExpr"

    let WidgetKey =
        Widgets.register "IfThenElif" (fun widget ->
            let branches =
                Widgets.getScalarValue widget Branches
                |> List.choose(fun x ->
                    match Expr.Node(x) with
                    | :? ExprIfThenNode as node -> Some node
                    | _ -> None)

            let elseExpr = Widgets.tryGetScalarValue widget ElseExpr

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
                                    SingleTextNode.Create(StringParsing.normalizeIdentifierQuotes(value))
                                )
                            )
                        )
                    | StringOrWidget.WidgetExpr expr -> Some(SingleTextNode.``else``, expr)

            Expr.IfThenElif(ExprIfThenElifNode(branches, elseExpr, Range.Zero)))

[<AutoOpen>]
module IfThenElifBuilders =
    type Ast with

        static member inline IfThenElifExpr(branches: WidgetBuilder<Expr> list, elseExpr: WidgetBuilder<Expr>) =
            let branches = branches |> List.map Gen.mkOak

            WidgetBuilder<Expr>(
                IfThenElif.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        IfThenElif.Branches.WithValue(branches),
                        IfThenElif.ElseExpr.WithValue(StringOrWidget.WidgetExpr(Gen.mkOak elseExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member inline IfThenElifExpr(branches: WidgetBuilder<Expr> list, elseExpr: StringVariant) =
            WidgetBuilder<Expr>(
                IfThenElif.WidgetKey,
                AttributesBundle(
                    StackList.two(
                        IfThenElif.Branches.WithValue(branches |> List.map Gen.mkOak),
                        IfThenElif.ElseExpr.WithValue(StringOrWidget.StringExpr(elseExpr))
                    ),
                    Array.empty,
                    Array.empty
                )
            )

        static member inline IfThenElifExpr(branches: WidgetBuilder<Expr> list) =
            WidgetBuilder<Expr>(
                IfThenElif.WidgetKey,
                AttributesBundle(
                    StackList.one(IfThenElif.Branches.WithValue(branches |> List.map Gen.mkOak)),
                    Array.empty,
                    Array.empty
                )
            )
