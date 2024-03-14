namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Lazy =

    [<Fact>]
    let ``let value with a lazy expression``() =
        Oak() { AnonymousModule() { Value("x", "lazy 12").hasQuotes(false) } }
        |> produces
            """

let x = lazy 12
"""

    [<Fact>]
    let ``let value with a lazy expression widgets``() =
        Oak() { AnonymousModule() { Value("x", LazyExpr(ConstantExpr(Constant("12").hasQuotes(false)))) } }
        |> produces
            """

let x = lazy 12
"""

    [<Fact>]
    let ``let value with a lazy expression in parenthesis``() =
        Oak() { AnonymousModule() { Value("x", LazyExpr(ParenExpr(ConstantExpr(Constant("12").hasQuotes(false))))) } }
        |> produces
            """

let x = lazy (12)
"""
