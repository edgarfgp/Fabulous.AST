namespace Fabulous.AST.Tests.Expressions

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module StructTuple =

    [<Test>]
    let ``let value with a StructTuple expression`` () =
        AnonymousModule() {
            Value(
                "x",
                StructTupleExpr() {
                    ConstantExpr("1")
                    ConstantExpr("2")
                    ConstantExpr("3")
                }
            )
        }
        |> produces
            """

let x = struct (1, 2, 3)
"""

    [<Test>]
    let ``let value with a StructTuple expression with parenthesis`` () =
        AnonymousModule() {
            Value(
                "x",
                ParenExpr(
                    StructTupleExpr() {
                        ConstantExpr("1")
                        ConstantExpr("2")
                        ConstantExpr("3")
                    }
                )
            )
        }
        |> produces
            """

let x = (struct (1, 2, 3))
"""
