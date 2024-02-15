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
                    ConstantExpr(ConstantString "1")
                    ConstantExpr(ConstantString "2")
                    ConstantExpr(ConstantString "3")
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
                        ConstantExpr(ConstantString "1")
                        ConstantExpr(ConstantString "2")
                        ConstantExpr(ConstantString "3")
                    }
                )
            )
        }
        |> produces
            """

let x = (struct (1, 2, 3))
"""
