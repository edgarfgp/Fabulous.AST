namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module ListCons =

    [<Fact>]
    let ``let value with a ListCons pattern``() =
        Oak() { AnonymousModule() { Value(ListConsPat(NamedPat("a"), NamedPat("b")), ConstantExpr(Int(12))) } }
        |> produces
            """
let a :: b = 12
"""
