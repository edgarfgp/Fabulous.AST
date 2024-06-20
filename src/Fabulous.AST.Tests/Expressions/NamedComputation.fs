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

    [<Fact>]
    let ``let value with a Seq expression``() =
        Oak() {
            AnonymousModule() {
                SeqExpr(CompExprBodyExpr([ String("a"); String("b") ]))

                SeqExpr([ IdentExpr("1"); IdentExpr("2") ])

                SeqExpr([ String("a"); String("b") ])
                SeqExpr([ "1"; "2" ])
            }
        }
        |> produces
            """
seq {
    "a"
    "b"
}

seq {
    1
    2
}

seq {
    "a"
    "b"
}

seq {
    1
    2
}
"""
