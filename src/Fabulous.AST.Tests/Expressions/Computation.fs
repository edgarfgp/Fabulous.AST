namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Computation =

    [<Fact>]
    let ``let value with a Computation expression``() =
        Oak() {
            AnonymousModule() {
                ComputationExpr(ConstantExpr(String("a")))
                ComputationExpr(String("a"))
                ComputationExpr "12"
            }
        }
        |> produces
            """
{ "a" }
{ "a" }
{ 12 }
"""
