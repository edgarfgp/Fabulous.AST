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

                NamedComputationExpr(
                    (Constant "task"),
                    SingleExpr(SingleNode("return", String("a")).supportsStroustrup(true))
                )

                NamedComputationExpr("task", SingleExpr(SingleNode("return", String("a")).addSpace(true)))
                NamedComputationExpr("task", String("a"))
                NamedComputationExpr("task", "a")
            }
        }
        |> producesValid
            """
task { "a" }
task { return "a" }
task { return "a" }
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
        |> producesValid
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

    [<Fact>]
    let ``use! binding in a computation expression``() =
        Oak() {
            AnonymousModule() {
                Value(
                    "read",
                    NamedComputationExpr(
                        "task",
                        CompExprBodyExpr(
                            [ UseBangExpr("stream", "openAsync path")
                              LetOrUseBangExpr("text", "readAllAsync stream")
                              OtherExpr(SingleExpr("return", "text")) ]
                        )
                    )
                )
            }
        }
        |> producesValid
            """
let read =
    task {
        use! stream = openAsync path
        let! text = readAllAsync stream
        return text
    }
"""
