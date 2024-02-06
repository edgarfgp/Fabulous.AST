namespace Fabulous.AST.Tests.Patterns

open NUnit.Framework
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module NamedParenStarIdent =

    [<Test>]
    let ``let value with a NamedParenStarIdent pattern`` () =
        AnonymousModule() { Value(NamedParenStarIdentPat("a"), ConstantExpr("12")) }
        |> produces
            """
let ( a ) = 12
"""
