namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module ForEach =

    [<Fact>]
    let ``let value with a ForEach expression using do``() =
        Oak() { AnonymousModule() { ForEachExpr("i", "0..9", "i") } }
        |> produces
            """
for i in 0..9 do
    i
"""

    [<Fact>]
    let ``let value with a ForEach IndexRange expression``() =
        Oak() { AnonymousModule() { ForEachExpr("i", IndexRangeExpr("0", "9"), "i") } }
        |> produces
            """
for i in 0..9 do
    i
"""

    [<Fact>]
    let ``let value with a ForEach expression using do ignore``() =
        Oak() { AnonymousModule() { ForEachExpr("i", "0..9", InfixAppExpr(Unquoted("i"), "|>", Unquoted("ignore"))) } }
        |> produces
            """
for i in 0..9 do
    i |> ignore
"""

    [<Fact>]
    let ``let value with a ForEach comp body expression using do ignore``() =
        Oak() {
            AnonymousModule() {
                ForEachExpr(
                    "i",
                    "0..9",
                    CompExprBodyExpr(
                        [ OtherExpr(AppExpr("printfn", ConstantExpr(DoubleQuoted(""))))
                          OtherExpr(AppExpr("printfn", ConstantExpr(DoubleQuoted(""))))
                          OtherExpr(InfixAppExpr(Unquoted("i"), "|>", Unquoted("ignore"))) ]
                    )
                )
            }
        }
        |> produces
            """
for i in 0..9 do
    printfn ""
    printfn ""
    i |> ignore
"""

    [<Fact>]
    let ``let value with a ForEach expression using arrow``() =
        Oak() { AnonymousModule() { ForEachExpr("i", "0..9", "i").useArrow() } }
        |> produces
            """
for i in 0..9 -> i
"""
