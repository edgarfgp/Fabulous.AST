namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module NamedParenStarIdent =

    [<Fact>]
    let ``let value with a NamedParenStarIdent pattern``() =
        Oak() { AnonymousModule() { Value(NamedParenStarIdentPat("a"), ConstantExpr(Int(12))) } }
        |> produces
            """
let ( a ) = 12
"""

    [<Fact>]
    let ``let value with a NamedParenStarIdent pattern with private accessibility``() =
        Oak() { AnonymousModule() { Value(NamedParenStarIdentPat("a").toPrivate(), ConstantExpr(Int(12))) } }
        |> produces
            """
let private ( a ) = 12
"""
