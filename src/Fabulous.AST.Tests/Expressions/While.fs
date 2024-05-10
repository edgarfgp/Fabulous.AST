namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module While =

    [<Fact>]
    let ``let value with a WhileExpr expression``() =
        Oak() { AnonymousModule() { WhileExpr(ConstantExpr(Bool(true)), ConstantExpr(Int(0))) } }
        |> produces
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
                    ConstantExpr(Bool(true)),
                    CompExprBodyExpr(
                        [ OtherExpr(AppExpr(ConstantExpr(Constant("printfn")), ConstantExpr(String(""))))
                          OtherExpr(AppExpr(ConstantExpr(Constant("printfn")), ConstantExpr(String(""))))
                          OtherExpr(InfixAppExpr(ConstantExpr(Int(0)), "|>", ConstantExpr(Constant("ignore")))) ]
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
