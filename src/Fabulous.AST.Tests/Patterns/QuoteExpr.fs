namespace Fabulous.AST.Tests.Patterns

#nowarn "44" // QuoteExprPat is obsolete; this test keeps its output covered

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module QuoteExpr =

    [<Fact>]
    let ``let value with a QuoteExpr pattern``() =
        Oak() { AnonymousModule() { Value(QuoteExprPat(ConstantExpr(Int(345))), ConstantExpr(Int(12))) } }
        |> producesValid
            """
let <@ 345 @> = 12
"""
