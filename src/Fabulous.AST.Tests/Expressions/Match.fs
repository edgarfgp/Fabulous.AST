namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Match =

    [<Fact>]
    let ``let value with a Match expression``() =
        Oak() {
            AnonymousModule() {
                MatchExpr(
                    ListExpr() {
                        ConstantExpr(Constant(Unquoted "1"))
                        ConstantExpr(Constant(Unquoted "2"))
                    }
                ) {
                    MatchClauseExpr(NamedPat("a"), ConstantExpr(Constant(Unquoted "3")))
                }
            }
        }
        |> produces
            """

match [ 1; 2 ] with
| a -> 3
"""
