namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Chain =

    [<Fact>]
    let ``let value with a Chain expression``() =
        Oak() { AnonymousModule() { ChainExpr(ChainLinkDot()) } }
        |> produces
            """
.
"""

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
        |> produces
            """
A.B.C(D).E().F(G).[H]
"""
