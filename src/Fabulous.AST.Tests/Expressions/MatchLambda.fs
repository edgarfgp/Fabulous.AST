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

    [<Fact>]
    let ``MatchLambda with single clause overload``() =
        Oak() { AnonymousModule() { MatchLambdaExpr(MatchClauseExpr("x", Int(42))) } }
        |> produces
            """
function
| x -> 42
"""
