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
                InfixAppExpr(ConstantExpr(Constant(Unquoted "a")), "|>", ConstantExpr(Constant(Unquoted "b")))

            }
        }
        |> produces
            """

a |> b
"""
