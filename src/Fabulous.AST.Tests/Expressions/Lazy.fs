namespace Fabulous.AST.Tests.Expressions

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Lazy =

    [<Test>]
    let ``let value with a lazy expression`` () =
        AnonymousModule() { Value("x", "lazy 12") }
        |> produces
            """

let x = lazy 12
"""

    [<Test>]
    let ``let value with a lazy expression widgets`` () =
        AnonymousModule() { Value("x", LazyExpr(ConstantExpr("12"))) }
        |> produces
            """

let x = lazy 12
"""

    [<Test>]
    let ``let value with a lazy expression in parenthesis`` () =
        AnonymousModule() { Value("x", LazyExpr(ParenExpr(ConstantExpr("12")))) }
        |> produces
            """

let x = lazy (12)
"""
