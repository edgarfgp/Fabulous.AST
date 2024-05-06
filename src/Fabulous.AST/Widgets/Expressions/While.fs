namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module While =
    let WhileExpr = Attributes.defineWidget "EnumExpr"

    let BodyExpr = Attributes.defineWidget "BodyExpr"

    let WidgetKey =
        Widgets.register "While" (fun widget ->
            let whileExpr = Widgets.getNodeFromWidget widget WhileExpr
            let doExpr = Widgets.getNodeFromWidget widget BodyExpr
            Expr.While(ExprWhileNode(SingleTextNode.``while``, whileExpr, doExpr, Range.Zero)))

[<AutoOpen>]
module WhileBuilders =
    type Ast with

        static member WhileExpr(expr: WidgetBuilder<Expr>, bodyExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<Expr>(
                While.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| While.WhileExpr.WithValue(expr.Compile())
                       While.BodyExpr.WithValue(bodyExpr.Compile()) |],
                    Array.empty
                )
            )
