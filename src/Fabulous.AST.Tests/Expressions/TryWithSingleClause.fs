namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module TryWithSingleClause =
    [<Fact>]
    let ``TryWithSingleClause expression``() =
        Oak() {
            AnonymousModule() {
                TryWithSingleClauseExpr(
                    ConstantExpr(Int(12)),
                    MatchClauseExpr("_", AppExpr("failwith", String("Not implemented")))
                )

                TryWithSingleClauseExpr(Int(12), MatchClauseExpr(WildPat(), FailWithExpr(String("Not implemented"))))
                TryWithSingleClauseExpr("12", MatchClauseExpr("_", FailWithExpr(String("Not implemented"))))
            }
        }
        |> produces
            """
try
    12
with _ ->
    failwith "Not implemented"

try
    12
with _ ->
    failwith "Not implemented"

try
    12
with _ ->
    failwith "Not implemented"
"""
