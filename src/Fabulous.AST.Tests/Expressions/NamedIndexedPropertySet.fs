namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module NamedIndexedPropertySet =

    [<Fact>]
    let ``NamedIndexedPropertySet expression``() =
        Oak() {
            AnonymousModule() {
                NamedIndexedPropertySetExpr("a", "c", "d")
                NamedIndexedPropertySetExpr("a", IdentExpr("c"), IdentExpr("d"))
                NamedIndexedPropertySetExpr("a", IdentExpr("c"), "d")

                NamedIndexedPropertySetExpr("a", "c", IdentExpr("d"))

                NamedIndexedPropertySetExpr("a", IdentExpr("c"), IdentExpr("d"))

                NamedIndexedPropertySetExpr("a", "b", IdentExpr("d"))

                NamedIndexedPropertySetExpr("1", Constant("c"), Constant("d"))

            }
        }
        |> produces
            """
a c <- d
a c <- d
a c <- d
a c <- d
a c <- d
a b <- d
1 c <- d
"""
