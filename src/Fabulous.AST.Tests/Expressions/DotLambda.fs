namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module DotLambda =

    [<Fact>]
    let ``let value with a DotLambda expression``() =
        Oak() { AnonymousModule() { DotLambdaExpr(ConstantExpr("x")) } }
        |> produces
            """

_.x
"""

    [<Fact>]
    let ``let value with a DotLambda constant widgets``() =
        Oak() {
            AnonymousModule() {
                DotLambdaExpr(Constant("x"))
                DotLambdaExpr(String("x"))
            }
        }
        |> produces
            """
_.x
_."x"
"""

    [<Fact>]
    let ``let value with a DotLambda string``() =
        Oak() { AnonymousModule() { DotLambdaExpr("x") } }
        |> produces
            """
_.x
"""
