namespace Fabulous.AST

open System.Runtime.CompilerServices
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module ForEach =
    let Pat = Attributes.defineWidget "Pat"

    let EnumExpr = Attributes.defineWidget "EnumExpr"

    let IsArrow = Attributes.defineScalar<bool> "IsArrow"

    let BodyExpr = Attributes.defineWidget "BodyExpr"

    let WidgetKey =
        Widgets.register "ForEach" (fun widget ->
            let pat = Widgets.getNodeFromWidget widget Pat
            let enumExpr = Widgets.getNodeFromWidget widget EnumExpr

            let isArrow =
                Widgets.tryGetScalarValue widget IsArrow |> ValueOption.defaultValue false

            let bodyExpr = Widgets.getNodeFromWidget widget BodyExpr

            Expr.ForEach(ExprForEachNode(SingleTextNode.``for``, pat, enumExpr, isArrow, bodyExpr, Range.Zero)))

[<AutoOpen>]
module ForEachBuilders =
    type Ast with

        static member ForEachExpr
            (pattern: WidgetBuilder<Pattern>, enumExpr: WidgetBuilder<Expr>, bodyExpr: WidgetBuilder<Expr>)
            =
            WidgetBuilder<Expr>(
                ForEach.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| ForEach.Pat.WithValue(pattern.Compile())
                       ForEach.EnumExpr.WithValue(enumExpr.Compile())
                       ForEach.BodyExpr.WithValue(bodyExpr.Compile()) |],
                    Array.empty
                )
            )

type ForEachExprModifiers =
    [<Extension>]
    static member useArrow(this: WidgetBuilder<Expr>) =
        this.AddScalar(ForEach.IsArrow.WithValue(true))
