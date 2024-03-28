namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module NamedComputation =

    [<Fact>]
    let ``let value with a NamedComputationExpr expression``() =
        Oak() { AnonymousModule() { NamedComputationExpr(ConstantExpr(Unquoted "task"), ConstantExpr(Quoted "a")) } }
        |> produces
            """
task { "a" }
"""
