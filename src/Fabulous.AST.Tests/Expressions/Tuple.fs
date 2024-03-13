namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Tuple =

    [<Fact>]
    let ``let value with a Tuple expression``() =
        AnonymousModule() {
            Value(
                "x",
                TupleExpr() {
                    ConstantExpr("1").hasQuotes(false)
                    ConstantExpr("2").hasQuotes(false)
                    ConstantExpr("3").hasQuotes(false)
                }
            )
        }
        |> produces
            """

let x = 1, 2, 3
"""

    [<Fact>]
    let ``let value with a Tuple expression with parenthesis``() =
        AnonymousModule() {
            Value(
                "x",
                ParenExpr(
                    TupleExpr() {
                        ConstantExpr("1").hasQuotes(false)
                        ConstantExpr("2").hasQuotes(false)
                        ConstantExpr("3").hasQuotes(false)
                    }
                )
            )
        }
        |> produces
            """

let x = (1, 2, 3)
"""
