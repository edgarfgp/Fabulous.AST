namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module TuplePat =

    [<Fact>]
    let ``let value with a Tuple pattern``() =
        AnonymousModule() {
            Value(
                TuplePat() {
                    NamedPat("a")
                    NamedPat("b")
                },
                ConstantExpr(Constant("12", false))
            )
        }
        |> produces
            """

let a, b = 12
"""
