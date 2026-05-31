namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module While =

    [<Fact>]
    let ``let value with a WhileExpr expression``() =
        Oak() { AnonymousModule() { WhileExpr(ConstantExpr(Bool(true)), ConstantExpr(Int(0))) } }
        |> producesValid
            """
while true do
    0
"""

    [<Fact>]
    let ``let value with a WhileExpr expression using do ignore``() =
        Oak() {
            AnonymousModule() {
                WhileExpr(
                    ConstantExpr(Bool(true)),
                    InfixAppExpr(ConstantExpr(Int(0)), "|>", ConstantExpr(Constant("ignore")))
                )
            }
        }
        |> producesValid
            """
while true do
    0 |> ignore
"""

    [<Fact>]
    let ``let value with a WhileExpr comp body expression using do ignore``() =
        Oak() {
            AnonymousModule() {
                WhileExpr(
                    ConstantExpr(Bool(true)),
                    CompExprBodyExpr(
                        [ OtherExpr(AppExpr(ConstantExpr(Constant("printfn")), ConstantExpr(String(""))))
                          OtherExpr(AppExpr(ConstantExpr(Constant("printfn")), ConstantExpr(String(""))))
                          OtherExpr(InfixAppExpr(ConstantExpr(Int(0)), "|>", ConstantExpr(Constant("ignore")))) ]
                    )
                )
            }
        }
        |> producesValid
            """
while true do
    printfn ""
    printfn ""
    0 |> ignore
"""

    [<Fact>]
    let ``let value with a WhileExpr comp body string expression using do ignore``() =
        Oak() {
            AnonymousModule() {
                WhileExpr(ConstantExpr(Bool(true)), CompExprBodyExpr([ "printfn \"\""; "printfn \"\""; "0 |> ignore" ]))
            }
        }
        |> producesValid
            """
while true do
    printfn ""
    printfn ""
    0 |> ignore
"""

    [<Fact>]
    let ``WhileExpr with Constant condition and Expr body``() =
        Oak() { AnonymousModule() { WhileExpr(Bool(true), AppExpr("doSomething", ConstantExpr("()"))) } }
        |> producesValid
            """
while true do
    doSomething ()
"""

    [<Fact>]
    let ``WhileExpr with string condition and Expr body``() =
        Oak() { AnonymousModule() { WhileExpr("condition()", AppExpr("doSomething", ConstantExpr("()"))) } }
        |> producesValid
            """
while condition() do
    doSomething ()
"""

    [<Fact>]
    let ``WhileExpr with Expr condition and Constant body``() =
        Oak() { AnonymousModule() { WhileExpr(AppExpr("shouldContinue", ConstantExpr("()")), Int(0)) } }
        |> producesValid
            """
while shouldContinue () do
    0
"""

    [<Fact>]
    let ``WhileExpr with Expr condition and string body``() =
        Oak() { AnonymousModule() { WhileExpr(AppExpr("shouldContinue", ConstantExpr("()")), "processNext()") } }
        |> producesValid
            """
while shouldContinue () do
    processNext()
"""
