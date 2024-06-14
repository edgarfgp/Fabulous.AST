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
                TryWithExpr(ConstantExpr(Int(12)), MatchClauseExpr("_", AppExpr("failwith", String("Not implemented"))))
                TryWithExpr(Int(12), MatchClauseExpr(WildPat(), FailWithExpr(String("Not implemented"))))
                TryWithExpr("12", MatchClauseExpr("_", FailWithExpr(String("Not implemented"))))
                TryWithExpr(Int(12), [])
            }
        }
        |> produces
            """
try
    12
with
| _ -> failwith "Not implemented"

try
    12
with
| _ -> failwith "Not implemented"

try
    12
with
| _ -> failwith "Not implemented"

try
    12
with
"""
