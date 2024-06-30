namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module DotNamedIndexedPropertySet =

    [<Fact>]
    let ``DotNamedIndexedPropertySet expression``() =
        Oak() {
            AnonymousModule() {
                DotNamedIndexedPropertySetExpr("a", "b", "c", "d")
                DotNamedIndexedPropertySetExpr(IdentExpr("a"), "b", IdentExpr("c"), IdentExpr("d"))
                DotNamedIndexedPropertySetExpr(IdentExpr("a"), "b", IdentExpr("c"), "d")

                DotNamedIndexedPropertySetExpr(IdentExpr("a"), "b", "c", IdentExpr("d"))

                DotNamedIndexedPropertySetExpr("a", "b", IdentExpr("c"), IdentExpr("d"))

                DotNamedIndexedPropertySetExpr("a", "b", "c", IdentExpr("d"))

                DotNamedIndexedPropertySetExpr(Int(1), "b", Constant("c"), IdentExpr("d"))

            }
        }
        |> produces
            """
a.bc <- d
a.bc <- d
a.bc <- d
a.bc <- d
a.bc <- d
a.bc <- d
1.bc <- d
"""
