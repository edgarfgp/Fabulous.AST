namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module InfixApp =

    [<Fact>]
    let ``let value with a InfixApp expression``() =
        Oak() {
            AnonymousModule() {
                InfixAppExpr(ConstantExpr(Constant("a")), "|>", ConstantExpr(Constant("b")))

            }
        }
        |> produces
            """

a |> b
"""
