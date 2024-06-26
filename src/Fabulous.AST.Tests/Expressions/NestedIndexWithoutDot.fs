namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module NestedIndexWithoutDot =

    [<Fact>]
    let ``NestedIndexWithoutDot expression``() =
        Oak() {
            AnonymousModule() {
                NestedIndexWithoutDotExpr("a", "c", "d")
                NestedIndexWithoutDotExpr(IdentExpr("a"), IdentExpr("c"), IdentExpr("d"))
                NestedIndexWithoutDotExpr(IdentExpr("a"), IdentExpr("c"), "d")

                NestedIndexWithoutDotExpr(IdentExpr("a"), "c", IdentExpr("d"))

                NestedIndexWithoutDotExpr("a", IdentExpr("c"), IdentExpr("d"))

                NestedIndexWithoutDotExpr("a", "b", IdentExpr("d"))

                NestedIndexWithoutDotExpr(Int(1), Constant("c"), IdentExpr("d"))
            }
        }
        |> produces
            """
a[c]d
a[c]d
a[c]d
a[c]d
a[c]d
a[b]d
1[c]d
"""
