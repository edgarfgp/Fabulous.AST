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
                Value(
                    QuoteExprPat(ConstantExpr(Constant("345").hasQuotes(false))),
                    ConstantExpr(Constant("12").hasQuotes(false))
                )
            }
        }
        |> produces
            """
let <@ 345 @> = 12
"""
