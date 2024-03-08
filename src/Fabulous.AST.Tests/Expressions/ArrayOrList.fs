namespace Fabulous.AST.Tests.Expressions

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module ArrayOrList =

    [<Test>]
    let ``let value with a Array expression`` () =
        AnonymousModule() {
            Value(
                "x",
                ArrayExpr() {
                    ConstantExpr("1", false)
                    ConstantExpr("2", false)
                    ConstantExpr("3", false)
                }
            )
        }
        |> produces
            """

let x = [| 1; 2; 3 |]
"""

    [<Test>]
    let ``let value with a List expression`` () =
        AnonymousModule() {
            Value(
                "x",
                ListExpr() {
                    ConstantExpr("1", false)
                    ConstantExpr("2", false)
                    ConstantExpr("3", false)
                }
            )
        }
        |> produces
            """
    
let x = [ 1; 2; 3 ]
"""
