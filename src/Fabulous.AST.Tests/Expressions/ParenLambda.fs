namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module ParenLambda =

    [<Fact>]
    let ``let value with a ParenLambda expression``() =
        Oak() {
            AnonymousModule() {
                ParenLambdaExpr([ "a" ], "a")
                ParenLambdaExpr([ "a" ], Constant "a")
                ParenLambdaExpr([ Constant("a") ], "a")
            }
        }
        |> produces
            """
(fun a -> a)
(fun a -> a)
(fun a -> a)
"""

    [<Fact>]
    let ``let value with a ParenLambda expression with a constant``() =
        Oak() { AnonymousModule() { ParenLambdaExpr([ Constant("a") ], Int(1)) } }
        |> produces
            """
(fun a -> 1)
"""

    [<Fact>]
    let ``let value with a ParenLambda expression with a constant list``() =
        Oak() { AnonymousModule() { ParenLambdaExpr([ Constant("a"); Constant("b") ], Int(1)) } }
        |> produces
            """
(fun a b -> 1)
"""

    [<Fact>]
    let ``let value with a ParenLambda expression with a constant list and a constant``() =
        Oak() { AnonymousModule() { ParenLambdaExpr([ Constant("a"); Constant("b") ], Constant("c")) } }
        |> produces
            """
(fun a b -> c)
"""
