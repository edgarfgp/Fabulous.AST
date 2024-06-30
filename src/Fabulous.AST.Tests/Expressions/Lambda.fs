namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Lambda =

    [<Fact>]
    let ``let value with a Lambda expression``() =
        Oak() { AnonymousModule() { LambdaExpr([ "a" ], "a") } }
        |> produces
            """
fun a -> a
"""

    [<Fact>]
    let ``let value with a Lambda expression with a constant``() =
        Oak() { AnonymousModule() { LambdaExpr([ Constant("a") ], Int(1)) } }
        |> produces
            """
fun a -> 1
"""

    [<Fact>]
    let ``let value with a Lambda expression with a constant list``() =
        Oak() { AnonymousModule() { LambdaExpr([ Constant("a"); Constant("b") ], Int(1)) } }
        |> produces
            """
fun a b -> 1
"""

    [<Fact>]
    let ``let value with a Lambda expression with a constant list and a constant``() =
        Oak() { AnonymousModule() { LambdaExpr([ Constant("a"); Constant("b") ], Constant("c")) } }
        |> produces
            """
fun a b -> c
"""
