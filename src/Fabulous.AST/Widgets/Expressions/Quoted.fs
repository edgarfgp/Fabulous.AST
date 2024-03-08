namespace Fabulous.AST

open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module Quoted =
    let Value = Attributes.defineWidget "Value"

    let WidgetKey =
        Widgets.register "Quoted" (fun widget ->
            let expr = Helpers.getNodeFromWidget<Expr> widget Value
            Expr.Quote(ExprQuoteNode(SingleTextNode.leftQuotation, expr, SingleTextNode.rightQuotation, Range.Zero)))

[<AutoOpen>]
module QuotedBuilders =
    type Ast with

        static member QuotedExpr(value: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                Quoted.WidgetKey,
                AttributesBundle(StackList.empty(), ValueSome [| Quoted.Value.WithValue(value.Compile()) |], ValueNone)
            )

        static member QuotedExpr(value: string, ?hasQuotes: bool) =
            match hasQuotes with
            | None
            | Some true ->
                WidgetBuilder<Expr>(
                    Quoted.WidgetKey,
                    AttributesBundle(
                        StackList.empty(),
                        ValueSome [| Quoted.Value.WithValue(Ast.ConstantExpr(value, true).Compile()) |],
                        ValueNone
                    )
                )
            | _ ->
                WidgetBuilder<Expr>(
                    Quoted.WidgetKey,
                    AttributesBundle(
                        StackList.empty(),
                        ValueSome [| Quoted.Value.WithValue(Ast.ConstantExpr(value, false).Compile()) |],
                        ValueNone
                    )
                )
