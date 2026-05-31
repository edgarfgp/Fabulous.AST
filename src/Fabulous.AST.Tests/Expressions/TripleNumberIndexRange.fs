namespace Fabulous.AST.Tests.Expressions

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module TripleNumberIndexRange =

    [<Fact>]
    let ``TripleNumberIndexRange expression``() =
        Oak() { AnonymousModule() { TripleNumberIndexRangeExpr("1", "2", "3") } }
        |> producesValid
            """
1..2..3
"""
