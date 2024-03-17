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
                    "x",
                    ArrayExpr() {
                        ConstantExpr(Unquoted "1")
                        ConstantExpr(Unquoted "2")
                        ConstantExpr(Unquoted "3")
                    }
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
                    "x",
                    ListExpr() {
                        ConstantExpr(Unquoted "1")
                        ConstantExpr(Unquoted "2")
                        ConstantExpr(Unquoted "3")
                    }
                )
            }
        }
        |> produces
            """

let x = [ 1; 2; 3 ]
"""
