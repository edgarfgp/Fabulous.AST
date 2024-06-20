namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module TryWith =
    [<Fact>]
    let ``TryWith expression``() =
        Oak() {
            AnonymousModule() {
                TryWithExpr(Int(12), [])

                TryWithExpr(
                    ConstantExpr(Int(12)),
                    [ MatchClauseExpr("a", AppExpr("failwith", String("Not implemented"))) ]
                )

                TryWithExpr(
                    Int(12),
                    [ MatchClauseExpr("b", FailWithExpr(String("Not implemented")))
                      MatchClauseExpr(WildPat(), FailWithExpr("Not implemented")) ]
                )
            }
        }
        |> produces
            """
try
    12
with


try
    12
with
| a -> failwith "Not implemented"

try
    12
with
| b -> failwith "Not implemented"
| _ -> failwith "Not implemented"
"""
