namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module OptionalVal =

    [<Fact>]
    let ``let value with a OptionalVal pattern``() =
        Oak() { AnonymousModule() { Value(OptionalValPat("a"), ConstantExpr(Int(12))) } }
        |> produces
            """
let a = 12
"""
