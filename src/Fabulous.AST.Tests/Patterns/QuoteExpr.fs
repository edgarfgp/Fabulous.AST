namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module QuoteExpr =

    [<Fact>]
    let ``let value with a QuoteExpr pattern`` () =
        AnonymousModule() {
            Value(QuoteExprPat(ConstantExpr(Constant("345", false))), ConstantExpr(Constant("12", false)))
        }
        |> produces
            """
let <@ 345 @> = 12
"""
