namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module QuoteExpr =

    [<Fact>]
    let ``let value with a QuoteExpr pattern``() =
        Oak() {
            AnonymousModule() {
                Value(QuoteExprPat(ConstantExpr(Constant(Unquoted "345"))), ConstantExpr(Constant(Unquoted "12")))
            }
        }
        |> produces
            """
let <@ 345 @> = 12
"""
