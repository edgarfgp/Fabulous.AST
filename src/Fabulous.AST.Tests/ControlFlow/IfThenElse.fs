namespace Fabulous.AST.Tests.ControlFlow

open FSharp.Compiler.Text
open Fabulous.AST
open Fabulous.AST.Tests

open type Ast
open Fantomas.Core.SyntaxOak
open NUnit.Framework

module IfThenElse =
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

        AnonymousModule() { IfThenElse(EscapeHatch(ifExp), EscapeHatch(elseExpr)) { EscapeHatch(thenExpr) } }
        |> produces
            """

if x = 12 then () else ()
"""
