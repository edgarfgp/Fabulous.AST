namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module StructTuple =

    [<Fact>]
    let ``let value with a StructTuple expression``() =
        Oak() {
            AnonymousModule() {
                Value(
                    "x",
                    StructTupleExpr() {
                        ConstantExpr(Unquoted "1")
                        ConstantExpr(Unquoted "2")
                        ConstantExpr(Unquoted "3")
                    }
                )
            }
        }
        |> produces
            """

let x = struct (1, 2, 3)
"""

    [<Fact>]
    let ``let value with a StructTuple expression with parenthesis``() =
        Oak() {
            AnonymousModule() {
                Value(
                    "x",
                    ParenExpr(
                        StructTupleExpr() {
                            ConstantExpr(Unquoted "1")
                            ConstantExpr(Unquoted "2")
                            ConstantExpr(Unquoted "3")
                        }
                    )
                )
            }
        }
        |> produces
            """

let x = (struct (1, 2, 3))
"""
