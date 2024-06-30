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
                    ConstantPat(Constant("x")),
                    StructTupleExpr([ ConstantExpr(Int 1); ConstantExpr(Int 2); ConstantExpr(Int 3) ])
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
                    ConstantPat(Constant("x")),
                    ParenExpr(StructTupleExpr([ ConstantExpr(Int 1); ConstantExpr(Int 2); ConstantExpr(Int 3) ]))
                )
            }
        }
        |> produces
            """

let x = (struct (1, 2, 3))
"""
