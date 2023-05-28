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

        AnonymousModule() { ElIfElse(EscapeHatch(elseExpr)) { EscapeHatch([ elifExpr ]) } }
        |> produces
            """

elif x = 0 then ()
else ()
"""
