namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module PrefixApp =

    [<Fact>]
    let ``PrefixApp expression``() =
        Oak() {
            AnonymousModule() {
                PrefixAppExpr("a", ConstantExpr(Constant("b")))
                PrefixAppExpr("a", Constant("b"))
                PrefixAppExpr("a", "b")
            }
        }
        |> produces
            """
a b
a b
a b
"""
