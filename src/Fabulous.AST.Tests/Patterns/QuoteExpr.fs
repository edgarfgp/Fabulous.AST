namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module QuoteExpr =

    [<Fact>]
    let ``let value with a QuoteExpr pattern``() =
        Oak() { AnonymousModule() { Value(QuoteExprPat(ConstantExpr(Int(345))), ConstantExpr(Int(12))) } }
        |> produces
            """
let <@ 345 @> = 12
"""
