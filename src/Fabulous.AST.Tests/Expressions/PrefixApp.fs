namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module PrefixApp =

    // See https://github.com/fsprojects/fantomas/issues/2998
    [<Fact>]
    let ``PrefixApp expression``() =
        Oak() {
            AnonymousModule() {
                PrefixAppExpr("?getPropertyValue", ConstantExpr(Constant("--statusBarHeight")))
                PrefixAppExpr("?getPropertyValue", Constant("--statusBarHeight"))
                PrefixAppExpr("?getPropertyValue", "--statusBarHeight")
            }
        }
        |> produces
            """
?getPropertyValue --statusBarHeight
?getPropertyValue --statusBarHeight
?getPropertyValue --statusBarHeight
"""
