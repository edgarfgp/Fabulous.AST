namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module OptVar =

    [<Fact>]
    let ``OptVarExpr with identifier``() =
        Oak() { AnonymousModule() { Value("x", OptVarExpr("myVar")) } }
        |> producesValid
            """
let x = myVar
"""

    [<Fact>]
    let ``OptVarExpr with isOptional false``() =
        Oak() { AnonymousModule() { Value("x", OptVarExpr("myVar", false)) } }
        |> producesValid
            """
let x = myVar
"""

    [<Fact>]
    let ``OptVarExpr with isOptional true``() =
        Oak() { AnonymousModule() { Value("x", OptVarExpr("myVar", true)) } }
        |> producesValid
            """
let x = ?myVar
"""
