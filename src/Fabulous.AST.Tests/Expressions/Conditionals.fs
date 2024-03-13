namespace Fabulous.AST.Tests.Expressions

open Fantomas.FCS.Text
open Xunit
open Fabulous.AST
open Fabulous.AST.Tests
open Fantomas.Core.SyntaxOak

open type Ast

module IfThen =
    [<Fact>]
    let ``Produces if-then expression with EscapeHatch``() =
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

        AnonymousModule() { IfThenExpr(EscapeHatch(ifExp), EscapeHatch(thenExpr)) }
        |> produces
            """

if x = 12 then
    ()
"""

    [<Fact>]
    let ``Produces if-then expression on a let binding``() =
        let ifThenExpr =
            IfThenExpr(
                InfixAppExpr(ConstantExpr("x").hasQuotes(false), "=", ConstantExpr("12").hasQuotes(false)),
                ConstantExpr(ConstantUnit())
            )

        AnonymousModule() {
            Value("res", ifThenExpr)

            Value(
                "res2",
                IfThenElseExpr(
                    InfixAppExpr(
                        ConstantExpr(Constant("x").hasQuotes(false)),
                        "=",
                        ConstantExpr(Constant("12").hasQuotes(false))
                    ),
                    ConstantExpr(ConstantUnit()),
                    ConstantExpr(ConstantUnit())
                )
            )

            Value(
                "res3",
                IfThenElifExpr(ConstantExpr(ConstantUnit())) {
                    IfThenExpr(
                        InfixAppExpr(ConstantExpr("x").hasQuotes(false), "=", ConstantExpr("12").hasQuotes(false)),
                        ConstantExpr(ConstantUnit())
                    )

                    ElIfThenExpr(
                        InfixAppExpr(
                            ConstantExpr(Constant("x").hasQuotes(false)),
                            "=",
                            ConstantExpr("11").hasQuotes(false)
                        ),
                        ConstantExpr(ConstantUnit())
                    )
                }
            )

        }
        |> produces
            """

let res =
    if x = 12 then
        ()

let res2 = if x = 12 then () else ()

let res3 =
    if x = 12 then ()
    elif x = 11 then ()
    else ()
"""

    [<Fact>]
    let ``Produces if-then expression  with expr widgets``() =
        AnonymousModule() {
            IfThenExpr(InfixAppExpr("x", "=", "12").hasQuotes(false), ConstantExpr(ConstantUnit()))
            ElIfThenExpr(InfixAppExpr("x", "=", "12").hasQuotes(false), ConstantExpr(ConstantUnit()))
        }
        |> produces
            """

if x = 12 then
    ()

elif x = 12 then
    ()
"""

    [<Fact>]
    let ``Produces elif-then expression with EscapeHatch``() =
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

        AnonymousModule() { ElIfThenExpr(EscapeHatch(ifExp), EscapeHatch(thenExpr)) }
        |> produces
            """

elif x = 12 then
    ()
"""

    [<Fact>]
    let ``Produces elif-then expression with expr widget``() =
        AnonymousModule() {
            ElIfThenExpr(
                InfixAppExpr(ConstantExpr("x").hasQuotes(false), "=", ConstantExpr("12").hasQuotes(false)),
                ConstantExpr(ConstantUnit())
            )
        }
        |> produces
            """

elif x = 12 then
    ()
"""

    [<Fact>]
    let ``Produces elseif-then expression with expr widget``() =
        AnonymousModule() {
            ElseIfThenExpr(
                InfixAppExpr(
                    ConstantExpr(Constant("x").hasQuotes(false)),
                    "=",
                    ConstantExpr(Constant("12").hasQuotes(false))
                ),
                ConstantExpr(ConstantUnit())
            )
        }
        |> produces
            """

else if x = 12 then
    ()
"""

module IfThenElif =
    [<Fact>]
    let ``Produces If Then Elif Then expression with widgets``() =
        AnonymousModule() {
            IfThenElifExpr() {
                IfThenExpr(
                    InfixAppExpr(ConstantExpr("x").hasQuotes(false), "=", ConstantExpr("12").hasQuotes(false)),
                    ConstantExpr(ConstantUnit())
                )

                ElIfThenExpr(
                    InfixAppExpr(ConstantExpr(Constant("x").hasQuotes(false)), "=", ConstantExpr("11").hasQuotes(false)),
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
    let ``Produces If Then Elif Then Else expression with widgets``() =
        AnonymousModule() {
            IfThenElifExpr(ConstantExpr(ConstantUnit())) {
                IfThenExpr(
                    InfixAppExpr(ConstantExpr("x").hasQuotes(false), "=", ConstantExpr("12").hasQuotes(false)),
                    ConstantExpr(ConstantUnit())
                )

                ElIfThenExpr(
                    InfixAppExpr(ConstantExpr("x").hasQuotes(false), "=", ConstantExpr("11").hasQuotes(false)),
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
    let ``Produces if-then-else expression with EscapeHatch``() =
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

        AnonymousModule() { IfThenElseExpr(EscapeHatch(ifExp), EscapeHatch(thenExpr), EscapeHatch(elseExpr)) }
        |> produces
            """

if x = 12 then () else ()
"""

    [<Fact>]
    let ``Produces if-then-else expression with widgets``() =

        AnonymousModule() {
            IfThenElseExpr(
                InfixAppExpr(
                    ConstantExpr(Constant("x").hasQuotes(false)),
                    "=",
                    ConstantExpr(Constant("12").hasQuotes(false))
                ),
                ConstantExpr(ConstantUnit()),
                ConstantExpr(ConstantUnit())
            )
        }
        |> produces
            """

if x = 12 then () else ()
"""
