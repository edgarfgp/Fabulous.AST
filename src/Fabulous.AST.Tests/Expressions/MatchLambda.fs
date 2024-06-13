namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module MatchLambda =

    [<Fact>]
    let ``let value with a MatchLambda expression``() =
        Oak() { AnonymousModule() { MatchLambdaExpr([ MatchClauseExpr("a", Int(3)) ]) } }
        |> produces
            """
function
| a -> 3
"""
