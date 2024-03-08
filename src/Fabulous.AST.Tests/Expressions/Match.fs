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
                    ConstantExpr(Constant("1", false))
                    ConstantExpr(Constant("2", false))
                }
            ) {
                MatchClauseExpr(NamedPat("a"), ConstantExpr(Constant("3", false)))
            }
        }
        |> produces
            """

match [ 1; 2 ] with
| a -> 3
"""
