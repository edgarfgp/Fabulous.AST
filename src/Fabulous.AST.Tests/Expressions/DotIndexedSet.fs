namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module DotIndexedSet =

    [<Fact>]
    let ``DotIndexedSet expression``() =
        Oak() {
            AnonymousModule() {
                DotIndexedSetExpr("a", "c", "d")
                DotIndexedSetExpr(IdentExpr("a"), IdentExpr("c"), IdentExpr("d"))
                DotIndexedSetExpr(IdentExpr("a"), IdentExpr("c"), "d")

                DotIndexedSetExpr(IdentExpr("a"), "c", IdentExpr("d"))

                DotIndexedSetExpr("a", IdentExpr("c"), IdentExpr("d"))

                DotIndexedSetExpr("a", "b", IdentExpr("d"))

                DotIndexedSetExpr(Int(1), Constant("c"), IdentExpr("d"))

                DotIndexedSetExpr(Int(1), Constant("c"), Constant("d"))

            }
        }
        |> produces
            """
a.[c] <- d
a.[c] <- d
a.[c] <- d
a.[c] <- d
a.[c] <- d
a.[b] <- d
1.[c] <- d
1.[c] <- d
"""
