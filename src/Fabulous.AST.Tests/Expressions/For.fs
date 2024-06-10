namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module For =

    [<Fact>]
    let ``A simple for...to loop.``() =
        Oak() {
            AnonymousModule() { ForToExpr("i", ConstantExpr("1"), ConstantExpr("10"), ConstantExpr(ConstantUnit())) }
        }
        |> produces
            """
for i = 1 to 10 do
    ()
"""

    [<Fact>]
    let ``A simple for...to loop with constant widgets``() =
        Oak() { AnonymousModule() { ForToExpr("i", Int(1), Int(10), ConstantUnit()) } }
        |> produces
            """
for i = 1 to 10 do
    ()
"""

    [<Fact>]
    let ``A simple for...to loop with strings``() =
        Oak() { AnonymousModule() { ForToExpr("i", "1", "10", "()") } }
        |> produces
            """
for i = 1 to 10 do
    ()
"""

    [<Fact>]
    let ``A simple for...downto loop.``() =
        Oak() {
            AnonymousModule() {
                ForDownToExpr("i", ConstantExpr("1"), ConstantExpr("10"), ConstantExpr(ConstantUnit()))
            }
        }
        |> produces
            """
for i = 1 downto 10 do
    ()
"""

    [<Fact>]
    let ``A simple for...downto loop with constant widgets``() =
        Oak() { AnonymousModule() { ForDownToExpr("i", Int(1), Int(10), ConstantUnit()) } }
        |> produces
            """
for i = 1 downto 10 do
    ()
"""

    [<Fact>]
    let ``A simple for...downto loop with strings``() =
        Oak() { AnonymousModule() { ForDownToExpr("i", "1", "10", "()") } }
        |> produces
            """
for i = 1 downto 10 do
    ()
"""
