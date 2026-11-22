namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module NamedParenStarIdent =

    [<Fact>]
    let ``let value with a NamedParenStarIdent pattern``() =
        Oak() { AnonymousModule() { Value(NamedParenStarIdentPat("a"), ConstantExpr(Int(12))) } }
        |> producesValid
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

    [<Fact>]
    let ``Operator names are not backticked``() =
        Oak() {
            AnonymousModule() {
                Value(NamedParenStarIdentPat("*"), Constant("fun a b -> a * b"))
                Value(NamedParenStarIdentPat("*+"), Constant("fun a b -> a * b + b"))
            }
        }
        |> producesValid
            """
let ( * ) = fun a b -> a * b
let ( *+ ) = fun a b -> a * b + b
"""
