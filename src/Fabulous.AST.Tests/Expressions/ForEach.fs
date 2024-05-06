namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module ForEach =

    [<Fact>]
    let ``let value with a ForEach expression using do``() =
        Oak() {
            AnonymousModule() {
                ForEachExpr(ConstantPat(Constant("i")), ConstantExpr(Constant("0..9")), ConstantExpr(Constant("i")))
            }
        }
        |> produces
            """
for i in 0..9 do
    i
"""

    [<Fact>]
    let ``let value with a ForEach IndexRange expression``() =
        Oak() {
            AnonymousModule() {
                ForEachExpr(
                    ConstantPat(Constant("i")),
                    IndexRangeExpr(ConstantExpr(Int(0)), ConstantExpr(Int(9))),
                    ConstantExpr(Constant("i"))
                )
            }
        }
        |> produces
            """
for i in 0..9 do
    i
"""

    [<Fact>]
    let ``let value with a ForEach expression using do ignore``() =
        Oak() {
            AnonymousModule() {
                ForEachExpr(
                    ConstantPat(Constant("i")),
                    ConstantExpr(Constant("0..9")),
                    InfixAppExpr(ConstantExpr(Constant("i")), "|>", ConstantExpr(Constant("ignore")))
                )
            }
        }
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
                    ConstantPat(Constant("i")),
                    ConstantExpr(Constant("0..9")),
                    CompExprBodyExpr(
                        [ OtherExpr(AppExpr(ConstantExpr(Constant("printfn")), ConstantExpr(String(""))))
                          OtherExpr(AppExpr(ConstantExpr(Constant("printfn")), ConstantExpr(String(""))))
                          OtherExpr(InfixAppExpr(ConstantExpr(Constant("i")), "|>", ConstantExpr(Constant("ignore")))) ]
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
        Oak() {
            AnonymousModule() {
                ForEachExpr(ConstantPat(Constant("i")), ConstantExpr(Constant("0..9")), ConstantExpr(Constant("i")))
                    .useArrow()
            }
        }
        |> produces
            """
for i in 0..9 -> i
"""
