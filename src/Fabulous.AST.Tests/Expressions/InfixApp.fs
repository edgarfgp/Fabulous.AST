namespace Fabulous.AST.Tests.Expressions

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module InfixApp =

    [<Test>]
    let ``let value with a InfixApp expression`` () =
        AnonymousModule() { InfixAppExpr(ConstantExpr(Constant("a", false)), "|>", ConstantExpr(Constant("b", false))) }
        |> produces
            """

a |> b
"""
