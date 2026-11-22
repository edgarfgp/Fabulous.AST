namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Lambda =

    [<Fact>]
    let ``let value with a Lambda expression``() =
        Oak() { AnonymousModule() { LambdaExpr([ "a" ], "a") } }
        |> producesValid
            """
fun a -> a
"""

    [<Fact>]
    let ``let value with a Lambda expression with a constant``() =
        Oak() { AnonymousModule() { LambdaExpr([ Constant("a") ], Int(1)) } }
        |> producesValid
            """
fun a -> 1
"""

    [<Fact>]
    let ``let value with a Lambda expression with a constant list``() =
        Oak() { AnonymousModule() { LambdaExpr([ Constant("a"); Constant("b") ], Int(1)) } }
        |> producesValid
            """
fun a b -> 1
"""

    [<Fact>]
    let ``let value with a Lambda expression with a constant list and a constant``() =
        Oak() { AnonymousModule() { LambdaExpr([ Constant("a"); Constant("b") ], Constant("c")) } }
        |> producesValid
            """
fun a b -> c
"""

    [<Fact>]
    let ``Typed and tuple lambda parameters are parenthesized``() =
        Oak() {
            AnonymousModule() {
                Value("add", LambdaExpr([ ParameterPat("a", Int()); ParameterPat("b", Int()) ], Constant("a + b")))
                Value("first", LambdaExpr(TuplePat([ NamedPat("a"); NamedPat("_") ]), Constant("a")))
            }
        }
        |> producesValid
            """
let add = fun (a: int) (b: int) -> a + b
let first = fun (a, _) -> a
"""
