namespace Fabulous.AST.Tests.Expressions

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Match =

    [<Test>]
    let ``let value with a Match expression`` () =
        AnonymousModule() {
            MatchExpr(
                ListExpr() {
                    ConstantExpr("1")
                    ConstantExpr("2")
                }
            ) {
                MatchClauseExpr(Named("a"), ConstantExpr("3"))
            }
        }
        |> produces
            """

match [ 1; 2 ] with
| a -> 3
"""
