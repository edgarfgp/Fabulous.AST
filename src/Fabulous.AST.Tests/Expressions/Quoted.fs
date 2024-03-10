namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Quoted =
    [<Fact>]
    let ``let value with a Quoted expression`` () =
        AnonymousModule() { Value("x", QuotedExpr(ConstantExpr(Constant("12", false)))) }
        |> produces
            """

let x = <@ 12 @>
"""
