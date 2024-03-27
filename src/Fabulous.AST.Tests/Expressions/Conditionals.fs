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

        Oak() { AnonymousModule() { IfThenExpr(EscapeHatch(ifExp), EscapeHatch(thenExpr)) } }
        |> produces
            """

if x = 12 then
    ()
"""

    [<Fact>]
    let ``Produces if-then expression on a let binding``() =
        let ifThenExpr =
            IfThenExpr(
                InfixAppExpr(ConstantExpr(Unquoted "x"), "=", ConstantExpr(Unquoted "12")),
                ConstantExpr(ConstantUnit())
            )

        Oak() {
            AnonymousModule() {
                Value("res", ifThenExpr)

                Value(
                    "res2",
                    IfThenElseExpr(
                        InfixAppExpr(ConstantExpr(Constant(Unquoted "x")), "=", ConstantExpr(Constant(Unquoted "12"))),
                        ConstantExpr(ConstantUnit()),
                        ConstantExpr(ConstantUnit())
                    )
                )

                Value(
                    "res3",
                    IfThenElifExpr(
                        [ IfThenExpr(
                              InfixAppExpr(ConstantExpr(Unquoted "x"), "=", ConstantExpr(Unquoted "12")),
                              ConstantExpr(ConstantUnit())
                          )

                          ElIfThenExpr(
                              InfixAppExpr(ConstantExpr(Constant(Unquoted "x")), "=", ConstantExpr(Unquoted "11")),
                              ConstantExpr(ConstantUnit())
                          ) ],
                        ConstantExpr(ConstantUnit())
                    )
                )

            }
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
        Oak() {
            AnonymousModule() {
                IfThenExpr(InfixAppExpr(Unquoted "x", "=", Unquoted "12"), ConstantExpr(ConstantUnit()))
                ElIfThenExpr(InfixAppExpr(Unquoted "x", "=", Unquoted "12"), ConstantExpr(ConstantUnit()))
            }
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

        Oak() { AnonymousModule() { ElIfThenExpr(EscapeHatch(ifExp), EscapeHatch(thenExpr)) } }
        |> produces
            """

elif x = 12 then
    ()
"""

    [<Fact>]
    let ``Produces elif-then expression with expr widget``() =
        Oak() {
            AnonymousModule() {
                ElIfThenExpr(
                    InfixAppExpr(ConstantExpr(Unquoted "x"), "=", ConstantExpr(Unquoted "12")),
                    ConstantExpr(ConstantUnit())
                )
            }
        }
        |> produces
            """

elif x = 12 then
    ()
"""

    [<Fact>]
    let ``Produces elseif-then expression with expr widget``() =
        Oak() {
            AnonymousModule() {
                ElseIfThenExpr(
                    InfixAppExpr(ConstantExpr(Constant(Unquoted "x")), "=", ConstantExpr(Constant(Unquoted "12"))),
                    ConstantExpr(ConstantUnit())
                )
            }
        }
        |> produces
            """

else if x = 12 then
    ()
"""

module IfThenElif =
    [<Fact>]
    let ``Produces If Then Elif Then expression with widgets``() =
        Oak() {
            AnonymousModule() {
                IfThenElifExpr(
                    [ IfThenExpr(
                          InfixAppExpr(ConstantExpr(Unquoted "x"), "=", ConstantExpr(Unquoted "12")),
                          ConstantExpr(ConstantUnit())
                      )

                      ElIfThenExpr(
                          InfixAppExpr(ConstantExpr(Constant(Unquoted "x")), "=", ConstantExpr(Unquoted "11")),
                          ConstantExpr(ConstantUnit())
                      ) ]
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
        Oak() {
            AnonymousModule() {
                IfThenElifExpr(
                    [ IfThenExpr(
                          InfixAppExpr(ConstantExpr(Unquoted "x"), "=", ConstantExpr(Unquoted "12")),
                          ConstantExpr(ConstantUnit())
                      )

                      ElIfThenExpr(
                          InfixAppExpr(ConstantExpr(Unquoted "x"), "=", ConstantExpr(Unquoted "11")),
                          ConstantExpr(ConstantUnit())
                      ) ],
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

        Oak() { AnonymousModule() { IfThenElseExpr(EscapeHatch(ifExp), EscapeHatch(thenExpr), EscapeHatch(elseExpr)) } }
        |> produces
            """

    if x = 12 then () else ()
    """

    [<Fact>]
    let ``Produces if-then-else expression with widgets``() =

        Oak() {
            AnonymousModule() {
                IfThenElseExpr(
                    InfixAppExpr(ConstantExpr(Constant(Unquoted "x")), "=", ConstantExpr(Constant(Unquoted "12"))),
                    ConstantExpr(ConstantUnit()),
                    ConstantExpr(ConstantUnit())
                )
            }
        }
        |> produces
            """

if x = 12 then () else ()
"""
