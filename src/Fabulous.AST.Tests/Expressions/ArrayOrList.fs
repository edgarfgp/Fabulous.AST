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
                        ConstantExpr("1").hasQuotes(false)
                        ConstantExpr("2").hasQuotes(false)
                        ConstantExpr("3").hasQuotes(false)
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
                        ConstantExpr("1").hasQuotes(false)
                        ConstantExpr("2").hasQuotes(false)
                        ConstantExpr("3").hasQuotes(false)
                    }
                )
            }
        }
        |> produces
            """

let x = [ 1; 2; 3 ]
"""
