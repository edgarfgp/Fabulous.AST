namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Tuple =

    [<Fact>]
    let ``let value with a Tuple expression``() =
        Oak() {
            AnonymousModule() {
                Value(
                    "x",
                    TupleExpr(
                        [ ConstantExpr(Unquoted "1")
                          ConstantExpr(Unquoted "2")
                          ConstantExpr(Unquoted "3") ]
                    )
                )
            }
        }
        |> produces
            """

let x = 1, 2, 3
"""

    [<Fact>]
    let ``let value with a Tuple expression with parenthesis``() =
        Oak() {
            AnonymousModule() {
                Value(
                    "x",
                    ParenExpr(
                        TupleExpr(
                            [ ConstantExpr(Unquoted "1")
                              ConstantExpr(Unquoted "2")
                              ConstantExpr(Unquoted "3") ]
                        )
                    )
                )
            }
        }
        |> produces
            """

let x = (1, 2, 3)
"""
