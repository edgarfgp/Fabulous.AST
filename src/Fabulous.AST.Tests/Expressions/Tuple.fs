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
                    ConstantPat(Constant("x")),
                    TupleExpr([ ConstantExpr(Int 1); ConstantExpr(Int 2); ConstantExpr(Int 3) ])
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
                    ConstantPat(Constant("x")),
                    ParenExpr(TupleExpr([ ConstantExpr(Int 1); ConstantExpr(Int 2); ConstantExpr(Int 3) ]))
                )
            }
        }
        |> produces
            """

let x = (1, 2, 3)
"""
