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

    [<Fact>]
    let ``match! and while! inside a computation expression``() =
        Oak() {
            AnonymousModule() {
                Value(
                    "run",
                    NamedComputationExpr(
                        "task",
                        CompExprBodyExpr(
                            [ OtherExpr(WhileBangExpr("reader.ReadAsync()", AppExpr("handle", "reader")))
                              OtherExpr(
                                  MatchBangExpr(
                                      "fetch ()",
                                      [ MatchClauseExpr(LongIdentPat("Some", NamedPat("x")), "x")
                                        MatchClauseExpr(LongIdentPat("None"), Int(0)) ]
                                  )
                              ) ]
                        )
                    )
                )
            }
        }
        |> producesValid
            """
let run =
    task {
        while! reader.ReadAsync() do
            handle reader

        match! fetch () with
        | Some x -> x
        | None -> 0
    }
"""

    [<Fact>]
    let ``withIn ends bindings with in``() =
        Oak() {
            AnonymousModule() {
                Value(
                    "run",
                    NamedComputationExpr(
                        "async",
                        CompExprBodyExpr(
                            [ LetOrUseBangExpr("x", "fetch ()").withIn()
                              LetOrUseExpr(Value("y", "x")).withIn()
                              OtherExpr(SingleExpr("return", "y")) ]
                        )
                    )
                )
            }
        }
        |> producesValid
            """
let run =
    async {
        let! x = fetch () in
        let y = x in
        return y
    }
"""
