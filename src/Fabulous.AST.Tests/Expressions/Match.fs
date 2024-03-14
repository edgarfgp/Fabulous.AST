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
                        ConstantExpr(Constant("1").hasQuotes(false))
                        ConstantExpr(Constant("2").hasQuotes(false))
                    }
                ) {
                    MatchClauseExpr(NamedPat("a"), ConstantExpr(Constant("3").hasQuotes(false)))
                }
            }
        }
        |> produces
            """

match [ 1; 2 ] with
| a -> 3
"""
