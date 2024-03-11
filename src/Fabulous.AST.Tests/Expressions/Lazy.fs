namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Lazy =

    [<Fact>]
    let ``let value with a lazy expression``() =
        AnonymousModule() { Value("x", "lazy 12", false) }
        |> produces
            """

let x = lazy 12
"""

    [<Fact>]
    let ``let value with a lazy expression widgets``() =
        AnonymousModule() { Value("x", LazyExpr(ConstantExpr(Constant("12", false)))) }
        |> produces
            """

let x = lazy 12
"""

    [<Fact>]
    let ``let value with a lazy expression in parenthesis``() =
        AnonymousModule() { Value("x", LazyExpr(ParenExpr(ConstantExpr(Constant("12", false))))) }
        |> produces
            """

let x = lazy (12)
"""
