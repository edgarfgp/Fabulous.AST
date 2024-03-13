namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Paren =
    [<Fact>]
    let ``let value with a expression wrapped parenthesis``() =
        AnonymousModule() { Value("x", ParenExpr(ConstantExpr(Constant("12").hasQuotes(false)))) }
        |> produces
            """

let x = (12)
"""
