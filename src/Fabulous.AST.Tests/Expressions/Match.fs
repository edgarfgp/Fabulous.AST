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
                    ConstantExpr(ConstantString "1")
                    ConstantExpr(ConstantString "2")
                }
            ) {
                MatchClauseExpr(NamedPat("a"), ConstantExpr(ConstantString "3"))
            }
        }
        |> produces
            """

match [ 1; 2 ] with
| a -> 3
"""
