namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module As =

    [<Fact>]
    let ``let value with a As pattern``() =
        Oak() { AnonymousModule() { Value(AsPat(NamedPat("A"), NamedPat("B")), ConstantExpr(Int(12))) } }
        |> produces
            """

let A as B = 12
"""
