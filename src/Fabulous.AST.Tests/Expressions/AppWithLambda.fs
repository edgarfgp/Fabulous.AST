namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module AppWithLambda =

    [<Fact>]
    let ``AppWithLambda expression``() =
        Oak() { AnonymousModule() { AppWithLambdaExpr(ConstantExpr "c", [ ConstantPat("a") ], ConstantExpr("a")) } }
        |> produces
            """
c (fun a -> a)
"""

    [<Fact>]
    let ``AppWithLambda expression with strings``() =
        Oak() { AnonymousModule() { AppWithLambdaExpr("c", [ "a" ], "a") } }
        |> produces
            """
c (fun a -> a)
"""

    [<Fact>]
    let ``AppWithMatchLambdaExpr expression``() =
        Oak() {
            AnonymousModule() {
                AppWithMatchLambdaExpr(ConstantExpr("c"), [ ConstantExpr("a") ], [ MatchClauseExpr("a", "12") ])
            }
        }
        |> produces
            """
c a (function
    | a -> 12)
"""

    [<Fact>]
    let ``AppWithMatchLambdaExpr expression with strings``() =
        Oak() { AnonymousModule() { AppWithMatchLambdaExpr("c", [ "a" ], [ MatchClauseExpr("a", "12") ]) } }
        |> produces
            """
c a (function
    | a -> 12)
"""
