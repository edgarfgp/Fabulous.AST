namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Chain =

    [<Fact>]
    let ``a Chain expression must start with a receiver``() =
        // Fantomas 8 models a chain as a receiver followed by dotted steps, so a chain that
        // is only a dot has no representation; ChainExpr rejects it with a clear message.
        let ex =
            Assert.Throws<System.ArgumentException>(fun () ->
                Oak() { AnonymousModule() { ChainExpr(ChainLinkDot()) } } |> Gen.mkOak |> ignore)

        Assert.Contains("starts with a dot", ex.Message)

    [<Fact>]
    let ``let value with a Chain expression with multiple links``() =
        Oak() {
            AnonymousModule() {
                ChainExpr(
                    [ ChainLinkIdentifier("A")
                      ChainLinkDot()
                      ChainLinkExpr(ConstantExpr("B"))
                      ChainLinkDot()
                      ChainLinkAppParen("C", ConstantExpr("D"))
                      ChainLinkDot()
                      ChainLinkAppUnit("E")
                      ChainLinkDot()
                      ChainLinkAppParen("F", ConstantExpr("G"))
                      ChainLinkDot()
                      ChainLinkIndex(ConstantExpr("H")) ]
                )
            }
        }
        |> producesValid
            """
A.B.C(D).E().F(G).[H]
"""
