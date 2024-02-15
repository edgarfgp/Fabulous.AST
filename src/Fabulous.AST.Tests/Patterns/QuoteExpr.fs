namespace Fabulous.AST.Tests.Patterns

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module QuoteExpr =

    [<Test>]
    let ``let value with a QuoteExpr pattern`` () =
        AnonymousModule() { Value(QuoteExprPat(ConstantExpr(ConstantString "345")), ConstantExpr(ConstantString "12")) }
        |> produces
            """
let <@ 345 @> = 12
"""
