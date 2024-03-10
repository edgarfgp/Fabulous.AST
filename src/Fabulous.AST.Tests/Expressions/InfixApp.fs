namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module InfixApp =

    [<Fact>]
    let ``let value with a InfixApp expression`` () =
        AnonymousModule() { InfixAppExpr(ConstantExpr(Constant("a", false)), "|>", ConstantExpr(Constant("b", false))) }
        |> produces
            """

a |> b
"""
