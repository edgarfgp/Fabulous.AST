namespace Fabulous.AST.Tests.Expressions

open Fantomas.FCS.Text
open Xunit
open Fabulous.AST
open Fabulous.AST.Tests
open Fantomas.Core.SyntaxOak

open type Ast

module IfThen =
    [<Fact>]
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

        let thenExpr =
            Expr.CompExprBody(
                ExprCompExprBodyNode([ ComputationExpressionStatement.OtherStatement(thenExpr) ], Range.Zero)
            )

        AnonymousModule() { IfThen(EscapeHatch(ifExp), EscapeHatch(thenExpr)) }
        |> produces
            """

if x = 12 then
    ()
"""

    [<Fact>]
    let ``Produces if-then expression  with expr widgets`` () =
        AnonymousModule() { IfThen(InfixAppExpr("x", "=", "12"), ConstantExpr(ConstantUnit())) }
        |> produces
            """

if x = 12 then
    ()
"""

    [<Fact>]
    let ``Produces elif-then expression with EscapeHatch`` () =
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

        let thenExpr =
            Expr.CompExprBody(
                ExprCompExprBodyNode([ ComputationExpressionStatement.OtherStatement(thenExpr) ], Range.Zero)
            )

        AnonymousModule() { ElIfThen(EscapeHatch(ifExp), EscapeHatch(thenExpr)) }
        |> produces
            """

elif x = 12 then
    ()
"""

    [<Fact>]
    let ``Produces elif-then expression with expr widget`` () =
        AnonymousModule() {
            ElIfThen(
                InfixAppExpr(ConstantExpr("x", false), "=", ConstantExpr("12", false)),
                ConstantExpr(ConstantUnit())
            )
        }
        |> produces
            """

elif x = 12 then
    ()
"""

    [<Fact>]
    let ``Produces elseif-then expression with expr widget`` () =
        AnonymousModule() {
            ElseIfThen(
                InfixAppExpr(ConstantExpr(Constant("x", false)), "=", ConstantExpr(Constant("12", false))),
                ConstantExpr(ConstantUnit())
            )
        }
        |> produces
            """

else if x = 12 then
    ()
"""

module ConditionalExpr =
    [<Fact>]
    let ``Produces If Then Elif Then expression with widgets`` () =
        AnonymousModule() {
            ConditionalExpr() {
                IfThen(
                    InfixAppExpr(ConstantExpr("x", false), "=", ConstantExpr("12", false)),
                    ConstantExpr(ConstantUnit())
                )

                ElIfThen(
                    InfixAppExpr(ConstantExpr(Constant("x", false)), "=", ConstantExpr("11", false)),
                    ConstantExpr(ConstantUnit())
                )
            }
        }
        |> produces
            """
if x = 12 then
    ()
elif x = 11 then
    ()
"""

    [<Fact>]
    let ``Produces If Then Elif Then Else expression with widgets`` () =
        AnonymousModule() {
            ConditionalExpr(ConstantExpr(ConstantUnit())) {
                IfThen(
                    InfixAppExpr(ConstantExpr("x", false), "=", ConstantExpr("12", false)),
                    ConstantExpr(ConstantUnit())
                )

                ElIfThen(
                    InfixAppExpr(ConstantExpr("x", false), "=", ConstantExpr("11", false)),
                    ConstantExpr(ConstantUnit())
                )
            }
        }
        |> produces
            """
if x = 12 then ()
elif x = 11 then ()
else ()
"""

    [<Fact>]
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
            Expr.CompExprBody(
                ExprCompExprBodyNode(
                    [ ComputationExpressionStatement.OtherStatement(
                          Expr.Constant(
                              Constant.Unit(
                                  UnitNode(SingleTextNode("(", Range.Zero), SingleTextNode(")", Range.Zero), Range.Zero)
                              )
                          )
                      ) ],
                    Range.Zero
                )
            )

        let elseExpr =
            Expr.CompExprBody(
                ExprCompExprBodyNode(
                    [ ComputationExpressionStatement.OtherStatement(
                          Expr.Constant(
                              Constant.Unit(
                                  UnitNode(SingleTextNode("(", Range.Zero), SingleTextNode(")", Range.Zero), Range.Zero)
                              )
                          )
                      ) ],
                    Range.Zero
                )
            )

        AnonymousModule() { IfThenElse(EscapeHatch(ifExp), EscapeHatch(thenExpr), EscapeHatch(elseExpr)) }
        |> produces
            """

if x = 12 then () else ()
"""

    [<Fact>]
    let ``Produces if-then-else expression with widgets`` () =

        AnonymousModule() {
            IfThenElse(
                InfixAppExpr(ConstantExpr(Constant("x", false)), "=", ConstantExpr(Constant("12", false))),
                ConstantExpr(ConstantUnit()),
                ConstantExpr(ConstantUnit())
            )
        }
        |> produces
            """

if x = 12 then () else ()
"""
