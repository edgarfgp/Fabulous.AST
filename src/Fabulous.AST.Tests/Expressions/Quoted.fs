namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Quoted =
    [<Fact>]
    let ``let value with a Quoted expression``() =
        Oak() { AnonymousModule() { Value("x", QuotedExpr(ConstantExpr(Constant(Unquoted "12")))) } }
        |> produces
            """

let x = <@ 12 @>
"""
