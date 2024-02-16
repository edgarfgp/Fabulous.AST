namespace Fabulous.AST.Tests.Expressions

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Tuple =

    [<Test>]
    let ``let value with a Tuple expression`` () =
        AnonymousModule() {
            Value(
                "x",
                TupleExpr() {
                    ConstantExpr(ConstantString "1")
                    ConstantExpr(ConstantString "2")
                    ConstantExpr(ConstantString "3")
                }
            )
        }
        |> produces
            """

let x = 1, 2, 3
"""

    [<Test>]
    let ``let value with a Tuple expression with parenthesis`` () =
        AnonymousModule() {
            Value(
                "x",
                ParenExpr(
                    TupleExpr() {
                        ConstantExpr(ConstantString "1")
                        ConstantExpr(ConstantString "2")
                        ConstantExpr(ConstantString "3")
                    }
                )
            )
        }
        |> produces
            """

let x = (1, 2, 3)
"""
