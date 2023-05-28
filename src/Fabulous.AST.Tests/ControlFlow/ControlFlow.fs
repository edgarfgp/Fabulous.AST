namespace Fabulous.AST.Tests.ControlFlow

open NUnit.Framework
open FSharp.Compiler.Text
open Fabulous.AST
open Fabulous.AST.Tests

open type Ast
open Fantomas.Core.SyntaxOak

module ControlFlow =
    [<Test>]
    let ``Produces if-elif-then-else expression with EscapeHatch`` () =
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

        let elifExpr =
            ExprIfThenNode(
                IfKeywordNode.SingleWord(SingleTextNode("elif", Range.Zero)),
                ifElifExp,
                SingleTextNode("then", Range.Zero),
                elifThenExpr,
                Range.Zero
            )

        AnonymousModule() {
            IfThen(EscapeHatch(ifExp)) { EscapeHatch(thenExpr) }

            ElIfElse(EscapeHatch(elseExpr)) { EscapeHatch([ elifExpr ]) }
        }
        |> produces
            """

if x = 12 then
    ()

elif x = 0 then ()
else ()
"""
