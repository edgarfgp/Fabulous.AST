namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module NamedParenStarIdent =

    [<Fact>]
    let ``let value with a NamedParenStarIdent pattern`` () =
        AnonymousModule() { Value(NamedParenStarIdentPat("a"), ConstantExpr(Constant("12", false))) }
        |> produces
            """
let ( a ) = 12
"""
