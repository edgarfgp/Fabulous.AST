namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Paren =
    [<Fact>]
    let ``let value with a expression wrapped parenthesis``() =
        Oak() { AnonymousModule() { Value("x", ParenExpr(ConstantExpr(Constant(Unquoted "12")))) } }
        |> produces
            """

let x = (12)
"""
