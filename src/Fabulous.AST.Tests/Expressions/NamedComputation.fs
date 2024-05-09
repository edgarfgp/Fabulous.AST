namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module NamedComputation =

    [<Fact>]
    let ``let value with a NamedComputationExpr expression``() =
        Oak() {
            AnonymousModule() {
                NamedComputationExpr(ConstantExpr(Constant "task"), ConstantExpr(String("a")))
                NamedComputationExpr((Constant "task"), ConstantExpr(String("a")))
                NamedComputationExpr("task", ConstantExpr(String("a")))
                NamedComputationExpr("task", String("a"))
                NamedComputationExpr("task", "a")
            }
        }
        |> produces
            """
task { "a" }
task { "a" }
task { "a" }
task { "a" }
task { a }
"""
