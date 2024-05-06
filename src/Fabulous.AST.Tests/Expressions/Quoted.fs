namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Quoted =
    [<Fact>]
    let ``let value with a Quoted expression``() =
        Oak() { AnonymousModule() { Value(ConstantPat(Constant("x")), QuotedExpr(ConstantExpr(Int(12)))) } }
        |> produces
            """

let x = <@ 12 @>
"""
