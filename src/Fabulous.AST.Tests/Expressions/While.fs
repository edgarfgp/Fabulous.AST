namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module While =

    [<Fact>]
    let ``let value with a WhileExpr expression``() =
        Oak() { AnonymousModule() { WhileExpr("true", "0") } }
        |> produces
            """
while true do
    0
"""

    [<Fact>]
    let ``let value with a WhileExpr expression using do ignore``() =
        Oak() { AnonymousModule() { WhileExpr("true", InfixAppExpr(Unquoted("0"), "|>", Unquoted("ignore"))) } }
        |> produces
            """
while true do
    0 |> ignore
"""

    [<Fact>]
    let ``let value with a WhileExpr comp body expression using do ignore``() =
        Oak() {
            AnonymousModule() {
                WhileExpr(
                    "true",
                    CompExprBodyExpr(
                        [ OtherExpr(AppExpr("printfn", ConstantExpr(DoubleQuoted(""))))
                          OtherExpr(AppExpr("printfn", ConstantExpr(DoubleQuoted(""))))
                          OtherExpr(InfixAppExpr(Unquoted("0"), "|>", Unquoted("ignore"))) ]
                    )
                )
            }
        }
        |> produces
            """
while true do
    printfn ""
    printfn ""
    0 |> ignore
"""
