namespace Fabulous.AST.Tests.Patterns

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module QuoteExpr =

    [<Test>]
    let ``let value with a QuoteExpr pattern`` () =
        AnonymousModule() { Value(QuoteExprPat(ConstantExpr("345")), ConstantExpr("12")) }
        |> produces
            """
let <@ 345 @> = 12
"""
