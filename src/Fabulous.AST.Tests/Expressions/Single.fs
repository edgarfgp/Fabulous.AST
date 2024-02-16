namespace Fabulous.AST.Tests.Expressions

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Single =
    [<Test>]
    let ``let value with a Single expression`` () =
        AnonymousModule() { Value("x", SingleExpr("a", true, false, ConstantExpr(ConstantString "b"))) }
        |> produces
            """

let x = a b
"""
