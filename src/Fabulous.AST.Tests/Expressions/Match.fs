namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Match =

    [<Fact>]
    let ``Match expression``() =
        Oak() {
            AnonymousModule() {
                MatchExpr(
                    ListExpr([ ConstantExpr(Int(1)); ConstantExpr(Int(2)) ]),
                    MatchClauseExpr(NamedPat("a"), ConstantExpr(Int(3)))
                )
            }
        }
        |> produces
            """

match [ 1; 2 ] with
| a -> 3
"""

    [<Fact>]
    let ``Match expression 2``() =
        Oak() {
            AnonymousModule() {
                MatchExpr(ConstantExpr(Constant("[ 1; 2 ]")), MatchClauseExpr(NamedPat("a"), ConstantExpr(Int(3))))
            }
        }
        |> produces
            """

match [ 1; 2 ] with
| a -> 3
"""

    [<Fact>]
    let ``Match expression with multiple clauses``() =
        Oak() {
            AnonymousModule() {
                MatchExpr(
                    "value",
                    [ MatchClauseExpr(
                          LongIdentPat("Member.C.C7b7df1dc", "arg1"),
                          ConstantExpr("failwith \"Not implemented\"")
                      )

                      MatchClauseExpr(
                          LongIdentPat("Member.C.C7b7df1dc", ParenPat(TuplePat([ "arg1"; "arg2" ]))),
                          ConstantExpr("failwith \"Not implemented\"")
                      ) ]
                )
            }
        }
        |> produces
            """
match value with
| Member.C.C7b7df1dc arg1 -> failwith "Not implemented"
| Member.C.C7b7df1dc(arg1, arg2) -> failwith "Not implemented"
"""
