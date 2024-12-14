namespace Fabulous.AST

open Fabulous.AST
open Fabulous.AST.StackAllocatedCollections.StackList
open Fantomas.Core.SyntaxOak
open Fantomas.FCS.Text

module MatchClause =
    let PatternNamed = Attributes.defineWidget "PatternNamed"
    let WhenExpr = Attributes.defineWidget "WhenExpr"
    let BodyExpr = Attributes.defineWidget "BodyExpr"

    let WidgetKey =
        Widgets.register "MatchClause" (fun widget ->
            let pattern = Widgets.getNodeFromWidget widget PatternNamed
            let whenExpr = Widgets.tryGetNodeFromWidget widget WhenExpr
            let bodyExpr = Widgets.getNodeFromWidget widget BodyExpr

            let whenExpr =
                match whenExpr with
                | ValueNone -> None
                | ValueSome whenExpr -> Some whenExpr

            MatchClauseNode(
                Some(SingleTextNode.bar),
                pattern,
                whenExpr,
                SingleTextNode.rightArrow,
                bodyExpr,
                Range.Zero
            ))

[<AutoOpen>]
module MatchClauseBuilders =
    type Ast with

        static member MatchClauseExpr
            (pattern: WidgetBuilder<Pattern>, whenExpr: WidgetBuilder<Expr>, bodyExpr: WidgetBuilder<Expr>)
            =
            WidgetBuilder<MatchClauseNode>(
                MatchClause.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| MatchClause.PatternNamed.WithValue(pattern.Compile())
                       MatchClause.BodyExpr.WithValue(bodyExpr.Compile())
                       MatchClause.WhenExpr.WithValue(whenExpr.Compile()) |],
                    Array.empty
                )
            )

        static member MatchClauseExpr
            (pattern: WidgetBuilder<Constant>, whenExpr: WidgetBuilder<Expr>, bodyExpr: WidgetBuilder<Expr>)
            =
            Ast.MatchClauseExpr(Ast.ConstantPat(pattern), whenExpr, bodyExpr)

        static member MatchClauseExpr(pattern: string, whenExpr: WidgetBuilder<Expr>, bodyExpr: WidgetBuilder<Expr>) =
            Ast.MatchClauseExpr(Ast.ConstantPat(pattern), whenExpr, bodyExpr)

        static member MatchClauseExpr
            (pattern: WidgetBuilder<Pattern>, whenExpr: WidgetBuilder<Constant>, bodyExpr: WidgetBuilder<Expr>) =
            Ast.MatchClauseExpr(pattern, Ast.ConstantExpr(whenExpr), bodyExpr)

        static member MatchClauseExpr
            (pattern: WidgetBuilder<Pattern>, whenExpr: string, bodyExpr: WidgetBuilder<Expr>)
            =
            Ast.MatchClauseExpr(pattern, Ast.Constant(whenExpr), bodyExpr)

        static member MatchClauseExpr
            (pattern: WidgetBuilder<Constant>, whenExpr: WidgetBuilder<Constant>, bodyExpr: WidgetBuilder<Expr>) =
            Ast.MatchClauseExpr(pattern, Ast.ConstantExpr(whenExpr), bodyExpr)

        static member MatchClauseExpr
            (pattern: WidgetBuilder<Constant>, whenExpr: string, bodyExpr: WidgetBuilder<Expr>)
            =
            Ast.MatchClauseExpr(pattern, Ast.Constant(whenExpr), bodyExpr)

        static member MatchClauseExpr
            (pattern: string, whenExpr: WidgetBuilder<Constant>, bodyExpr: WidgetBuilder<Expr>)
            =
            Ast.MatchClauseExpr(Ast.ConstantPat(pattern), whenExpr, bodyExpr)

        static member MatchClauseExpr(pattern: string, whenExpr: string, bodyExpr: WidgetBuilder<Expr>) =
            Ast.MatchClauseExpr(pattern, Ast.Constant(whenExpr), bodyExpr)

        static member MatchClauseExpr
            (pattern: WidgetBuilder<Pattern>, whenExpr: WidgetBuilder<Expr>, bodyExpr: WidgetBuilder<Constant>) =
            Ast.MatchClauseExpr(pattern, whenExpr, Ast.ConstantExpr(bodyExpr))

        static member MatchClauseExpr
            (pattern: WidgetBuilder<Constant>, whenExpr: WidgetBuilder<Expr>, bodyExpr: WidgetBuilder<Constant>) =
            Ast.MatchClauseExpr(pattern, whenExpr, Ast.ConstantExpr(bodyExpr))

        static member MatchClauseExpr
            (pattern: string, whenExpr: WidgetBuilder<Expr>, bodyExpr: WidgetBuilder<Constant>)
            =
            Ast.MatchClauseExpr(Ast.ConstantPat(pattern), whenExpr, Ast.ConstantExpr(bodyExpr))

        static member MatchClauseExpr
            (pattern: WidgetBuilder<Pattern>, whenExpr: string, bodyExpr: WidgetBuilder<Constant>)
            =
            Ast.MatchClauseExpr(pattern, Ast.Constant(whenExpr), Ast.ConstantExpr(bodyExpr))

        static member MatchClauseExpr
            (pattern: WidgetBuilder<Constant>, whenExpr: string, bodyExpr: WidgetBuilder<Constant>)
            =
            Ast.MatchClauseExpr(pattern, Ast.Constant(whenExpr), Ast.ConstantExpr(bodyExpr))

        static member MatchClauseExpr
            (pattern: string, whenExpr: WidgetBuilder<Constant>, bodyExpr: WidgetBuilder<Constant>)
            =
            Ast.MatchClauseExpr(pattern, whenExpr, Ast.ConstantExpr(bodyExpr))

        static member MatchClauseExpr(pattern: string, whenExpr: string, bodyExpr: string) =
            Ast.MatchClauseExpr(pattern, whenExpr, Ast.ConstantExpr(bodyExpr))

        static member MatchClauseExpr(pattern: WidgetBuilder<Pattern>, bodyExpr: WidgetBuilder<Expr>) =
            WidgetBuilder<MatchClauseNode>(
                MatchClause.WidgetKey,
                AttributesBundle(
                    StackList.empty(),
                    [| MatchClause.PatternNamed.WithValue(pattern.Compile())
                       MatchClause.BodyExpr.WithValue(bodyExpr.Compile()) |],
                    Array.empty
                )
            )

        static member MatchClauseExpr(pattern: WidgetBuilder<Constant>, bodyExpr: WidgetBuilder<Expr>) =
            Ast.MatchClauseExpr(Ast.ConstantPat(pattern), bodyExpr)

        static member MatchClauseExpr(pattern: string, bodyExpr: WidgetBuilder<Expr>) =
            Ast.MatchClauseExpr(Ast.Constant(pattern), bodyExpr)

        static member MatchClauseExpr(pattern: WidgetBuilder<Pattern>, bodyExpr: WidgetBuilder<Constant>) =
            Ast.MatchClauseExpr(pattern, Ast.ConstantExpr(bodyExpr))

        static member MatchClauseExpr(pattern: WidgetBuilder<Constant>, bodyExpr: WidgetBuilder<Constant>) =
            Ast.MatchClauseExpr(pattern, Ast.ConstantExpr(bodyExpr))

        static member MatchClauseExpr(pattern: string, bodyExpr: WidgetBuilder<Constant>) =
            Ast.MatchClauseExpr(Ast.Constant(pattern), bodyExpr)

        static member MatchClauseExpr(pattern: string, bodyExpr: string) =
            Ast.MatchClauseExpr(pattern, Ast.Constant(bodyExpr))
