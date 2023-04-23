namespace Fabulous.AST.Tests.ControlFlow

open FSharp.Compiler.Text
open Fabulous.AST
open Fabulous.AST.Tests

open type Ast
open Fantomas.Core.SyntaxOak
open NUnit.Framework

module IfThenElif =
    [<Test>]
    let ``Produces if-then expression with EscapeHatch`` () =
        let ifExp =
            Expr.InfixApp(
                ExprInfixAppNode(
                    Expr.Ident(SingleTextNode("x", Range.Zero)),
                    SingleTextNode("=", Range.Zero),
                    Expr.Ident(SingleTextNode("12", Range.Zero)),
                    Range.Zero
                )
            )

        let thenExpr =
            Expr.Constant(
                Constant.Unit(UnitNode(SingleTextNode("(", Range.Zero), SingleTextNode(")", Range.Zero), Range.Zero))
            )

        AnonymousModule() { IfThen(EscapeHatch(ifExp), EscapeHatch(thenExpr)) }
        |> produces
            """

if x = 12 then
    ()
"""

    [<Test>]
    let ``Produces if-then-elif expression with EscapeHatch`` () =
        let ifExp =
            Expr.InfixApp(
                ExprInfixAppNode(
                    Expr.Ident(SingleTextNode("x", Range.Zero)),
                    SingleTextNode("=", Range.Zero),
                    Expr.Ident(SingleTextNode("12", Range.Zero)),
                    Range.Zero
                )
            )

        let ifElifExp =
            Expr.InfixApp(
                ExprInfixAppNode(
                    Expr.Ident(SingleTextNode("x", Range.Zero)),
                    SingleTextNode("=", Range.Zero),
                    Expr.Ident(SingleTextNode("0", Range.Zero)),
                    Range.Zero
                )
            )

        let elifThenExpr =
            Expr.Constant(
                Constant.Unit(UnitNode(SingleTextNode("(", Range.Zero), SingleTextNode(")", Range.Zero), Range.Zero))
            )

        let thenExpr =
            Expr.Constant(
                Constant.Unit(UnitNode(SingleTextNode("(", Range.Zero), SingleTextNode(")", Range.Zero), Range.Zero))
            )

        let elifExpr =
            ExprIfThenNode(
                IfKeywordNode.SingleWord(SingleTextNode("elif", Range.Zero)),
                ifElifExp,
                SingleTextNode("then", Range.Zero),
                elifThenExpr,
                Range.Zero
            )

        AnonymousModule() { IfThenElIf(EscapeHatch(ifExp), EscapeHatch(thenExpr), [ elifExpr ]) }
        |> produces
            """

if x = 12 then
    ()
elif x = 0 then
    ()
"""

    [<Test>]
    let ``Produces if-then-elif-else expression with EscapeHatch`` () =
        let ifExp =
            Expr.InfixApp(
                ExprInfixAppNode(
                    Expr.Ident(SingleTextNode("x", Range.Zero)),
                    SingleTextNode("=", Range.Zero),
                    Expr.Ident(SingleTextNode("12", Range.Zero)),
                    Range.Zero
                )
            )

        let ifElifExp =
            Expr.InfixApp(
                ExprInfixAppNode(
                    Expr.Ident(SingleTextNode("x", Range.Zero)),
                    SingleTextNode("=", Range.Zero),
                    Expr.Ident(SingleTextNode("0", Range.Zero)),
                    Range.Zero
                )
            )

        let elifThenExpr =
            Expr.Constant(
                Constant.Unit(UnitNode(SingleTextNode("(", Range.Zero), SingleTextNode(")", Range.Zero), Range.Zero))
            )

        let thenExpr =
            Expr.Constant(
                Constant.Unit(UnitNode(SingleTextNode("(", Range.Zero), SingleTextNode(")", Range.Zero), Range.Zero))
            )

        let elifExpr =
            ExprIfThenNode(
                IfKeywordNode.SingleWord(SingleTextNode("elif", Range.Zero)),
                ifElifExp,
                SingleTextNode("then", Range.Zero),
                elifThenExpr,
                Range.Zero
            )

        AnonymousModule() {
            IfThenElIfElse(EscapeHatch(ifExp), EscapeHatch(thenExpr), [ elifExpr ], EscapeHatch(thenExpr))
        }
        |> produces
            """

if x = 12 then ()
elif x = 0 then ()
else ()
"""

    [<Test>]
    let ``Produces if-then-else expression with EscapeHatch`` () =
        let ifExp =
            Expr.InfixApp(
                ExprInfixAppNode(
                    Expr.Ident(SingleTextNode("x", Range.Zero)),
                    SingleTextNode("=", Range.Zero),
                    Expr.Ident(SingleTextNode("12", Range.Zero)),
                    Range.Zero
                )
            )

        let thenExpr =
            Expr.Constant(
                Constant.Unit(UnitNode(SingleTextNode("(", Range.Zero), SingleTextNode(")", Range.Zero), Range.Zero))
            )

        let elseExpr =
            Expr.Constant(
                Constant.Unit(UnitNode(SingleTextNode("(", Range.Zero), SingleTextNode(")", Range.Zero), Range.Zero))
            )

        AnonymousModule() { IfThenElse(EscapeHatch(ifExp), EscapeHatch(thenExpr), EscapeHatch(elseExpr)) }
        |> produces
            """

if x = 12 then ()
else ()

"""
