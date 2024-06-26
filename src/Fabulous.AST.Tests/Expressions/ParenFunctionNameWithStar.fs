namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module ParenFunctionNameWithStar =

    [<Fact>]
    let ``ParenFunctionNameWithStar expression``() =
        Oak() { AnonymousModule() { ParenFunctionNameWithStarExpr("a") } }
        |> produces
            """
( a )
"""
