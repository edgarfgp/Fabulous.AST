namespace Fabulous.AST.Tests.Patterns

open Xunit
open Fabulous.AST.Tests

open Fabulous.AST

open type Ast

module TuplePat =

    [<Fact>]
    let ``let value with a Tuple pattern``() =
        Oak() {
            AnonymousModule() {
                Value(
                    TuplePat() {
                        NamedPat("a")
                        NamedPat("b")
                    },
                    ConstantExpr(Constant(Unquoted "12"))
                )
            }
        }
        |> produces
            """

let a, b = 12
"""
