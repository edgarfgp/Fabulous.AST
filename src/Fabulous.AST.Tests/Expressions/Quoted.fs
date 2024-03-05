namespace Fabulous.AST.Tests.Expressions

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Quoted =
    [<Test>]
    let ``let value with a Quoted expression`` () =
        AnonymousModule() { Value("x", QuotedExpr(ConstantExpr(Constant "12"))) }
        |> produces
            """

let x = <@ 12 @>
"""
