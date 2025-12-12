namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module OptVar =

    [<Fact>]
    let ``OptVarExpr with identifier``() =
        Oak() { AnonymousModule() { Value("x", OptVarExpr("myVar")) } }
        |> produces
            """
let x = myVar
"""

    [<Fact>]
    let ``OptVarExpr with isOptional false``() =
        Oak() { AnonymousModule() { Value("x", OptVarExpr("myVar", false)) } }
        |> produces
            """
let x = myVar
"""

    [<Fact>]
    let ``OptVarExpr with isOptional true``() =
        Oak() { AnonymousModule() { Value("x", OptVarExpr("myVar", true)) } }
        |> produces
            """
let x = ?myVar
"""
