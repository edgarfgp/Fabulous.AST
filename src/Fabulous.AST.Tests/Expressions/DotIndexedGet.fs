namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module DotIndexedGet =

    [<Fact>]
    let ``DotIndexedSet expression``() =
        Oak() {
            AnonymousModule() {
                DotIndexedGetExpr("a", "d")
                DotIndexedGetExpr(IdentExpr("a"), IdentExpr("d"))
                DotIndexedGetExpr(IdentExpr("a"), "d")

                DotIndexedGetExpr(IdentExpr("a"), IdentExpr("d"))

                DotIndexedGetExpr("a", IdentExpr("d"))

                DotIndexedGetExpr(Int(1), IdentExpr("d"))

                DotIndexedGetExpr(Int(1), Constant("d"))

            }
        }
        |> produces
            """
a.[d]
a.[d]
a.[d]
a.[d]
a.[d]
1.[d]
1.[d]
"""
