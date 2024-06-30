namespace Fabulous.AST

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

        static member private ForEachBaseExpr
            (
                pattern: WidgetBuilder<Pattern>,
                enumExpr: WidgetBuilder<Expr>,
                bodyExpr: WidgetBuilder<Expr>,
                isArrow: bool
            ) =
            WidgetBuilder<Expr>(
                ForEach.WidgetKey,
                AttributesBundle(
                    StackList.one(ForEach.IsArrow.WithValue(isArrow)),
                    [| ForEach.Pat.WithValue(pattern.Compile())
                       ForEach.EnumExpr.WithValue(enumExpr.Compile())
                       ForEach.BodyExpr.WithValue(bodyExpr.Compile()) |],
                    Array.empty
                )
            )

        static member ForEachDoExpr
            (pattern: WidgetBuilder<Pattern>, enumExpr: WidgetBuilder<Expr>, bodyExpr: WidgetBuilder<Expr>)
            =
            Ast.ForEachBaseExpr(pattern, enumExpr, bodyExpr, false)

        static member ForEachDoExpr
            (pattern: WidgetBuilder<Constant>, enumExpr: WidgetBuilder<Expr>, bodyExpr: WidgetBuilder<Expr>)
            =
            Ast.ForEachDoExpr(Ast.ConstantPat(pattern), enumExpr, bodyExpr)

        static member ForEachDoExpr(pattern: string, enumExpr: WidgetBuilder<Expr>, bodyExpr: WidgetBuilder<Expr>) =
            Ast.ForEachDoExpr(Ast.Constant(pattern), enumExpr, bodyExpr)

        static member ForEachDoExpr
            (pattern: WidgetBuilder<Pattern>, enumExpr: WidgetBuilder<Constant>, bodyExpr: WidgetBuilder<Expr>) =
            Ast.ForEachDoExpr(pattern, Ast.ConstantExpr(enumExpr), bodyExpr)

        static member ForEachDoExpr(pattern: WidgetBuilder<Pattern>, enumExpr: string, bodyExpr: WidgetBuilder<Expr>) =
            Ast.ForEachDoExpr(pattern, Ast.Constant(enumExpr), bodyExpr)

        static member ForEachDoExpr
            (pattern: WidgetBuilder<Pattern>, enumExpr: WidgetBuilder<Expr>, bodyExpr: WidgetBuilder<Constant>) =
            Ast.ForEachDoExpr(pattern, enumExpr, Ast.ConstantExpr(bodyExpr))

        static member ForEachDoExpr(pattern: WidgetBuilder<Pattern>, enumExpr: WidgetBuilder<Expr>, bodyExpr: string) =
            Ast.ForEachDoExpr(pattern, enumExpr, Ast.Constant(bodyExpr))

        static member ForEachDoExpr
            (pattern: WidgetBuilder<Constant>, enumExpr: WidgetBuilder<Constant>, bodyExpr: WidgetBuilder<Expr>) =
            Ast.ForEachDoExpr(Ast.ConstantPat(pattern), enumExpr, bodyExpr)

        static member ForEachDoExpr(pattern: WidgetBuilder<Constant>, enumExpr: string, bodyExpr: WidgetBuilder<Expr>) =
            Ast.ForEachDoExpr(pattern, Ast.Constant(enumExpr), bodyExpr)

        static member ForEachDoExpr(pattern: string, enumExpr: string, bodyExpr: WidgetBuilder<Expr>) =
            Ast.ForEachDoExpr(Ast.ConstantPat(pattern), Ast.ConstantExpr(enumExpr), bodyExpr)

        static member ForEachDoExpr(pattern: string, enumExpr: string, bodyExpr: string) =
            Ast.ForEachDoExpr(Ast.ConstantPat(pattern), Ast.ConstantExpr(enumExpr), Ast.Constant(bodyExpr))

        static member ForEachArrowExpr
            (pattern: WidgetBuilder<Pattern>, enumExpr: WidgetBuilder<Expr>, bodyExpr: WidgetBuilder<Expr>)
            =
            Ast.ForEachBaseExpr(pattern, enumExpr, bodyExpr, true)

        static member ForEachArrowExpr
            (pattern: WidgetBuilder<Constant>, enumExpr: WidgetBuilder<Expr>, bodyExpr: WidgetBuilder<Expr>)
            =
            Ast.ForEachArrowExpr(Ast.ConstantPat(pattern), enumExpr, bodyExpr)

        static member ForEachArrowExpr(pattern: string, enumExpr: WidgetBuilder<Expr>, bodyExpr: WidgetBuilder<Expr>) =
            Ast.ForEachArrowExpr(Ast.Constant(pattern), enumExpr, bodyExpr)

        static member ForEachArrowExpr
            (pattern: WidgetBuilder<Pattern>, enumExpr: WidgetBuilder<Constant>, bodyExpr: WidgetBuilder<Expr>) =
            Ast.ForEachArrowExpr(pattern, Ast.ConstantExpr(enumExpr), bodyExpr)

        static member ForEachArrowExpr
            (pattern: WidgetBuilder<Pattern>, enumExpr: string, bodyExpr: WidgetBuilder<Expr>)
            =
            Ast.ForEachArrowExpr(pattern, Ast.Constant(enumExpr), bodyExpr)

        static member ForEachArrowExpr
            (pattern: WidgetBuilder<Pattern>, enumExpr: WidgetBuilder<Expr>, bodyExpr: WidgetBuilder<Constant>) =
            Ast.ForEachArrowExpr(pattern, enumExpr, Ast.ConstantExpr(bodyExpr))

        static member ForEachArrowExpr
            (pattern: WidgetBuilder<Pattern>, enumExpr: WidgetBuilder<Expr>, bodyExpr: string)
            =
            Ast.ForEachArrowExpr(pattern, enumExpr, Ast.Constant(bodyExpr))

        static member ForEachArrowExpr
            (pattern: WidgetBuilder<Constant>, enumExpr: WidgetBuilder<Constant>, bodyExpr: WidgetBuilder<Expr>) =
            Ast.ForEachArrowExpr(Ast.ConstantPat(pattern), enumExpr, bodyExpr)

        static member ForEachArrowExpr
            (pattern: WidgetBuilder<Constant>, enumExpr: WidgetBuilder<Constant>, bodyExpr: WidgetBuilder<Constant>) =
            Ast.ForEachArrowExpr(Ast.ConstantPat(pattern), enumExpr, Ast.ConstantExpr(bodyExpr))

        static member ForEachArrowExpr
            (pattern: WidgetBuilder<Constant>, enumExpr: string, bodyExpr: WidgetBuilder<Expr>)
            =
            Ast.ForEachArrowExpr(pattern, Ast.Constant(enumExpr), bodyExpr)

        static member ForEachArrowExpr(pattern: string, enumExpr: string, bodyExpr: string) =
            Ast.ForEachArrowExpr(Ast.ConstantPat(pattern), Ast.ConstantExpr(enumExpr), Ast.Constant(bodyExpr))
