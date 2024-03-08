namespace Fabulous.AST.Tests.Expressions

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Paren =
    [<Test>]
    let ``let value with a expression wrapped parenthesis`` () =
        AnonymousModule() { Value("x", ParenExpr(ConstantExpr(Constant("12", false)))) }
        |> produces
            """

let x = (12)
"""
