namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module Or =

    [<Fact>]
    let ``let value with a Or pattern``() =
        Oak() { AnonymousModule() { Value(OrPat(NamedPat("A"), NamedPat("B")), ConstantExpr(Int(12))) } }
        |> produces
            """

let A | B = 12
"""

    [<Fact>]
    let ``Or patterns``() =
        Oak() {
            AnonymousModule() {
                Value(OrPat(Constant("A"), Constant("B")), ConstantExpr(Int(12)))
                Value(OrPat("A", "B"), ConstantExpr(Int(12)))
            }
        }
        |> produces
            """

let A | B = 12
let A | B = 12
"""
