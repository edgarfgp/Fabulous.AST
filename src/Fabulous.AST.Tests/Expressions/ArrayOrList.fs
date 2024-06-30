namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module ArrayOrList =

    [<Fact>]
    let ``let value with a Array expression``() =
        Oak() {
            AnonymousModule() {
                Value(
                    ConstantPat(Constant("x")),
                    ArrayExpr([ ConstantExpr(Int 1); ConstantExpr(Int 2); ConstantExpr(Int 3) ])
                )
            }
        }
        |> produces
            """

let x = [| 1; 2; 3 |]
"""

    [<Fact>]
    let ``let value with a List expression``() =
        Oak() {
            AnonymousModule() {
                Value(
                    ConstantPat(Constant("x")),
                    ListExpr([ ConstantExpr(Int 1); ConstantExpr(Int 2); ConstantExpr(Int 3) ])
                )
            }
        }
        |> produces
            """

let x = [ 1; 2; 3 ]
"""
