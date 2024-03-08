namespace Fabulous.AST.Tests.Expressions

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Typed =
    [<Test>]
    let ``let value with a typed expression`` () =
        AnonymousModule() { Value("x", TypedExpr(ConstantExpr(Constant("2", false)), ":", LongIdent("string"))) }
        |> produces
            """

let x = 2: string
"""
